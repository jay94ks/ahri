using System;
using System.Threading.Tasks;

namespace Ahri.Http
{
    public interface IHttpApplication
    {
        /// <summary>
        /// Service Provider instance.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Invoke the application asynchronously.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        Task InvokeAsync(IHttpContext Context);
    }
}
