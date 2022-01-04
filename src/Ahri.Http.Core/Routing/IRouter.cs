using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Routing
{
    /// <summary>
    /// Router interface.
    /// </summary>
    public interface IRouter
    {
        /// <summary>
        /// Invoke the router and its execution routines to handle the <see cref="IHttpContext"/>.
        /// </summary>
        /// <param name="Http"></param>
        /// <returns></returns>
        Task InvokeAsync(IHttpContext Http);
    }
}
