using Ahri.Hosting;
using Ahri.Orp.Internals.Accessors;
using System.Threading;
using System.Threading.Tasks;

namespace Ahri.Orp.Internals
{
    internal class OrpClientService : IHostedService
    {
        private OrpClientAccessor m_ClientAccessor;
        private CancellationTokenSource m_Cts;
        private Task m_Service;

        public OrpClientService(OrpClientAccessor ClientAccessor)
        {
            m_ClientAccessor = ClientAccessor;
        }

        public Task StartAsync(CancellationToken Token = default)
        {
            if (m_Cts != null)
                return Task.CompletedTask;

            m_Cts = new CancellationTokenSource();
            m_Service = m_ClientAccessor.Client.RunAsync(m_Cts.Token);
            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            if (m_Cts != null)
            {
                m_Cts.Cancel();

                if (m_Service != null)
                    await m_Service;

                m_Cts.Dispose();
                m_Cts = null;
            }
        }
    }
}
