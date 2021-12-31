using Ahri.Http.Hosting;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Ahri.Http.Orb.Internals
{
    public class HttpServerAdapter : IHttpServer
    {
        private IPEndPoint m_EndPoint;
        private HttpServer m_Server;

        /// <summary>
        /// Initialize a new <see cref="HttpServerAdapter"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="EndPoint"></param>
        public HttpServerAdapter(IServiceProvider Services, IPEndPoint EndPoint)
            => m_Server = new HttpServer(Services, m_EndPoint = EndPoint);

        /// <summary>
        /// Run the internal <see cref="HttpServer"/> instance.
        /// </summary>
        /// <param name="Application"></param>
        /// <param name="StoppingToken"></param>
        /// <returns></returns>
        public async Task RunAsync(IHttpApplication Application, CancellationToken StoppingToken = default)
        {
            var Tcs = new TaskCompletionSource();
            using (StoppingToken.Register(Tcs.SetResult))
            {
                m_Server.SetApplication(Application);

                if (!await m_Server.StartAsync())
                {
                    throw new InvalidOperationException(
                        "Couldn't start the HttpServer instance for " +
                        $"{m_EndPoint.Address}:{m_EndPoint.Port}.");
                }


                await Tcs.Task;
                await m_Server.StopAsync();
            }
        }
    }
}
