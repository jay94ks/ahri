using System;
using System.Threading.Tasks;

namespace Ahri.Http.Core
{
    public interface IHttpMiddlewareBuilder
    {
        /// <summary>
        /// Build the middleware delegate.
        /// </summary>
        /// <returns></returns>
        Func<IHttpContext, Func<Task>, Task> Build();
    }
}
