using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Routing
{
    /// <summary>
    /// Router Builder interface.
    /// </summary>
    public interface IRouterBuilder
    {
        /// <summary>
        /// A central location that is used to share components between mapped endpoints.
        /// </summary>
        IDictionary<object, object> Properties { get; }

        /// <summary>
        /// Map an endpoint for the method.
        /// </summary>
        /// <param name="Method"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        IRouterBuilder Map(string Method, Func<IHttpContext, Task<IHttpAction>> Endpoint);

        /// <summary>
        /// Adds or append actions to the sub-router that has responsibility to handle the specific path.
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="Configure"></param>
        /// <returns></returns>
        IRouterBuilder Path(string Path, Action<IRouterBuilder> Configure);

        /// <summary>
        /// Adds the conditional sub-router.
        /// </summary>
        /// <param name="Condition"></param>
        /// <param name="Configure"></param>
        /// <returns></returns>
        IRouterBuilder When(Func<IHttpContext, Task<bool>> Condition, Action<IRouterBuilder> Configure);

        /// <summary>
        /// Adds a middleware for the current routing scope.
        /// </summary>
        /// <param name="Middleware"></param>
        /// <returns></returns>
        IRouterBuilder Use(Func<IHttpContext, Func<Task>, Task> Middleware);

        /// <summary>
        /// Build a router instance.
        /// </summary>
        /// <returns></returns>
        IRouter Build();
    }
}
