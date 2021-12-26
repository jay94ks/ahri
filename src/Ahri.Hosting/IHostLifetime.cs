using System.Threading;

namespace Ahri.Hosting
{
    /// <summary>
    /// Abstracts the host's lifetime.
    /// </summary>
    public interface IHostLifetime
    {
        /// <summary>
        /// Triggered when the host is started.
        /// </summary>
        CancellationToken Started { get; }

        /// <summary>
        /// Triggered when the host is stopping.
        /// </summary>
        CancellationToken Stopping { get; }

        /// <summary>
        /// Triggered when the host is stopped.
        /// </summary>
        CancellationToken Stopped { get; }

        /// <summary>
        /// Requests the termination of the host application.
        /// </summary>
        void Terminate();
    }
}
