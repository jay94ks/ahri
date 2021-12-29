using Ahri.Networks.Tcp;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Networks.Http
{
    public class HttpServer<TSession> : TcpServer<TSession> where TSession : HttpSession
    {
        /// <summary>
        /// Initialize a new <see cref="HttpServer{TSession}"/> instance.
        /// </summary>
        /// <param name="EndPoint"></param>
        protected HttpServer(IPEndPoint EndPoint) : base(EndPoint)
        {
        }

        /// <summary>
        /// Initialize a new <see cref="HttpServer{TSession}"/> instance.
        /// </summary>
        /// <param name="Address"></param>
        /// <param name="Port"></param>
        protected HttpServer(IPAddress Address, int Port) : base(Address, Port)
        {
        }

        /// <summary>
        /// Initialize a new <see cref="HttpServer{TSession}"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="EndPoint"></param>
        protected HttpServer(IServiceProvider Services, IPEndPoint EndPoint) : base(Services, EndPoint)
        {
        }

        /// <summary>
        /// Initialize a new <see cref="HttpServer{TSession}"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="Address"></param>
        /// <param name="Port"></param>
        protected HttpServer(IServiceProvider Services, IPAddress Address, int Port) : base(Services, Address, Port)
        {
        }
    }
}
