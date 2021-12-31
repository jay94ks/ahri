using Ahri.Http.Hosting;
using Ahri.Http.Orb.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Orb
{
    public static class OrbExtensions
    {
        /// <summary>
        /// Use `Orb` Http Server implementation to host web application.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IHttpHostBuilder UseOrb(this IHttpHostBuilder This)
        {
            This.ConfigureHttpServer(Server
                => Server.Factory = (Services, EndPoint) => new HttpServerAdapter(Services, EndPoint));

            return This;
        }
    }
}
