using System;
using System.Threading;

namespace Ahri.Hosting.Internals
{
    internal class HostLifetime : IHostLifetime, IDisposable
    {
        private CancellationTokenSource m_Started = new(), m_Stopping = new(), m_Stopped = new();

        /// <inheritdoc/>
        public CancellationToken Started => m_Started.Token;

        /// <inheritdoc/>
        public CancellationToken Stopping => m_Stopping.Token;

        /// <inheritdoc/>
        public CancellationToken Stopped => m_Stopped.Token;

        /// <inheritdoc/>
        public void Terminate() => Fire(m_Stopping);

        /// <summary>
        /// Triggers the <see cref="CancellationTokenSource"/>.
        /// </summary>
        /// <param name="Cts"></param>
        private void Fire(CancellationTokenSource Cts)
        {
            lock(Cts)
            {
                if (Cts.IsCancellationRequested)
                    return;

                Cts.Cancel();
            }
        }

        public void OnStarted() => Fire(m_Started);
        public void OnStopped() => Fire(m_Stopped);

        /// <inheritdoc/>
        public void Dispose()
        {
            m_Started.Dispose();
            m_Stopping.Dispose();
            m_Stopped.Dispose();
        }
    }
}
