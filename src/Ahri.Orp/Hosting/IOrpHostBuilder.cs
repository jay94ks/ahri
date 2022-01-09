using Ahri.Orp.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ahri.Orp.Hosting
{
    public interface IOrpHostBuilder
    {
        /// <summary>
        /// A central location that is used to share datas between host building process.
        /// </summary>
        IDictionary<object, object> Properties { get; }

        /// <summary>
        /// Adds a delegate that configures the <see cref="OrpServerOptions"/> instance.
        /// </summary>
        /// <param name="Options"></param>
        /// <returns></returns>
        IOrpHostBuilder UseOptions(Action<OrpServerOptions> Options);

        /// <summary>
        /// Adds a delegate that registers the services.
        /// </summary>
        /// <param name="Configure"></param>
        /// <returns></returns>
        IOrpHostBuilder ConfigureServices(Action<IServiceCollection> Configure);

        /// <summary>
        /// Adds a delegate that configures the <see cref="IOrpMappings"/> table.
        /// </summary>
        /// <param name="Configure"></param>
        /// <returns></returns>
        IOrpHostBuilder ConfigureMappings(Action<IOrpMappingsBuilder> Configure);

        /// <summary>
        /// Adds a delegate that configure the Orp application instance.
        /// </summary>
        /// <param name="Configure"></param>
        /// <returns></returns>
        IOrpHostBuilder Configure(Action<IOrpApplicationBuilder> Configure);
    }
}
