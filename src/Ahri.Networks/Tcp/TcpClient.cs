using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Ahri.Networks.Tcp
{
    public abstract class TcpClient<TSession> where TSession : TcpSession
    {
        private IServiceProvider m_Services;

        private TSession m_Session;
        private IPEndPoint m_EndPoint;

        private string m_Host;
        private int m_Port;
        private int m_Index = 0;

        /// <summary>
        /// Initialize a new <see cref="TcpClient{TSession}"/> instance.
        /// Note that in this case, the <see cref="TcpClient{TSession}"/> will connect 
        /// to host addresses choosen by round-robin from DNS host records.
        /// </summary>
        /// <param name="Host"></param>
        /// <param name="Port"></param>
        public TcpClient(string Host, int Port) : this(null, Host, Port) { }

        /// <summary>
        /// Initialize a new <see cref="TcpClient{TSession}"/> instance.
        /// </summary>
        /// <param name="Address"></param>
        /// <param name="Port"></param>
        public TcpClient(IPAddress Address, int Port) : this(null, Address, Port) { }

        /// <summary>
        /// Initialize a new <see cref="TcpClient{TSession}"/> instance.
        /// </summary>
        /// <param name="EndPoint"></param>
        public TcpClient(IPEndPoint EndPoint) : this(null, EndPoint) { }

        /// <summary>
        /// Initialize a new <see cref="TcpClient{TSession}"/> instance.
        /// Note that in this case, the <see cref="TcpClient{TSession}"/> will connect 
        /// to host addresses choosen by round-robin from DNS host records.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="Host"></param>
        /// <param name="Port"></param>
        public TcpClient(IServiceProvider Services, string Host, int Port)
        {
            m_Host = Host;
            m_Port = Port;
            m_Services = Services ?? Shared.Null;
        }

        /// <summary>
        /// Initialize a new <see cref="TcpClient{TSession}"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="Address"></param>
        /// <param name="Port"></param>
        public TcpClient(IServiceProvider Services, IPAddress Address, int Port) : this(Services, new IPEndPoint(Address, Port)) { }

        /// <summary>
        /// Initialize a new <see cref="TcpClient{TSession}"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="EndPoint"></param>
        public TcpClient(IServiceProvider Services, IPEndPoint EndPoint)
        {
            m_EndPoint = EndPoint;
            m_Services = Services ?? Shared.Null;
        }

        /// <summary>
        /// Indicates whether the session is alive or not.
        /// </summary>
        public bool IsConnected => m_Session != null && m_Session.IsAlive;

        /// <summary>
        /// Task that completed when the connection closed.
        /// </summary>
        public Task Completion => m_Session is null ? Task.CompletedTask : m_Session.Completion;

        /// <summary>
        /// Remote Endpoint.
        /// </summary>
        public EndPoint RemoteEndPoint => m_EndPoint;

        /// <summary>
        /// Connect to the remote endpoint.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        public async Task<bool> ConnectAsync(CancellationToken Token = default)
        {
            IPEndPoint Endpoint = m_EndPoint;
            IPAddress[] Addresses = null;

            if (m_Host != null)
            {
                try
                {
                    Addresses = await Dns.GetHostAddressesAsync(m_Host);
                    if (Addresses is null || Addresses.Length <= 0)
                        return false;
                }
                catch { return false; }
            }

            while(true)
            {
                if (m_Host != null)
                    Endpoint = new IPEndPoint(Addresses[m_Index], m_Port);

                var Tcp = new Socket(Endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                try { await Tcp.ConnectAsync(Endpoint, Token); }
                catch
                {
                    try { Tcp.Dispose(); }
                    catch { }

                    if (Addresses != null)
                        m_Index = (m_Index + 1) % Addresses.Length;

                    if (Token.IsCancellationRequested)
                        return false;

                    continue;
                }

                var Scope = m_Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
                var Injector = Scope.ServiceProvider.GetRequiredService<IServiceInjector>();

                m_Session = (TSession)Injector.Create(typeof(TSession));
                if (m_Session.Activate(Tcp, Scope, false, default))
                {
                    await OnConfigure(m_Session);

                    await m_Session.ExecuteLoopAsync();
                    m_Session.EnableDisposeOnLoopEnd();
                }
                else
                    await m_Session.DisposeAsync();

                return true;
            }
        }

        /// <summary>
        /// Disconnect the connected session.
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            if (m_Session != null)
                await m_Session.DisposeAsync();

            m_Session = null;
        }

        /// <summary>
        /// Called to configure the user-defined operations.
        /// </summary>
        /// <param name="Session"></param>
        /// <returns></returns>
        protected virtual Task OnConfigure(TSession Session) => Task.CompletedTask;

    }
}
