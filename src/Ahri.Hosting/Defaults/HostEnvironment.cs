using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ahri.Hosting.Defaults
{
    /// <summary>
    /// Default implementation of IHostEnvironment.
    /// This default implementation is implemented to be registered 
    /// by HostBuilder by default unless removed manually.
    /// </summary>
    public class HostEnvironment : IHostEnvironment
    {
        private IHostLifetime m_Lifetime;

        /// <summary>
        /// Initialize a new <see cref="HostEnvironment"/> instance.
        /// </summary>
        /// <param name="Lifetime"></param>
        public HostEnvironment(IHostLifetime Lifetime)
            => m_Lifetime = Lifetime;

        /// <inheritdoc/>
        public Task PrepareAsync(CancellationToken Token = default)
        {
            Console.CancelKeyPress += OnCancelKeyPress;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task FinishAsync()
        {
            Console.CancelKeyPress -= OnCancelKeyPress;
            return Task.CompletedTask;
        }

        /// <summary>
        /// When the Ctrl + C key or SIGINT signal is received, 
        /// the request is canceled and the <see cref="IHostLifetime.Terminate"/> request is sent to the lifetime instance.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            m_Lifetime.Terminate();
        }
    }
}
