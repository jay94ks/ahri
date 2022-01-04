using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Routing.Internals.Wrappers
{
    internal struct MethodExecution
    {
        private Dictionary<string, Func<IHttpContext, Task<IHttpAction>>> m_Targets;

        /// <summary>
        /// Initialize a new <see cref="MethodExecution"/> instance.
        /// </summary>
        /// <param name="Targets"></param>
        public MethodExecution(IEnumerable<(string, Func<IHttpContext, Task<IHttpAction>>)> Targets)
        {
            m_Targets = new();

            foreach (var Each in Targets)
                m_Targets[Each.Item1] = Each.Item2;
        }

        /// <summary>
        /// Invoke the conditional router asynchronously.
        /// </summary>
        /// <param name="Http"></param>
        /// <param name="Next"></param>
        /// <returns></returns>
        public async Task InvokeAsync(IHttpContext Http, Func<Task> Next)
        {
            var Method = Http.Request.Method;
            var State = RouterState.Get(Http);
            
            if (!m_Targets.TryGetValue(Method, out var Target) &&
                !m_Targets.TryGetValue("*", out Target))
            {
                if (m_Targets.Count > 0 && Http.Response.Status == 404)
                    Http.Response.Status = 405;

                await Next();
                return;
            }

            if (State.PendingPathSpaces.Count() <= 0)
            {
                var Action = await Target(Http);
                if (Action != null)
                {
                    await Action.InvokeAsync(Http);
                    return;
                }
            }

            await Next();
        }
    }
}
