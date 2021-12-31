using Ahri.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ahri.Http.Hosting.Internals
{
    public class HttpServerExecutor : IHostedService
    {
        private HttpServerAccessor m_ServerAccessor;
        private HttpApplicationAccessor m_ApplicationAccessor;

        private IHostLifetime m_HostLifetime;
        private Task m_Task;

        /// <summary>
        /// Initialize a new <see cref="HttpServerExecutor"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        public HttpServerExecutor(IServiceProvider Services, IHostLifetime Lifetime)
        {
            if (!Services.TryGetRequiredService(out m_ServerAccessor) ||
                !Services.TryGetRequiredService(out m_ApplicationAccessor))
            {
                throw new InvalidOperationException("No Http required services are correctly registerd.");
            }

            m_HostLifetime = Lifetime;
        }

        public Task StartAsync(CancellationToken Token = default)
        {
            var Server = m_ServerAccessor.Instance;
            var Application = m_ApplicationAccessor.Instance;

            m_Task = Server.RunAsync(Application, m_HostLifetime.Stopping);
            return Task.CompletedTask;
        }

        public Task StopAsync() => m_Task ?? Task.CompletedTask;
    }
}
