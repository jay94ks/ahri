using System;

namespace Ahri.Hosting
{
    /// <summary>
    /// Abstract the host.
    /// </summary>
    public interface IHost : IHostedService, IDisposable
    {
        /// <summary>
        /// Service Provider.
        /// </summary>
        IServiceProvider Services { get; }
    }
}
