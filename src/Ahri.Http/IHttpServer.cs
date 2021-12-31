using System.Threading;
using System.Threading.Tasks;

namespace Ahri.Http
{
    public interface IHttpServer
    {
        /// <summary>
        /// Start the server instance asynchronously.
        /// </summary>
        /// <param name="Application"></param>
        /// <param name="StoppingToken"></param>
        /// <returns></returns>
        Task RunAsync(IHttpApplication Application, CancellationToken StoppingToken = default);
    }
}
