using Ahri.Core;
using Ahri.Http.Orb.Internals.Models;
using Ahri.Networks;
using Ahri.Networks.Tcp;
using Ahri.Networks.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Ahri.Http.Orb.Internals
{
    public class HttpConnection : TcpSession
    {
        private const byte CR = (byte)'\n';

        private int m_State = 0;

        private PacketFragment m_Buffer = PacketFragment.Empty;
        private Queue<string> m_ReceivedHeaders = new();

        private CancellationTokenSource m_RequestAborts;

        private long m_InputLength;
        private byte[] m_InputBoundary;
        private bool m_InputDiscards = false;

        private ChannelWriter<byte[]> m_Inputs;
        private ChannelReader<byte[]> m_Outputs;

        private ChannelStream m_OutputStream;

        private Task m_Task;
        private Task m_TaskOutputs;

        private HttpResponse m_Response;

        /// <summary>
        /// Server Instance.
        /// </summary>
        internal HttpServer Server { get; set; }

        /// <inheritdoc/>
        protected override Task OnCreateAsync() => Task.CompletedTask;

        /// <inheritdoc/>
        protected override Task OnDestroyAsync(SocketError Error)
        {
            m_Inputs?.TryComplete();

            try { m_OutputStream?.Close(); }
            catch { }

            m_RequestAborts?.Cancel();
            m_RequestAborts?.Dispose();
            m_RequestAborts = null;

            return Task.CompletedTask;
        }

        private async Task OnInternalReceiveAsync(IHttpRequest Request, IHttpResponse Response)
        {
            using var Scope = Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

            (Request as HttpRequest).Services = Scope.ServiceProvider;
            await OnReceiveAsync(Request, Response);
        }

        /// <summary>
        /// Called when the request is received successfully.
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="Response"></param>
        /// <returns></returns>
        protected virtual Task OnReceiveAsync(IHttpRequest Request, IHttpResponse Response) 
            => Server.InvokeOnRequestAsync(new HttpContext(Request, Response));

        /// <inheritdoc/>
        protected override async Task OnReceiveAsync(PacketFragment Fragment)
        {
            while (true)
            {
                var Again = false;
                var State = m_State;
                switch (State)
                {
                    case 0: /* Receiving Headers. */
                        {
                            var Cr = Fragment.IndexOf(CR);
                            if (Cr >= 0)
                            {
                                var EndOfHeaders = false;
                                try
                                {
                                    var Header = (Encoding.ASCII.GetString(PacketFragment
                                       .Join(m_Buffer, Fragment.Take(Cr + 1))
                                       .ToArray(false)) ?? "").TrimEnd('\n', '\r', ' ', '\t');

                                    if (Header.Length > 0)
                                    {
                                        m_ReceivedHeaders.Enqueue(Header);
                                        Again = true;
                                    }

                                    else
                                        EndOfHeaders = true;
                                }

                                catch { }
                                m_Buffer = PacketFragment.Empty;
                                Fragment = Fragment.Skip(Cr + 1);

                                if (EndOfHeaders)
                                {
                                    if (m_RequestAborts != null)
                                    {
                                        m_RequestAborts.Cancel();
                                        m_RequestAborts.Dispose();
                                    }

                                    Again = await ExecuteAsync();
                                }
                            }
                        }
                        break;

                    case 1: /* Receiving Request Body and Pipe to stream. */
                        if (m_InputBoundary is null)
                            Fragment = await HandleLengthInputs(Fragment);

                        else
                            Fragment = await HandleBoundaryInputs(Fragment);

                        Again = true;
                        break;

                    case 2: /* Waiting the request processing's completion. */
                        {
                            await m_Task;

                            try { m_OutputStream.Close(); }
                            catch { }

                            if (m_TaskOutputs != null)
                                await m_TaskOutputs;

                            else
                            {
                                await OnResponseAsync(m_Response, true);
                            }

                            m_State = 0;
                        }
                        break;
                }

                if (State != m_State || Again)
                    continue;

                if (Fragment.Length > 0)
                    m_Buffer = PacketFragment.Join(m_Buffer, Fragment);

                break;
            }
        }

        /// <summary>
        /// Execute the request.
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ExecuteAsync()
        {
            HttpRequest Request;
            try { Request = MakeRequest(out m_RequestAborts); }
            catch
            {
                await CloseAsync();
                Request = null;
            }

            if (Request != null)
            {
                var Response = new HttpResponse();
                var Outputs = Channel.CreateBounded<byte[]>(16);

                Response.Content = m_OutputStream = new ChannelStream(Outputs.Writer, () =>
                {
                    m_TaskOutputs = OnResponseAsync(Response, false);
                });

                m_Outputs = Outputs.Reader;
                m_Response = Response;
                m_TaskOutputs = null;

                m_Task = OnInternalReceiveAsync(Request, Response);
                m_State++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Make the <see cref="HttpRequest"/> to the <see cref="m_Request"/> field.
        /// </summary>
        /// <returns></returns>
        private HttpRequest MakeRequest(out CancellationTokenSource Cts)
        {
            var Request = new HttpRequest();
            if (m_ReceivedHeaders.Count <= 0)
                throw new InvalidOperationException();

            var MRV = m_ReceivedHeaders.Dequeue().Split(' ', 3);
            if (!(Request.Protocol = MRV[2]).StartsWith("HTTP/1."))
                throw new InvalidOperationException();

            var PQ = MRV[1].Split('?', 2);
            Request.Method = MRV[0];
            Request.PathString = PQ.First();
            Request.QueryString = PQ.Skip(1).LastOrDefault();

            long InputLength = 0;
            string BoundaryValue = null;
            while (m_ReceivedHeaders.TryDequeue(out var Header))
            {
                var KV = Header.Split(':', 2);
                if (KV.Length <= 1)
                    throw new InvalidOperationException();

                var Key = KV.First().Trim();
                var Value = KV.Last().Trim();
                Request.Headers.Add(new HttpHeader(Key, Value));

                if (Key.Equals("Content-Length"))
                    InputLength = int.Parse(Value);

                else if (Key.Equals("Content-Type") &&
                       Value.Contains("boundary=", StringComparison.OrdinalIgnoreCase))
                {
                    BoundaryValue = Value.Split(';')
                        .Where(X => X.StartsWith("boundary="))
                        .First().Split('=', 2).LastOrDefault();
                }
            }

            var Inputs = Channel.CreateBounded<byte[]>(16);
            if (BoundaryValue != null)
                m_InputBoundary = Encoding.ASCII.GetBytes($"---------{BoundaryValue}--");
            else
                m_InputBoundary = null;

            m_Inputs = Inputs.Writer;
            m_InputLength = InputLength;
            m_InputDiscards = false;

            Request.Aborted = (Cts = new()).Token;
            Request.Content = new ChannelStream(Inputs.Reader, () =>
            {
                m_InputDiscards = true;
                m_Inputs.TryComplete();
            });

            return Request;
        }

        /// <summary>
        /// Pipes the received inputs to request handler as much as its length, `Content-Length` header.
        /// </summary>
        /// <param name="Fragment"></param>
        /// <returns></returns>
        private async Task<PacketFragment> HandleLengthInputs(PacketFragment Fragment)
        {
            int Length = (int)Math.Min(m_InputLength, Fragment.Length);
            if (Length > 0)
            {
                if (!m_InputDiscards)
                {
                    await m_Inputs.WriteAsync(Fragment.Take(Length).ToArray());
                }

                m_InputLength -= Length;
                Fragment = Fragment.Skip(Length);
            }

            if (m_InputLength <= 0)
            {
                m_Inputs.TryComplete();
                m_State++;
            }

            return Fragment;
        }

        /// <summary>
        /// Pipes the received inputs to request handler until the boundary string appears.
        /// </summary>
        /// <param name="Fragment"></param>
        /// <returns></returns>
        private async Task<PacketFragment> HandleBoundaryInputs(PacketFragment Fragment)
        {
            m_Buffer = PacketFragment.Join(m_Buffer, Fragment);
            var Position = m_Buffer.IndexOf(m_InputBoundary);
            if (Position < 0)
            {
                var Length = m_Buffer.Length - m_InputBoundary.Length;
                if (Length > 0 && !m_InputDiscards)
                    await m_Inputs.WriteAsync(m_Buffer.Take(Length).ToArray());

                m_Buffer = m_Buffer.TakeLast(m_InputBoundary.Length);
            }

            else
            {
                var Length = Position + m_InputBoundary.Length;
                if (Length > 0 && !m_InputDiscards)
                    await m_Inputs.WriteAsync(m_Buffer.Take(Length).ToArray());

                Fragment = m_Buffer.Skip(Length);
                m_Buffer = PacketFragment.Empty;
                m_State++;
            }

            return Fragment;
        }

        /// <summary>
        /// Called when the response should be sent.
        /// </summary>
        /// <param name="Response"></param>
        /// <param name="Unhandled"></param>
        /// <returns></returns>
        private async Task OnResponseAsync(HttpResponse Response, bool Unhandled = false)
        {
            Func<Task> Sender = () => SendChunkedAsync(
                    () => m_Outputs.Completion.IsCompleted,
                    () => m_Outputs.ReadAsync());

            Response.Headers.RemoveAll(X =>
            {
                return X.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase)
                    || X.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase);
            });

            if (Unhandled || Response.Content == m_OutputStream)
            {
                if (!Unhandled)
                    SetChunkedEncoding(Response);

                else
                {
                    Response.Headers.Add(new HttpHeader("Content-Length", "0"));
                    Sender = () => Task.CompletedTask;
                }
            }
            else
            {
                var Content = Response.Content;

                var EndOfStream = false;
                if (!Content.CanSeek)
                {
                    Response.Headers.Add(new HttpHeader("Transfer-Encoding", "chunked"));
                    Sender = () => SendChunkedAsync(
                        () => EndOfStream,
                        async () =>
                        {
                            var Buffer = new byte[4096];
                            int Length;

                            try { Length = await Content.ReadAsync(Buffer); }
                            catch { Length = 0; }

                            if (Length != Buffer.Length)
                                Array.Resize(ref Buffer, Length);

                            if (Length <= 0)
                            {
                                try { Content.Close(); } catch { }
                                try { Content.Dispose(); } catch { }

                                EndOfStream = true;
                            }

                            return Buffer;
                        });
                }

                else
                {
                    Response.Headers.Add(new HttpHeader("Content-Length", Content.Length.ToString()));
                    Sender = async () =>
                    {
                        var Buffer = new byte[4096];

                        while (true)
                        {
                            int Length;

                            try { Length = await Content.ReadAsync(Buffer); }
                            catch { Length = 0; }

                            if (Length <= 0)
                                break;

                            await SendAsync(new PacketFragment(Buffer, 0, Length));
                        }

                        try { Content.Close(); } catch { }
                        try { Content.Dispose(); } catch { }
                    };
                }
            }

            await SendHeadersAsync(Response);
            await Sender();
        }

        /// <summary>
        /// Send the response headers to browser.
        /// </summary>
        /// <param name="Response"></param>
        /// <returns></returns>
        private async Task SendHeadersAsync(HttpResponse Response)
        {
            if (string.IsNullOrWhiteSpace(Response.Phrase))
            {
                HttpStatusCodes.Table.TryGetValue(Response.Status, out var Phrase);
                Response.Phrase = (Phrase ?? "Unknown").Trim(' ', '\t');
            }

            string Header = $"HTTP/1.1 {Response.Status} {Response.Phrase}\r\n" +
                            string.Join("\r\n", Response.Headers.Select(X => $"{X.Key}: {X.Value}")) +
                            "\r\n\r\n";

            await SendAsync(Encoding.ASCII.GetBytes(Header));
        }

        /// <summary>
        /// Set the response as chunked encoding.
        /// </summary>
        /// <param name="Response"></param>
        private static void SetChunkedEncoding(HttpResponse Response)
        {
            Response.Headers.RemoveAll(X =>
            {
                return X.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase)
                    || X.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase);
            });

            Response.Headers.Add(new HttpHeader("Transfer-Encoding", "chunked"));
        }

        /// <summary>
        /// Send the output content as chunked.
        /// </summary>
        /// <param name="Completion"></param>
        /// <param name="ReadAsync"></param>
        /// <returns></returns>
        private async Task SendChunkedAsync(Func<bool> Completion, Func<ValueTask<byte[]>> ReadAsync, bool Unhandled = false)
        {
            PacketFragment Current;
            while (!Completion())
            {
                try { Current = await ReadAsync(); }
                catch { break; }

                if (Current.Length <= 0)
                    continue;

                await SendAsync(Encoding.ASCII.GetBytes(Current.Length.ToString("x") + "\r\n"));
                await SendAsync(Current);

                await SendAsync(Encoding.ASCII.GetBytes("\r\n"));
            }

            await SendAsync(Encoding.ASCII.GetBytes("0\r\n\r\n"));
        }
    }
}
