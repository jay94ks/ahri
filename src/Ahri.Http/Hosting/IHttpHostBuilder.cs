using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ahri.Http.Hosting
{
    public interface IHttpHostBuilder
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
        IHttpHostBuilder ConfigureServices(Action<IServiceCollection> Configure);

        /// <summary>
        /// Adds a delegate that builds the <see cref="IHttpServer"/> instance.
        /// </summary>
        /// <param name="Configure"></param>
        /// <returns></returns>
        IHttpHostBuilder ConfigureHttpServer(Action<IHttpServerBuilder> Configure);

        /// <summary>
        /// Adds a delegate that configures the Http application instance.
        /// </summary>
        /// <param name="Configure"></param>
        /// <returns></returns>
        IHttpHostBuilder Configure(Action<IHttpApplicationBuilder> Configure);
    }
}
