using Ahri.Hosting;
using Ahri.Orp.Internals;
using Ahri.Orp.Internals.Accessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Orp.Hosting.Builders
{
    public class OrpHostBuilder : IOrpHostBuilder
    {
        private IHostBuilder m_HostBuilder;

        private List<Action<IOrpMappingsBuilder>> m_ConfigureMappings = new();
        private List<Action<IOrpApplicationBuilder>> m_Configures = new();
        private List<Action<OrpServerOptions>> m_UseOptions = new();

        public OrpHostBuilder(IHostBuilder HostBuilder)
        {
            (m_HostBuilder = HostBuilder)
                .ConfigureServices(Services =>
                {
                    Services
                        .AddHostedService<OrpServerService>()
                        .AddSingleton<OrpServerAccessor>()
                        .AddScoped<IOrpContextAccessor, OrpContextAccessor>();
                })

                .Configure((Action<IServiceProvider>)(Services =>
                {
                    var ServerAccessor = Services.GetRequiredService<OrpServerAccessor>();
                    var ServerOptions = new OrpServerOptions();

                    var MappingsBuilder = new OrpMappingsBuilder();
                    var ApplicationBuilder = new OrpApplicationBuilder((IServiceProvider)Services);

                    foreach (var Each in m_UseOptions)
                        Each?.Invoke(ServerOptions);

                    foreach (var Each in m_ConfigureMappings)
                        Each?.Invoke(MappingsBuilder);

                    foreach (var Each in m_Configures)
                        Each?.Invoke(ApplicationBuilder);

                    var Server = new OrpServer(Services, ServerOptions);

                    Server.SetMappings(MappingsBuilder.Build());
                    Server.SetEndpoint(ApplicationBuilder.Build());

                    ServerAccessor.Server = Server;
                }));
        }

        /// <inheritdoc/>
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        /// <inheritdoc/>
        public IOrpHostBuilder UseOptions(Action<OrpServerOptions> Options)
        {
            m_UseOptions.Add(Options);
            return this;
        }

        /// <inheritdoc/>
        public IOrpHostBuilder Configure(Action<IOrpApplicationBuilder> Configure)
        {
            m_Configures.Add(Configure);
            return this;
        }

        /// <inheritdoc/>
        public IOrpHostBuilder ConfigureServices(Action<IServiceCollection> Configure)
        {
            m_HostBuilder.ConfigureServices(Configure);
            return this;
        }

        /// <inheritdoc/>
        public IOrpHostBuilder ConfigureMappings(Action<IOrpMappingsBuilder> Configure)
        {
            m_ConfigureMappings.Add(Configure);
            return this;
        }
    }
}
