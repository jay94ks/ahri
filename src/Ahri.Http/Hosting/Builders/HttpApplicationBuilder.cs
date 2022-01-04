using Ahri.Http.Hosting.Internals;
using Ahri.Http.Orb.Internals.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Hosting.Builders
{
    public class HttpApplicationBuilder : IHttpApplicationBuilder
    {
        private List<Action<IServiceProvider>> m_Configures = new();
        private List<Func<Func<IHttpContext, Func<Task>, Task>>> m_Middlewares = new();

        /// <summary>
        /// Initialize a new <see cref="HttpApplicationBuilder"/> instances.
        /// </summary>
        /// <param name="ApplicationServices"></param>
        public HttpApplicationBuilder(IServiceProvider ApplicationServices)
            => this.ApplicationServices = ApplicationServices;

        /// <inheritdoc/>
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        /// <inheritdoc/>
        public IServiceProvider ApplicationServices { get; }

        /// <inheritdoc/>
        public IHttpApplicationBuilder Use(Func<IHttpContext, Func<Task>, Task> Middleware)
        {
            m_Middlewares.Add(() => Middleware);
            return this;
        }

        /// <inheritdoc/>
        public IHttpApplicationBuilder Use(Func<Func<IHttpContext, Func<Task>, Task>> Factory)
        {
            m_Middlewares.Add(Factory);
            return this;
        }

        /// <inheritdoc/>
        public IHttpApplicationBuilder Configure(Action<IServiceProvider> Configure)
        {
            m_Configures.Add(Configure);
            return this;
        }

        /// <inheritdoc/>
        public IHttpApplication Build()
        {
            var Factory = m_Middlewares.FirstOrDefault();
            var App = Factory != null ? Factory() : null;
            if (App != null)
            {
                foreach(var Each in m_Middlewares.Skip(1))
                    App = new HttpMiddleware(App, Each()).InvokeAsync;
            }

            using(var Scope = ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                foreach (var Each in m_Configures)
                    Each?.Invoke(Scope.ServiceProvider);
            }

            return new HttpApplication(ApplicationServices, App);
        }
    }
}
