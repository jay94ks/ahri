using Ahri.Hosting;
using Ahri.Orp.Internals.Accessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ahri.Orp.Internals
{
    internal class OrpServerService : IHostedService
    {
        private OrpServerAccessor m_ServerAccessor;

        public OrpServerService(OrpServerAccessor ServerAccessor)
        {
            m_ServerAccessor = ServerAccessor;
        }

        public Task StartAsync(CancellationToken Token = default)
        {
            return m_ServerAccessor.Server.StartAsync();
        }

        public Task StopAsync()
        {
            return m_ServerAccessor.Server.StopAsync();
        }
    }
}
