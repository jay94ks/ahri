using System;

namespace Ahri
{
    /// <summary>
    /// Defines the scope of a service.
    /// </summary>
    public interface IServiceScope : IDisposable
    {
        /// <summary>
        /// Service Provider instance to access on the services that included on the scope.
        /// </summary>
        IServiceProvider ServiceProvider { get; }
    }
}
