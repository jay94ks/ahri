using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Routing.Internals.Wrappers
{
    internal struct PathRouting
    {
        private IRouter m_Router;
        private string m_Name;

        /// <summary>
        /// Initialize a new <see cref="PathRouting"/> instance.
        /// </summary>
        /// <param name="Router"></param>
        public PathRouting(IRouter Router, string Name)
        {
            m_Router = Router;
            m_Name = Name;
        }

        /// <summary>
        /// Invoke the path router asynchronously.
        /// </summary>
        /// <param name="Http"></param>
        /// <param name="Next"></param>
        /// <returns></returns>
        public async Task InvokeAsync(IHttpContext Http, Func<Task> Next)
        {
            var State = RouterState.Get(Http);
            var Name = State.PathSpacePending.FirstOrDefault();
            if (Name != null)
            {
                var Executed = false;
                if (m_Name.StartsWith(':'))
                {
                    State.PathParameters[m_Name.Substring(1)] = Name;
                    Executed = true;
                }

                else if (Name == m_Name)
                    Executed = true;
                if (Executed && State.ScopeIn(Name))
                {
                    await m_Router.InvokeAsync(Http);
                    Debug.Assert(State.ScopeOut(Name),
                        $"Error: Couldn't scope out from `{Name}`.");

                    return;
                }
            }

            await Next();
        }
    }
}
