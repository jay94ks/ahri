using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ahri.Hosting.Internals
{
    /// <summary>
    /// This implementation aggregates and runs the registered HostedServices.
    /// </summary>
    internal class HostedService : IHostedService
    {
        private Queue<IHostedService> m_Starts;
        private Stack<IHostedService> m_Stops;

        /// <summary>
        /// Initialize a new <see cref="HostedService"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="TargetTypes"></param>
        public HostedService(IServiceProvider Services, IEnumerable<Type> TargetTypes)
        {
            m_Stops = new();
            m_Starts = new(TargetTypes
                .Select(Services.GetService)
                .Select(X => X as IHostedService)
                .Where(X => X != null));
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken Token = default)
        {
            while (m_Starts.TryDequeue(out var HostedService))
            {
                m_Stops.Push(HostedService);
                await HostedService.StartAsync(Token);
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync()
        {
            while (m_Stops.TryPop(out var HostedService))
            {
                await HostedService.StopAsync();
                m_Starts.Enqueue(HostedService);
            }
        }
    }
}
