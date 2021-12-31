using System;
using System.Net;

namespace Ahri.Http.Hosting.Builders
{
    public class HttpServerBuilder : IHttpServerBuilder
    {
        private IServiceProvider m_Services;

        /// <summary>
        /// Initialize a new <see cref="HttpServerBuilder"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        public HttpServerBuilder(IServiceProvider Services)
        {
            m_Services = Services;
            Endpoint = new IPEndPoint(IPAddress.Any, 5000);
        }

        /// <inheritdoc/>
        public Func<IServiceProvider, IPEndPoint, IHttpServer> Factory { get; set; }

        /// <inheritdoc/>
        public IPEndPoint Endpoint { get; set; } = new IPEndPoint(IPAddress.Any, 5000);

        /// <inheritdoc/>
        public IHttpServer Build() => Factory(m_Services, Endpoint);
    }

}
