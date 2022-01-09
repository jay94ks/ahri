using Ahri.Networks.Tcp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Ahri.Orp.Internals
{
    public class OrpClient : TcpClient<OrpConnection>
    {
        private IOrpMappings m_Mappings;
        private Func<IOrpContext, Task> m_Endpoint;
        private OrpConnection m_Session;
        private OrpClientOptions m_Options;

        private IEnumerable<Func<IOrpConnection, Task>> m_Greetings;
        private IEnumerable<Func<IOrpConnection, Task>> m_Farewells;

        /// <summary>
        /// Initialize a new <see cref="OrpClient"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="Options"></param>
        public OrpClient(IServiceProvider Services, OrpClientOptions Options)
            : base(Services, Options.RemoteEndPoint) => m_Options = Options;

        /// <summary>
        /// Set Mappings for the client.
        /// </summary>
        /// <param name="Mappings"></param>
        public void SetMappings(IOrpMappings Mappings) => m_Mappings = Mappings;

        /// <summary>
        /// Set Endpoint for the client.
        /// </summary>
        /// <param name="Endpoint"></param>
        public void SetEndpoint(Func<IOrpContext, Task> Endpoint) => m_Endpoint = Endpoint;

        /// <summary>
        /// Set Greeting delegate enumerable.
        /// </summary>
        /// <param name="Greetings"></param>
        public void SetGreetings(IEnumerable<Func<IOrpConnection, Task>> Greetings) => m_Greetings = Greetings;

        /// <summary>
        /// Set Farewell delegate enumerable.
        /// </summary>
        /// <param name="Farewells"></param>
        public void SetFarewells(IEnumerable<Func<IOrpConnection, Task>> Farewells) => m_Farewells = Farewells;

        /// <summary>
        /// Configure the mapping and endpoint.
        /// </summary>
        /// <param name="Session"></param>
        /// <returns></returns>
        protected override Task OnConfigure(OrpConnection Session)
        {
            lock (this)
                m_Session = Session;

            Session.SetMappings(m_Mappings);
            Session.SetEndpoint(m_Endpoint);
            Session.SetGreetings(m_Greetings);
            Session.SetFarewells(m_Farewells);

            return base.OnConfigure(Session);
        }

        /// <summary>
        /// Run the <see cref="OrpClient"/> instance as stand-alone.
        /// </summary>
        /// <param name="Cancellation"></param>
        /// <returns></returns>
        public async Task RunAsync(CancellationToken Cancellation = default)
        {
            while (!Cancellation.IsCancellationRequested)
            {
                if (!await ConnectAsync(Cancellation))
                {
                    try { await Task.Delay(m_Options.RecoveryTerm, Cancellation); }
                    catch { }
                    continue;
                }

                var Tcs = new TaskCompletionSource();
                using (Cancellation.Register(Tcs.SetResult))
                    await Task.WhenAny(Completion, Tcs.Task);

                try { await DisconnectAsync(); }
                catch { }
            }
        }

        /// <summary>
        /// Emit the <paramref name="Message"/> asynchronously.
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
        public Task<OrpMessage> EmitAsync(object Message)
        {
            lock (this)
            {
                if (m_Session is null)
                    throw new InvalidOperationException("This instance isn't ready yet.");
            }

            return m_Session.EmitAsync(Message);
        }

        /// <summary>
        /// Notify the <paramref name="Message"/> asynchronously.
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
        public Task NotifyAsync(object Message)
        {
            lock (this)
            {
                if (m_Session is null)
                    throw new InvalidOperationException("This instance isn't ready yet.");
            }

            return m_Session.NotifyAsync(Message);
        }
    }
}
