using Ahri.Core;
using Ahri.Hosting.Defaults;
using Ahri.Hosting.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Hosting.Builders
{
    /// <summary>
    /// Abstracts the process of the host is built.
    /// </summary>
    public class HostBuilder : IHostBuilder
    {
        private const string ERROR_INVALID_OPERATION
            = "No actions can be performed on the host builder while the host is being built.";

        private List<Action<IServiceCollection>> m_ConfigureServices = new();
        private List<Action<IServiceProvider>> m_Configures = new();
        private bool m_Building = false;

        /// <summary>
        /// Initialize a new <see cref="IHostBuilder"> instance.
        /// </summary>
        public HostBuilder() : this(null) { }

        /// <summary>
        /// Initialize a new <see cref="IHostBuilder"> instance.
        /// Note that this variation is an option for interworking with other builders.
        /// </summary>
        /// <param name="Properties"></param>
        public HostBuilder(IDictionary<object, object> Properties)
            => this.Properties = Properties ?? new Dictionary<object, object>();

        /// <inheritdoc/>
        public IDictionary<object, object> Properties { get; }

        /// <inheritdoc/>
        public IHost Build()
        {
            m_Building = true;
            try
            {
                var Registry = new ServiceCollection();
                var RegistryConfigured = false;

                ConfigureServices(Registry, () => ConfigureServices(Registry, ref RegistryConfigured));
                ConfigureServices(Registry, ref RegistryConfigured);

                var Services = Registry.BuildServiceProvider();
                var ServicesConfigured = false;

                Configure(Services, () => Configure(Services, ref ServicesConfigured));
                Configure(Services, ref ServicesConfigured);

                return Encapsulate(Services);
            }
            finally { m_Building = false; }
        }

        /// <inheritdoc/>
        public IHostBuilder ConfigureServices(Action<IServiceCollection> Configure)
        {
            if (m_Building)
                throw new InvalidOperationException(ERROR_INVALID_OPERATION);

            m_ConfigureServices.Add(Configure);
            return this;
        }

        /// <inheritdoc/>
        public IHostBuilder Configure(Action<IServiceProvider> Configure)
        {
            if (m_Building)
                throw new InvalidOperationException(ERROR_INVALID_OPERATION);

            m_Configures.Add(Configure);
            return this;
        }

        /// <summary>
        /// Called to configure the default services to the <see cref="IServiceCollection"/>.
        /// Other builder classes that inherit from <see cref="HostBuilder"/> can override this function and change the order 
        /// in which the <paramref name="RegisteredDelegates"/> delegate is executed to perform the user defined operations.<br />
        /// <b>Note that</b>, If the delegate is not called, it will be called last as default behavior.
        /// </summary>
        /// <param name="Registry"></param>
        protected virtual void ConfigureServices(IServiceCollection Registry, Action RegisteredDelegates) 
            => Registry.AddSingleton<IHostLifetime, HostLifetime>()
                       .AddSingleton<IHostEnvironment, HostEnvironment>();

        /// <summary>
        /// Implementation that provided as `ConfigureServices` delegate.
        /// </summary>
        /// <param name="Registry"></param>
        /// <param name="Configured"></param>
        private void ConfigureServices(ServiceCollection Registry, ref bool Configured)
        {
            if (Configured)
                return;

            Configured = true;
            foreach (var ConfigureService in m_ConfigureServices)
                ConfigureService?.Invoke(Registry);
        }

        /// <summary>
        /// Called to configure the services to the <see cref="IServiceProvider"/>.
        /// Other builder classes that inherit from <see cref="HostBuilder"/> can override this function and change the order 
        /// in which the <paramref name="RegisteredDelegates"/> delegate is executed to perform the user defined operations.<br />
        /// <b>Note that</b>, If the delegate is not called, it will be called last as default behavior.
        /// </summary>
        /// <param name="Services"></param>
        protected virtual void Configure(IServiceProvider Services, Action RegisteredDelegates) { }

        /// <summary>
        /// Invokes all queued delegates to configure the service instances.
        /// </summary>
        /// <param name="Services"></param>
        private void Configure(IServiceProvider Services, ref bool Configured)
        {
            if (Configured)
                return;

            Configured = true;
            foreach (var Configure in m_Configures)
                Configure?.Invoke(Services);
        }

        /// <summary>
        /// Encapsulate the <see cref="IServiceProvider"/> to host instance.
        /// </summary>
        /// <param name="Services"></param>
        /// <returns></returns>
        protected virtual IHost Encapsulate(IServiceProvider Services) => new Host(Services);
    }
}
