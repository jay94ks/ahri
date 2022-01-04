using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Routing.Internals
{
    internal class Router : IRouter
    {
        private Func<IHttpContext, Func<Task>, Task> m_Endpoint;

        /// <summary>
        /// Initialize a new <see cref="Router"/> instance.
        /// </summary>
        /// <param name="Endpoint"></param>
        public Router(Func<IHttpContext, Func<Task>, Task> Endpoint)
            => m_Endpoint = Endpoint;

        /// <summary>
        /// Invoke the <see cref="Router"/>.
        /// </summary>
        /// <param name="Http"></param>
        /// <returns></returns>
        public async Task InvokeAsync(IHttpContext Http)
        {
            var State = RouterState.Get(Http);

            Http.Request.PathString 
                = RouterBuilder.Normalize(Http.Request.PathString);

            State.ScopeIn(this);
            await m_Endpoint(Http, () => Task.CompletedTask);

            Debug.Assert(State.ScopeOut(this),
                $"Error: Couldn't scope out from the router.");
        }
    }
}
