using Ahri.Http.Core.Routing.Internals.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Routing.Internals
{
    public class RouterBuilder : IRouterBuilder
    {
        private Dictionary<string, Func<IHttpContext, Task>> m_Methods = new();
        private List<(Func<IHttpContext, Task<bool>> Cond, IRouterBuilder Route)> m_Conditionals = new();
        private Dictionary<string, IRouterBuilder> m_PathRouters = new();
        private List<Func<IHttpContext, Func<Task>, Task>> m_Middlewares = new();

        /// <summary>
        /// Normalize the <paramref name="Path"/>.
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        internal static string Normalize(string Path)
        {
            var Completed = new List<string>();
            var Paths = Path
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Where(X => X != ".").Select(X => X.Trim())
                .Where(X => !string.IsNullOrWhiteSpace(X));

            foreach(var Each in Paths)
            {
                if (Each == "..")
                {
                    if (Completed.Count > 0)
                        Completed.RemoveAt(Completed.Count - 1);

                    continue;
                }

                Completed.Add(Each);
            }

            return string.Join('/', Completed);
        }

        /// <inheritdoc/>
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        /// <inheritdoc/>
        public IRouterBuilder Map(string Method, Func<IHttpContext, Task> Endpoint)
        {
            if (string.IsNullOrWhiteSpace(Method) || Method == "*")
            {
                m_Methods["*"] = Endpoint;
                return this;
            }

            m_Methods[Method] = Endpoint;
            return this;
        }

        /// <inheritdoc/>
        public IRouterBuilder Path(string Path, Action<IRouterBuilder> Configure)
        {
            var Index = (Path = Normalize(Path ?? "")).IndexOf('/');
            
            var Name = Path.Substring(0, Index);
            var Subpath = Path.Substring(Index + 1);

            m_PathRouters.TryGetValue(Name, out var Builder);
            if (Builder is null)
                m_PathRouters[Name] = Builder = new RouterBuilder();

            if (Subpath.Length > 0)
                Builder.Path(Subpath, Configure);

            else
                Configure?.Invoke(Builder);

            return this;
        }

        /// <inheritdoc/>
        public IRouterBuilder When(Func<IHttpContext, Task<bool>> Condition, Action<IRouterBuilder> Configure)
        {
            var Index = m_Conditionals.FindIndex(X => X.Cond == Condition);
            if (Index >= 0)
                Configure?.Invoke(m_Conditionals[Index].Route);

            else
            {
                var Router = new RouterBuilder();
                m_Conditionals.Add((Condition, Router));

                Configure?.Invoke(Router);
            }

            return this;
        }

        /// <inheritdoc/>
        public IRouterBuilder Use(Func<IHttpContext, Func<Task>, Task> Middleware)
        {
            m_Middlewares.Add(Middleware);
            return this;
        }

        /// <inheritdoc/>
        public IRouter Build()
        {
            Func<IHttpContext, Func<Task>, Task> Built = null;

            Built = ApplyMiddlewares(Built);
            Built = ApplyConditionalRoutings(Built);
            Built = ApplyPathRoutings(Built);
            Built = ApplyMethodExecution(Built);

            return new Router(Built);
        }

        private Func<IHttpContext, Func<Task>, Task> ApplyMiddlewares(Func<IHttpContext, Func<Task>, Task> Built)
        {
            foreach (var Each in m_Middlewares)
            {
                if (Built != null)
                    Built = new Middleware(Built, Each).InvokeAsync;

                else
                    Built = Each;
            }

            return Built;
        }

        private Func<IHttpContext, Func<Task>, Task> ApplyPathRoutings(Func<IHttpContext, Func<Task>, Task> Built)
        {
            foreach (var Subpath in m_PathRouters.Keys)
            {
                var Builder = m_PathRouters[Subpath];
                var Routing = new PathRouting(Builder.Build(), Subpath);
                if (Built != null)
                    Built = new Middleware(Built, Routing.InvokeAsync).InvokeAsync;

                else
                    Built = Routing.InvokeAsync;
            }

            return Built;
        }

        private Func<IHttpContext, Func<Task>, Task> ApplyConditionalRoutings(Func<IHttpContext, Func<Task>, Task> Built)
        {
            foreach (var Cond in m_Conditionals)
            {
                var Routing = new ConditionalRouting(Cond.Route.Build(), Cond.Cond);
                if (Built != null)
                    Built = new Middleware(Built, Routing.InvokeAsync).InvokeAsync;

                else
                    Built = Routing.InvokeAsync;
            }

            return Built;
        }

        private Func<IHttpContext, Func<Task>, Task> ApplyMethodExecution(Func<IHttpContext, Func<Task>, Task> Built)
        {
            var Routing = new MethodExecution(m_Methods.Select(X => (X.Key, X.Value)));
            if (Built != null)
                Built = new Middleware(Built, Routing.InvokeAsync).InvokeAsync;

            else
                Built = Routing.InvokeAsync;

            return Built;
        }
    }
}
