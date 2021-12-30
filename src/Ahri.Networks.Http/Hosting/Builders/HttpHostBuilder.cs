using Ahri.Hosting;
using Ahri.Http.Hosting.Internals;
using Ahri.Http.Orb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Hosting.Builders
{
    public class HttpHostBuilder : IHttpHostBuilder
    {
        private IHostBuilder m_HostBuilder;

        private List<Action<IHttpServerBuilder>> m_ConfigureHttpServers = new();
        private List<Action<IHttpApplicationBuilder>> m_Configures = new();

        /// <summary>
        /// Initialize a new <see cref="HttpHostBuilder"/> instance.
        /// </summary>
        /// <param name="HostBuilder"></param>
        public HttpHostBuilder(IHostBuilder HostBuilder)
        {
            (m_HostBuilder = HostBuilder)
                .ConfigureServices(Services =>
                {
                    Services
                        .AddHostedService<HttpServerExecutor>()
                        .AddSingleton<HttpServerAccessor>()
                        .AddSingleton<HttpApplicationAccessor>();
                })

                .Configure(Services =>
                {
                    var ServerAccessor = Services.GetRequiredService<HttpServerAccessor>();
                    var ApplicationAccessor = Services.GetRequiredService<HttpApplicationAccessor>();

                    var ServerBuilder = new HttpServerBuilder(Services);
                    var ApplicationBuilder = new HttpApplicationBuilder(Services);

                    foreach (var Each in m_ConfigureHttpServers)
                        Each?.Invoke(ServerBuilder);

                    foreach (var Each in m_Configures)
                        Each?.Invoke(ApplicationBuilder);

                    ServerAccessor.Instance = ServerBuilder.Build();
                    ApplicationAccessor.Instance = ApplicationBuilder.Build();
                });

            this.UseOrb();
        }

        /// <inheritdoc/>
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();


        /// <inheritdoc/>
        public IHttpHostBuilder ConfigureHttpServer(Action<IHttpServerBuilder> Configure)
        {
            m_ConfigureHttpServers.Add(Configure);
            return this;
        }

        /// <inheritdoc/>
        public IHttpHostBuilder ConfigureServices(Action<IServiceCollection> Configure)
        {
            m_HostBuilder.ConfigureServices(Configure);
            return this;
        }

        /// <inheritdoc/>
        public IHttpHostBuilder Configure(Action<IHttpApplicationBuilder> Configure)
        {
            m_Configures.Add(Configure);
            return this;
        }
    }
}
