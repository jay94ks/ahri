using System.Threading;
using System.Threading.Tasks;

namespace Ahri.Hosting
{
    /// <summary>
    /// Abstracts a hosted service.
    /// </summary>
    public interface IHostedService
    {
        /// <summary>
        /// Start the hosted service.
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        Task StartAsync(CancellationToken Token = default);

        /// <summary>
        /// Stop the hosted service.
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}
