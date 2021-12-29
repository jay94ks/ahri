using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Ahri.Networks.Tcp
{
    public abstract class TcpSession : IAsyncDisposable
    {
        private const int SIZE_RECV_BUFFER = 4096;

        private TaskCompletionSource<bool> m_Creation = new();
        private TaskCompletionSource<bool> m_Completion = new();
        private CancellationTokenSource m_Cts = null;
        private Socket m_Tcp;

        private Task m_Loop;
        private bool m_Killed = false, m_Disposed = false;
        private bool m_Activated = false;

        private EndPoint m_LocalEndPoint;
        private EndPoint m_RemoteEndPoint;

        private IServiceScope m_Scope;

        /// <summary>
        /// Activate the <see cref="TcpSession"/> instance.
        /// </summary>
        internal bool Activate(Socket Socket, IServiceScope Scope, bool IsServer, CancellationToken Token = default)
        {
            lock (this)
            {
                if (m_Disposed || m_Killed || m_Activated)
                    throw new InvalidOperationException("No activate method get called more.");

                m_Activated = true;
            }

            this.IsServer = IsServer;

            m_Scope = Scope;
            m_Tcp = Socket;
            m_Cts = CancellationTokenSource.CreateLinkedTokenSource(Token);

            try
            {
                m_LocalEndPoint = Socket.LocalEndPoint;
                m_RemoteEndPoint = Socket.RemoteEndPoint;
            }

            catch
            {
                KillSocket();
                return false;
            }

            m_Loop = RunLoop();
            return true;
        }

        internal Task<bool> ExecuteLoopAsync()
        {
            m_Loop = RunLoop();
            return m_Creation.Task;
        }

        /// <summary>
        /// Enable the automatical dispose on end of the loop.
        /// </summary>
        internal void EnableDisposeOnLoopEnd() => _ = Completion.ContinueWith(_ =>
        {
            DisposeAsync().GetAwaiter().GetResult();
        });

        /// <summary>
        /// Indicates whether the session is server-side or not.
        /// </summary>
        public bool IsServer { get; private set; }

        /// <summary>
        /// Indicates whether the session is alive or not.
        /// </summary>
        public bool IsAlive => !m_Activated || m_Loop != null && !m_Loop.IsCompleted;

        /// <summary>
        /// Task that completed when the connection closed.
        /// </summary>
        public Task Completion => m_Completion.Task;

        /// <summary>
        /// Local Endpoint.
        /// </summary>
        public EndPoint LocalEndPoint => m_LocalEndPoint ?? throw new InvalidOperationException("Session didn't be activated.");

        /// <summary>
        /// Remote Endpoint.
        /// </summary>
        public EndPoint RemoteEndPoint => m_RemoteEndPoint ?? throw new InvalidOperationException("Session didn't be activated.");

        /// <summary>
        /// Send a <see cref="PacketFragment"/> to the remote host.
        /// </summary>
        /// <param name="Fragment"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        public async Task SendAsync(PacketFragment Fragment, CancellationToken Token = default)
        {
            lock(this)
            {
                if (m_Disposed)
                    return;

                if (Token.IsCancellationRequested)
                    return;
            }

            if (!await m_Creation.Task || m_Killed)
                return;

            while (Fragment.Length > 0)
            {
                int Length;

                try { Length = await m_Tcp.SendAsync(Fragment, SocketFlags.None, m_Cts.Token); }
                catch (OperationCanceledException) { Length = 0; }
                catch (SocketException Exception)
                {
                    if (IsControlSignal(Exception.SocketErrorCode))
                        continue;

                    Length = 0;
                }

                if (Length <= 0)
                {
                    KillSocket();
                    break;
                }

                Fragment = Fragment.Skip(Length);
            }
        }

        /// <summary>
        /// Close the session immediately.
        /// </summary>
        /// <returns></returns>
        public async Task CloseAsync()
        {
            KillSocket();

            if (m_Loop != null)
                await m_Loop;
        }

        /// <summary>
        /// Called when the session has created.
        /// </summary>
        protected abstract Task OnCreateAsync();

        /// <summary>
        /// Called when the session has destroyed.
        /// </summary>
        /// <param name="Error">the reason why the session is now closing.</param>
        protected abstract Task OnDestroyAsync(SocketError Error);

        /// <summary>
        /// Called when packet fragment received.
        /// </summary>
        /// <param name="Fragment"></param>
        protected abstract Task OnReceiveAsync(PacketFragment Fragment);

        /// <summary>
        /// Test whether the <see cref="SocketError"/> is just control signal or not.
        /// </summary>
        /// <param name="Error"></param>
        /// <returns></returns>
        private static bool IsControlSignal(SocketError Error)
        {
            return  Error == SocketError.Success ||
                    Error == SocketError.Interrupted ||
                    Error == SocketError.WouldBlock ||
                    Error == SocketError.IOPending;
        }

        /// <summary>
        /// Run the loop asynchronously.
        /// </summary>
        /// <returns></returns>
        private async Task RunLoop()
        {
            var Buffer = new byte[SIZE_RECV_BUFFER];
            var Error = SocketError.Success;

            lock(this)
            {
                if (m_Disposed)
                    return;
            }

            if (m_Creation.TrySetResult(true))
            {
                await OnCreateAsync();

                try
                {
                    while (!m_Cts.IsCancellationRequested)
                    {
                        int Length;

                        try { Length = await m_Tcp.ReceiveAsync(Buffer, SocketFlags.None, m_Cts.Token); }
                        catch (OperationCanceledException) { Length = 0; }
                        catch (SocketException Exception)
                        {
                            if (IsControlSignal(Exception.SocketErrorCode))
                                continue;

                            Length = 0;
                            Error = Exception.SocketErrorCode;
                        }

                        if (Length <= 0)
                            break;

                        await OnReceiveAsync(new PacketFragment(Buffer, 0, Length));
                    }
                }

                finally { KillSocket(); }
                await OnDestroyAsync(Error);
            }
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            lock (this)
            {
                if (m_Disposed)
                    return;

                m_Disposed = true;
            }

            KillSocket();
            if (m_Loop != null)
                await m_Loop;

            try
            {
                m_Tcp?.Dispose();
            }

            catch { }
            m_Cts.Dispose();

            m_Scope?.Dispose();
            m_Scope = null;
        }

        /// <summary>
        /// Kill the socket immediately.
        /// </summary>
        /// <param name="Force"></param>
        private void KillSocket()
        {
            lock (this)
            {
                if (m_Killed)
                    return;

                m_Killed = true;
            }

            if (!m_Cts.IsCancellationRequested)
                 m_Cts.Cancel();

            m_Creation.TrySetResult(false);

            try { m_Tcp?.Close(); } 
            catch { }

            m_Completion.TrySetResult(true);
        }
    }
}
