using System;
using System.Collections.Generic;

namespace Ahri.Hosting
{
    /// <summary>
    /// Abstracts the process of the host is built.
    /// </summary>
    public interface IHostBuilder
    {
        /// <summary>
        /// A central location that is used to share datas between host building process.
        /// </summary>
        IDictionary<object, object> Properties { get; }

        /// <summary>
        /// Adds a delegate that registers the services.
        /// </summary>
        /// <param name="Configure"></param>
        /// <returns></returns>
        IHostBuilder ConfigureServices(Action<IServiceCollection> Configure);

        /// <summary>
        /// Adds a delegate that configures the service instances.
        /// </summary>
        /// <param name="Configure"></param>
        /// <returns></returns>
        IHostBuilder Configure(Action<IServiceProvider> Configure);

        /// <summary>
        /// Build the <see cref="IHost"/> instance.
        /// </summary>
        /// <returns></returns>
        IHost Build();
    }
}
