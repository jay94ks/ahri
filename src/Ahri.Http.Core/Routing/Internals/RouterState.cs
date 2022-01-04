using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Routing.Internals
{
    public class RouterState : IRouterState
    {
        internal Stack<IRouter> ScopedRouters { get; } = new();
        internal Stack<string> PathSpaceStack { get; } = new();
        internal List<string> PathSpacePending { get; } = new();

        /// <summary>
        /// Get <see cref="RouterState"/> instance.
        /// </summary>
        /// <param name="Http"></param>
        /// <returns></returns>
        internal static RouterState Get(IHttpContext Http)
        {
            Http.Request.Properties.TryGetValue(typeof(IRouterState), out var Temp);

            if (!(Temp is RouterState State))
            {
                Http.Request.Properties[typeof(IRouterState)] = State = new RouterState();
                State.PathSpacePending.AddRange(Http.Request.PathString
                    .Split('/', StringSplitOptions.RemoveEmptyEntries));
            }

            return State;
        }

        /// <inheritdoc/>
        public IRouter Router => ScopedRouters.Count > 0 ? ScopedRouters.Peek() : null;

        /// <inheritdoc/>
        public IEnumerable<string> CurrentPathSpaces => PathSpaceStack;

        /// <inheritdoc/>
        public IEnumerable<string> PendingPathSpaces => PathSpacePending;

        /// <inheritdoc/>
        public IDictionary<string, string> PathParameters { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Scope in to the path space.
        /// </summary>
        /// <returns></returns>
        internal bool ScopeIn(string Expects)
        {
            var Name = PathSpacePending.FirstOrDefault();
            if (Name != null && Name == Expects)
            {
                PathSpaceStack.Push(Name);
                PathSpacePending.RemoveAt(0);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Scope in to the programatic router.
        /// </summary>
        /// <param name="Router"></param>
        internal void ScopeIn(IRouter Router) => ScopedRouters.Push(Router);

        /// <summary>
        /// Scope out from the path space.
        /// </summary>
        /// <param name="EscapeFrom"></param>
        internal bool ScopeOut(string EscapeFrom)
        {
            if (PathSpaceStack.TryPeek(out var Name) &&
                Name == EscapeFrom)
            {
                PathSpaceStack.Pop();
                PathSpacePending.Insert(0, Name);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Scope out from the programatic router.
        /// </summary>
        internal bool ScopeOut(IRouter Router)
        {
            if (ScopedRouters.TryPeek(out var LastRouter) &&
                LastRouter == Router)
            {
                ScopedRouters.Pop();
                return true;
            }

            return false;
        }
    }
}
