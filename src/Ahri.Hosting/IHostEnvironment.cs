using System.Threading;
using System.Threading.Tasks;

namespace Ahri.Hosting
{
    /// <summary>
    /// Abstracts the host environment.
    /// Host implement this component as optional if required.
    /// </summary>
    public interface IHostEnvironment
    {
        /// <summary>
        /// Prepares the host environment and waits for it to be ready.
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        Task PrepareAsync(CancellationToken Token = default);

        /// <summary>
        /// Restore the prepared environment.
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        Task FinishAsync();
    }
}
