using System.Threading.Tasks;

namespace Ahri.Http.Core
{
    public interface IHttpAction
    {
        /// <summary>
        /// Invoke the <see cref="IHttpAction"/> instance.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        Task InvokeAsync(IHttpContext Context);
    }
}
