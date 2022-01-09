using Ahri.Orp.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ahri.Orp.Hosting.Builders
{
    public class OrpApplicationBuilder : IOrpApplicationBuilder
    {
        private List<Action<IServiceProvider>> m_Configures = new();
        private List<Func<IOrpContext, Func<Task>, Task>> m_Middlewares = new();

        private List<Func<IOrpConnection, Task>> m_Greetings = new();
        private List<Func<IOrpConnection, Task>> m_Farewells = new();

        /// <summary>
        /// Initialize a new <see cref="OrpApplicationBuilder"/> instances.
        /// </summary>
        /// <param name="ApplicationServices"></param>
        public OrpApplicationBuilder(IServiceProvider ApplicationServices)
            => this.ApplicationServices = ApplicationServices;

        /// <inheritdoc/>
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        /// <inheritdoc/>
        public IServiceProvider ApplicationServices { get; }

        /// <inheritdoc/>
        public Func<IOrpContext, Task> Build()
        {
            var App = m_Middlewares.FirstOrDefault();
            if (App != null)
            {
                foreach (var Each in m_Middlewares.Skip(1))
                    App = new OrpMiddleware(App, Each).InvokeAsync;
            }

            using (var Scope = ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                foreach (var Each in m_Configures)
                    Each?.Invoke(Scope.ServiceProvider);
            }

            return (Orp) => App(Orp, () => Task.CompletedTask);
        }

        /// <inheritdoc/>
        public IOrpApplicationBuilder Configure(Action<IServiceProvider> Configure)
        {
            m_Configures.Add(Configure);
            return this;
        }

        /// <inheritdoc/>
        public IOrpApplicationBuilder Use(Func<IOrpContext, Func<Task>, Task> Middleware)
        {
            m_Middlewares.Add(Middleware);
            return this;
        }

        /// <inheritdoc/>
        public IOrpApplicationBuilder UseGreetings(Func<IOrpConnection, Task> Greetings)
        {
            m_Greetings.Add(Greetings);
            return this;
        }

        /// <inheritdoc/>
        public IOrpApplicationBuilder UseFarewells(Func<IOrpConnection, Task> Farewells)
        {
            m_Farewells.Add(Farewells);
            return this;
        }

        /// <summary>
        /// Get Greeting operations.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<Func<IOrpConnection, Task>> GetGreetings() => m_Greetings;

        /// <summary>
        /// Get Farewell oprations.
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<Func<IOrpConnection, Task>> GetFarewells() => m_Farewells;
    }
}
