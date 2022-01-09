using Ahri.Hosting;
using Ahri.Orp.Internals;
using Ahri.Orp.Internals.Accessors;
using System;
using System.Collections.Generic;

namespace Ahri.Orp.Hosting.Builders
{
    public class OrpClientHostBuilder : IOrpClientHostBuilder
    {
        private IHostBuilder m_HostBuilder;

        private List<Action<IOrpMappingsBuilder>> m_ConfigureMappings = new();
        private List<Action<IOrpApplicationBuilder>> m_Configures = new();
        private List<Action<OrpClientOptions>> m_UseOptions = new();

        public OrpClientHostBuilder(IHostBuilder HostBuilder)
        {
            (m_HostBuilder = HostBuilder)
                .ConfigureServices(Services =>
                {
                    Services
                        .AddHostedService<OrpClientService>()
                        .AddSingleton<OrpClientAccessor>()
                        .AddScoped<IOrpContextAccessor, OrpContextAccessor>();
                })

                .Configure(Services =>
                {
                    var ClientAccessor = Services.GetRequiredService<OrpClientAccessor>();
                    var ClientOptions = new OrpClientOptions();

                    var MappingsBuilder = new OrpMappingsBuilder();
                    var ApplicationBuilder = new OrpApplicationBuilder(Services);

                    foreach (var Each in m_UseOptions)
                        Each?.Invoke(ClientOptions);

                    foreach (var Each in m_ConfigureMappings)
                        Each?.Invoke(MappingsBuilder);

                    foreach (var Each in m_Configures)
                        Each?.Invoke(ApplicationBuilder);

                    var Client = new OrpClient(Services, ClientOptions);

                    Client.SetMappings(MappingsBuilder.Build());
                    Client.SetEndpoint(ApplicationBuilder.Build());

                    Client.SetGreetings(ApplicationBuilder.GetGreetings());
                    Client.SetFarewells(ApplicationBuilder.GetFarewells());

                    ClientAccessor.Client = Client;
                });
        }

        /// <inheritdoc/>
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        /// <inheritdoc/>
        public IOrpClientHostBuilder UseOptions(Action<OrpClientOptions> Options)
        {
            m_UseOptions.Add(Options);
            return this;
        }

        /// <inheritdoc/>
        public IOrpClientHostBuilder Configure(Action<IOrpApplicationBuilder> Configure)
        {
            m_Configures.Add(Configure);
            return this;
        }

        /// <inheritdoc/>
        public IOrpClientHostBuilder ConfigureServices(Action<IServiceCollection> Configure)
        {
            m_HostBuilder.ConfigureServices(Configure);
            return this;
        }

        /// <inheritdoc/>
        public IOrpClientHostBuilder ConfigureMappings(Action<IOrpMappingsBuilder> Configure)
        {
            m_ConfigureMappings.Add(Configure);
            return this;
        }
    }
}
