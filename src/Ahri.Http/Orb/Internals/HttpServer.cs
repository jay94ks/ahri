using Ahri.Http.Hosting;
using Ahri.Http.Orb.Internals.Models;
using Ahri.Networks.Tcp;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Orb.Internals
{
    public class HttpServer : TcpServer<HttpConnection>
    {
        private IHttpApplication m_App;

        /// <summary>
        /// Initialize a new <see cref="HttpServer"/> instance.
        /// </summary>
        /// <param name="EndPoint"></param>
        public HttpServer(IPEndPoint EndPoint) : base(EndPoint)
        {
        }

        /// <summary>
        /// Initialize a new <see cref="HttpServer"/> instance.
        /// </summary>
        /// <param name="Address"></param>
        /// <param name="Port"></param>
        public HttpServer(IPAddress Address, int Port) : base(Address, Port)
        {
        }

        /// <summary>
        /// Initialize a new <see cref="HttpServer"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="EndPoint"></param>
        public HttpServer(IServiceProvider Services, IPEndPoint EndPoint) : base(Services, EndPoint)
        {
        }

        /// <summary>
        /// Initialize a new <see cref="HttpServer"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="Address"></param>
        /// <param name="Port"></param>
        public HttpServer(IServiceProvider Services, IPAddress Address, int Port) : base(Services, Address, Port)
        {
        }

        /// <summary>
        /// Set the application instance to the server.
        /// </summary>
        /// <param name="App"></param>
        internal void SetApplication(IHttpApplication App) => m_App = App;

        /// <inheritdoc/>
        protected override Task OnConfigure(HttpConnection Session)
        {
            Session.Server = this;
            return base.OnConfigure(Session);
        }

        /// <summary>
        /// Called to invoke <see cref="OnRequestAsync"/> method from the <see cref="HttpConnection"/>.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        internal Task InvokeOnRequestAsync(IHttpContext Context) => OnRequestAsync(Context);

        /// <summary>
        /// Called when the request is received successfully.
        /// </summary>
        /// <returns></returns>
        protected virtual Task OnRequestAsync(IHttpContext Context)
        {
            if (m_App != null)
                return m_App.InvokeAsync(Context);

            return Task.CompletedTask;
        }
    }
}
