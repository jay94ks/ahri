using System;
using System.Collections.Generic;
using System.Net;

namespace Ahri.Http.Hosting
{
    public interface IHttpServerBuilder
    {
        /// <summary>
        /// Factory delegate that creates <see cref="IHttpServer"/> instance.
        /// </summary>
        Func<IServiceProvider, IPEndPoint, IHttpServer> Factory { get; set; }

        /// <summary>
        /// Endpoint to listen.
        /// </summary>
        IPEndPoint Endpoint { get; set; }

        /// <summary>
        /// Build an <see cref="IHttpServer"/> instance.
        /// </summary>
        /// <returns></returns>
        IHttpServer Build();
    }
}