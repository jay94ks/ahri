using Ahri.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Hosting.Internals
{
    public class HttpApplication : IHttpApplication
    {
        private Func<IHttpContext, Func<Task>, Task> m_App;

        /// <summary>
        /// Initialize a new <see cref="HttpApplication"/> instance.
        /// </summary>
        /// <param name="App"></param>
        public HttpApplication(IServiceProvider Services, Func<IHttpContext, Func<Task>, Task> App)
        {
            m_App = App;
            this.Services = Services;
        }

        /// <inheritdoc/>
        public IServiceProvider Services { get; }

        /// <inheritdoc/>
        public Task InvokeAsync(IHttpContext Context)
        {
            if (m_App is null)
                return Task.CompletedTask;

            var Accessor = Context.Request.Services.GetService<IHttpContextAccessor>();
            if (Accessor is HttpContextAccessor Instance)
                Instance.Instance = Context;

            return m_App(Context, () => Task.CompletedTask);
        }
    }
}
