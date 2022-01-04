using Ahri.Http.Core.Actions.Internals;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Actions
{
    public class FileContent : IHttpAction
    {
        private FileInfo m_FileInfo;
        private string m_MimeType;

        /// <summary>
        /// Initialize a new <see cref="FileContent"/> instance.
        /// </summary>
        /// <param name="FileInfo"></param>
        /// <param name="MimeType"></param>
        public FileContent(FileInfo FileInfo, string MimeType = null)
        {
            if (!(m_FileInfo = FileInfo).Exists)
            {
                m_FileInfo = null;
                return;
            }

            if (string.IsNullOrWhiteSpace(m_MimeType = MimeType) && m_FileInfo.Exists)
            {
                var Extension = (m_FileInfo.Extension ?? "").TrimStart('.');
                HttpMimeTypes.Table.TryGetValue(Extension, out m_MimeType);
            }
        }

        /// <summary>
        /// Open the file by file-info instance.
        /// </summary>
        /// <returns></returns>
        private Stream OpenFile()
        {
            if (m_FileInfo != null)
            {
                try { return m_FileInfo.OpenRead(); }
                catch { }
            }

            return null;
        }

        /// <inheritdoc/>
        public Task InvokeAsync(IHttpContext Context)
        {
            var Request = Context.Request;
            var Response = Context.Response;
            var Stream = OpenFile();
            if (Stream is null)
            {
                Response.Status = 404;
                return Task.CompletedTask;
            }

            var Range = Request.Headers.Get("Range");
            if (Range != null)
            {
                var Ranges = Range
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(X => X.Trim())
                    .Select(X =>
                    {
                        var SE = X.Split('-', 2, StringSplitOptions.RemoveEmptyEntries);
                        if (SE.Length <= 1)
                            return (S: long.Parse(SE[0]), E: Stream.Length - 1);

                        var S = long.Parse(SE[0]);
                        var E = Math.Min(long.Parse(SE[1]), Stream.Length - 1);

                        return (S: S, E: E);
                    })
                    .Where(X => X.S >= 0 && X.E >= 0 && X.S <= X.E)
                    .ToArray();

                if (Ranges.Length <= 0)
                {
                    Response.Status = 400;
                    return Task.CompletedTask;
                }

                /* Multiple Range. */
                if (Ranges.Length > 1)
                    return SendMultipartRanges(Response, Ranges, Stream);

                /* Single Range. */
                var Value = Ranges.First();
                Response.Headers.Set("Content-Range", $"bytes {Value.S}-{Value.E}/{Stream.Length}");
                Stream = new RangeStream(Stream, Value.S, (Value.E + 1) - Value.S);
            }

            Response.Status = 200;
            Response.Headers.Set("Accept-Ranges", "bytes");
            Response.Headers.Set("Content-Type", m_MimeType);
            Response.Content = Stream;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Send multipart ranges to browser.
        /// </summary>
        /// <param name="Response"></param>
        /// <param name="Ranges"></param>
        /// <param name="Stream"></param>
        /// <returns></returns>
        private async Task SendMultipartRanges(IHttpResponse Response, (long S, long E)[] Ranges, Stream Stream)
        {
            var Boundary = Guid.NewGuid().ToString();
            var BoundaryEndBytes = Encoding.ASCII.GetBytes($"--{Boundary}--\r\n");

            Response.Headers.Set("Accept-Ranges", "bytes");
            Response.Headers.Set("Content-Type", $"multipart/byterange; boundary={Boundary}");

            var Buffer = new byte[8192];
            using (Stream)
            {
                foreach (var Range in Ranges)
                {
                    var Length = Math.Max((Range.E + 1) - Range.S, 0);
                    var EachBytes = Encoding.ASCII.GetBytes(
                        $"--{Boundary}\r\nContent-Type: {m_MimeType}\r\n" +
                        $"Content-Range: bytes {Range.S}-{Range.E}/{Stream.Length}\r\n\r\n");

                    /* Put boundary bytes. */
                    await Response.Content.WriteAsync(EachBytes);

                    /* Rewind to start of range. */
                    Stream.Position = Range.S;
                    while (Length > 0)
                    {
                        var Slice = (int) Math.Min(Buffer.Length, Length);
                        if (Slice <= 0) break;

                        var Read = await Stream.ReadAsync(Buffer, 0, Slice);
                        if (Read <= 0) break;

                        await Response.Content.WriteAsync(Buffer, 0, Read);
                        Length -= Read;
                    }
                }

                /* Put notification that represents end of the boundary content. */
                await Response.Content.WriteAsync(BoundaryEndBytes);
            }
        }
    }
}
