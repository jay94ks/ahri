using Ahri.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ahri.Networks.Tcp
{
    public class TcpServer<TSession> where TSession : TcpSession
    {
        private TaskCompletionSource<bool> m_Startup;

        private IPEndPoint m_EndPoint;
        private TcpListener m_Listener;
        private Task m_Loop;

        /// <summary>
        /// Initialize a new <see cref="TcpServer{TSession}"/> instance.
        /// </summary>
        /// <param name="Address"></param>
        /// <param name="Port"></param>
        public TcpServer(IPAddress Address, int Port) : this(null, Address, Port) { }

        /// <summary>
        /// Initialize a new <see cref="TcpServer{TSession}"/> instance.
        /// </summary>
        /// <param name="EndPoint"></param>
        public TcpServer(IPEndPoint EndPoint) : this(null, EndPoint) { }

        /// <summary>
        /// Initialize a new <see cref="TcpServer{TSession}"/> instance.
        /// </summary>
        /// <param name="Address"></param>
        /// <param name="Port"></param>
        public TcpServer(IServiceProvider Services, IPAddress Address, int Port)
            : this(Services, new IPEndPoint(Address, Port)) { }

        /// <summary>
        /// Initialize a new <see cref="TcpServer{TSession}"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="EndPoint"></param>
        public TcpServer(IServiceProvider Services, IPEndPoint EndPoint)
        {
            m_EndPoint = EndPoint;
            this.Services = Services ?? Shared.Null;
        }

        /// <summary>
        /// Service Provider instance.
        /// </summary>
        protected IServiceProvider Services { get; }

        /// <summary>
        /// Start the server asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> StartAsync()
        {
            lock (this)
            {
                if (m_Listener != null)
                    return false;

                m_Listener = new TcpListener(m_EndPoint);

                try { m_Listener.Start(); }
                catch
                {
                    try { m_Listener.Stop(); }
                    catch { }

                    return false;
                }

                if (m_Startup is null || m_Startup.Task.IsCompleted)
                    m_Startup = new TaskCompletionSource<bool>();

                m_Loop = RunLoop();
            }

            return await m_Startup.Task;
        }

        /// <summary>
        /// Stop the server asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            lock (this)
            {
                if (m_Loop is null || m_Loop.IsCompleted)
                    return;

                try { m_Listener.Stop(); }
                catch { }
            }

            m_Startup.TrySetResult(false);
            if (await m_Startup.Task)
                await m_Loop;

            m_Listener = null;
        }

        /// <summary>
        /// Called when the server is started.
        /// </summary>
        /// <returns></returns>
        protected virtual Task OnStartAsync() => Task.CompletedTask;

        /// <summary>
        /// Called when the server is stopped.
        /// </summary>
        /// <returns></returns>
        protected virtual Task OnStopAsync() => Task.CompletedTask;

        /// <summary>
        /// Called to configure the user-defined operations.
        /// </summary>
        /// <param name="Session"></param>
        /// <returns></returns>
        protected virtual Task OnConfigure(TSession Session) => Task.CompletedTask;

        /// <summary>
        /// Run the accept-loop.
        /// </summary>
        /// <returns></returns>
        private async Task RunLoop()
        {
            using var Cts = new CancellationTokenSource();
            if (m_Startup.TrySetResult(true))
            {
                await OnStartAsync();

                while (true)
                {
                    Socket Tcp;

                    try { Tcp = await m_Listener.AcceptSocketAsync(); }
                    catch
                    {
                        break;
                    }

                    var Scope = Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
                    var Injector = Scope.ServiceProvider.GetRequiredService<IServiceInjector>();

                    var Sess = (TSession)Injector.Create(typeof(TSession));
                    await OnConfigure(Sess);

                    if (Sess.Activate(Tcp, Scope, true, Cts.Token))
                    {
                        await Sess.ExecuteLoopAsync();
                        Sess.EnableDisposeOnLoopEnd();
                    }

                    else
                        await Sess.DisposeAsync();
                }

                Cts.Cancel();
                await OnStopAsync();
            }
        }
    }
}
