using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Routing.Internals.Wrappers
{
    internal struct ConditionalRouting
    {
        private IRouter m_Router;
        private Func<IHttpContext, Task<bool>> m_Condition;

        /// <summary>
        /// Initialize a new <see cref="ConditionalRouting"/> instance.
        /// </summary>
        /// <param name="Router"></param>
        public ConditionalRouting(IRouter Router, Func<IHttpContext, Task<bool>> Condition)
        {
            m_Router = Router;
            m_Condition = Condition;
        }

        /// <summary>
        /// Invoke the conditional router asynchronously.
        /// </summary>
        /// <param name="Http"></param>
        /// <param name="Next"></param>
        /// <returns></returns>
        public async Task InvokeAsync(IHttpContext Http, Func<Task> Next)
        {
            var State = RouterState.Get(Http);

            if (await m_Condition(Http))
            {
                await m_Router.InvokeAsync(Http);
                return;
            }

            await Next();
        }
    }
}
