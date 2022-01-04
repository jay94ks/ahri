using System;
using System.Threading.Tasks;

namespace Ahri.Http.Core
{
    public interface IHttpActionFilter
    {
        /// <summary>
        /// Called to filter the action before executing it.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        Task OnFilterAsync(ControllerContext Context, Func<Task> Next);
    }
}
