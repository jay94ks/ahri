using Ahri.Orp.Internals;
using System;
using System.Collections.Generic;

namespace Ahri.Orp.Hosting
{
    public interface IOrpClientHostBuilder
    {
        /// <summary>
        /// A central location that is used to share datas between host building process.
        /// </summary>
        IDictionary<object, object> Properties { get; }

        /// <summary>
        /// Adds a delegate that configures the <see cref="OrpClientOptions"/> instance.
        /// </summary>
        /// <param name="Options"></param>
        /// <returns></returns>
        IOrpClientHostBuilder UseOptions(Action<OrpClientOptions> Options);

        /// <summary>
        /// Adds a delegate that registers the services.
        /// </summary>
        /// <param name="Configure"></param>
        /// <returns></returns>
        IOrpClientHostBuilder ConfigureServices(Action<IServiceCollection> Configure);

        /// <summary>
        /// Adds a delegate that configures the <see cref="IOrpMappings"/> table.
        /// </summary>
        /// <param name="Configure"></param>
        /// <returns></returns>
        IOrpClientHostBuilder ConfigureMappings(Action<IOrpMappingsBuilder> Configure);

        /// <summary>
        /// Adds a delegate that configure the Orp application instance.
        /// </summary>
        /// <param name="Configure"></param>
        /// <returns></returns>
        IOrpClientHostBuilder Configure(Action<IOrpApplicationBuilder> Configure);
    }
}
