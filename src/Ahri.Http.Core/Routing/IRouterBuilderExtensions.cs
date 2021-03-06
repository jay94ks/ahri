using Ahri.Http.Core.Routing.Internals;
using Ahri.Http.Core.Routing.Internals.Actions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Routing
{
    public static class IRouterBuilderExtensions
    {
        /// <summary>
        /// Map an endpoint for the method.
        /// </summary>
        /// <param name="Method"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder Map(this IRouterBuilder Route, string Method, Func<IHttpContext, IHttpAction> Endpoint) 
            => Route.Map(Method, Context => Task.FromResult(Endpoint(Context)));

        /// <summary>
        /// Map a controller to the route.
        /// </summary>
        /// <param name="Route"></param>
        /// <returns></returns>
        public static IRouterBuilder Map(this IRouterBuilder Route, Type Type)
        {
            var Paths = Type.GetRoutePath();
            var Methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                  .Concat(Type.GetMethods(BindingFlags.Public | BindingFlags.Static));

            foreach (var Each in Methods)
            {
                var Endpath = Each.GetRoutePath();
                if (Endpath is null)
                    continue;

                var Endpoint = MakeEndpoint(Type, Each);
                if (Paths != null)
                    Endpath = $"{Paths.TrimEnd('/')}/{Endpath.TrimStart('/')}";

                var Method = Each.GetCustomAttribute<MethodRouteAttribute>();
                Route.Path(Endpath, Subroute =>
                {
                    if (Method != null && Method.Method != "*")
                        Subroute.Map(Method.Method, Endpoint);

                    else
                        Subroute.Map("*", Endpoint);
                });
            }

            return Route;
        }

        /// <summary>
        /// Map an endpoint to specific path on all methods.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnAny(this IRouterBuilder Route, string Path, Func<IHttpContext, Task<IHttpAction>> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("*", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on HEAD method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnHead(this IRouterBuilder Route, string Path, Func<IHttpContext, Task<IHttpAction>> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("HEAD", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on OPTIONS method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnOptions(this IRouterBuilder Route, string Path, Func<IHttpContext, Task<IHttpAction>> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("OPTIONS", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on GET method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnGet(this IRouterBuilder Route, string Path, Func<IHttpContext, Task<IHttpAction>> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("GET", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on POST method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnPost(this IRouterBuilder Route, string Path, Func<IHttpContext, Task<IHttpAction>> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("POST", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on PUT method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnPut(this IRouterBuilder Route, string Path, Func<IHttpContext, Task<IHttpAction>> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("PUT", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on PATCH method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnPatch(this IRouterBuilder Route, string Path, Func<IHttpContext, Task<IHttpAction>> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("PATCH", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on DELETE method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnDelete(this IRouterBuilder Route, string Path, Func<IHttpContext, Task<IHttpAction>> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("DELETE", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on all methods.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnAny(this IRouterBuilder Route, Func<IHttpContext, Task<IHttpAction>> Endpoint)
            => Route.Path(string.Empty, Subroute => Subroute.Map("*", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on HEAD method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnHead(this IRouterBuilder Route, Func<IHttpContext, Task<IHttpAction>> Endpoint)
            => Route.Path(string.Empty, Subroute => Subroute.Map("HEAD", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on OPTIONS method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnOptions(this IRouterBuilder Route, Func<IHttpContext, Task<IHttpAction>> Endpoint)
            => Route.Path(string.Empty, Subroute => Subroute.Map("OPTIONS", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on GET method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnGet(this IRouterBuilder Route, Func<IHttpContext, Task<IHttpAction>> Endpoint)
            => Route.Path(string.Empty, Subroute => Subroute.Map("GET", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on POST method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnPost(this IRouterBuilder Route, Func<IHttpContext, Task<IHttpAction>> Endpoint)
            => Route.Path(string.Empty, Subroute => Subroute.Map("POST", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on PUT method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnPut(this IRouterBuilder Route, Func<IHttpContext, Task<IHttpAction>> Endpoint)
            => Route.Path(string.Empty, Subroute => Subroute.Map("PUT", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on PATCH method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnPatch(this IRouterBuilder Route, Func<IHttpContext, Task<IHttpAction>> Endpoint)
            => Route.Path(string.Empty, Subroute => Subroute.Map("PATCH", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on DELETE method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnDelete(this IRouterBuilder Route, Func<IHttpContext, Task<IHttpAction>> Endpoint)
            => Route.Path(string.Empty, Subroute => Subroute.Map("DELETE", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on all methods.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnAny(this IRouterBuilder Route, string Path, Func<IHttpContext, IHttpAction> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("*", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on HEAD method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnHead(this IRouterBuilder Route, string Path, Func<IHttpContext, IHttpAction> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("HEAD", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on OPTIONS method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnOptions(this IRouterBuilder Route, string Path, Func<IHttpContext, IHttpAction> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("OPTIONS", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on GET method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnGet(this IRouterBuilder Route, string Path, Func<IHttpContext, IHttpAction> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("GET", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on POST method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnPost(this IRouterBuilder Route, string Path, Func<IHttpContext, IHttpAction> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("POST", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on PUT method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnPut(this IRouterBuilder Route, string Path, Func<IHttpContext, IHttpAction> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("PUT", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on PATCH method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnPatch(this IRouterBuilder Route, string Path, Func<IHttpContext, IHttpAction> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("PATCH", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on DELETE method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnDelete(this IRouterBuilder Route, string Path, Func<IHttpContext, IHttpAction> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("DELETE", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on all methods.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnAny(this IRouterBuilder Route, Func<IHttpContext, IHttpAction> Endpoint)
            => Route.Path(string.Empty, Subroute => Subroute.Map("*", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on HEAD method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnHead(this IRouterBuilder Route, Func<IHttpContext, IHttpAction> Endpoint)
            => Route.Path(string.Empty, Subroute => Subroute.Map("HEAD", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on OPTIONS method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnOptions(this IRouterBuilder Route, Func<IHttpContext, IHttpAction> Endpoint)
            => Route.Path(string.Empty, Subroute => Subroute.Map("OPTIONS", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on GET method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnGet(this IRouterBuilder Route, Func<IHttpContext, IHttpAction> Endpoint)
            => Route.Path(string.Empty, Subroute => Subroute.Map("GET", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on POST method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnPost(this IRouterBuilder Route, Func<IHttpContext, IHttpAction> Endpoint)
            => Route.Path(string.Empty, Subroute => Subroute.Map("POST", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on PUT method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnPut(this IRouterBuilder Route, Func<IHttpContext, IHttpAction> Endpoint)
            => Route.Path(string.Empty, Subroute => Subroute.Map("PUT", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on PATCH method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnPatch(this IRouterBuilder Route, Func<IHttpContext, IHttpAction> Endpoint)
            => Route.Path(string.Empty, Subroute => Subroute.Map("PATCH", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on DELETE method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnDelete(this IRouterBuilder Route, Func<IHttpContext, IHttpAction> Endpoint)
            => Route.Path(string.Empty, Subroute => Subroute.Map("DELETE", Endpoint));

        /// <summary>
        /// Get the route path from the <see cref="MemberInfo"/>.
        /// </summary>
        /// <param name="Where"></param>
        /// <returns></returns>
        private static string GetRoutePath(this MemberInfo Where)
        {
            var Route = Where.GetCustomAttribute<RouteAttribute>();
            return Route != null ? Route.Path : "";
        }

        /// <summary>
        /// Create a controller instance once.
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        internal static object GetController(this IHttpRequest Request, Type Type)
        {
            Request.Properties.TryGetValue(Type, out var Instance);

            if (Instance is null)
            {
                Instance = Request.Services
                    .GetRequiredService<IServiceInjector>()
                    .Create(Type);

                if (Instance is IDisposable Sync)
                    Request.Aborted.Register(Sync.Dispose);

                else if (Instance is IAsyncDisposable Async)
                {
                    Request.Aborted.Register(() => Async
                        .DisposeAsync().GetAwaiter().GetResult());
                }

                Request.Properties[Type] = Instance;
            }

            return Instance;
        }

        /// <summary>
        /// Make an endpoint that invokes the controller.
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Method"></param>
        /// <returns></returns>
        private static Func<IHttpContext, Task<IHttpAction>> MakeEndpoint(Type Type, MethodInfo Method)
        {
            return async Http =>
            {
                var Context = MakeControllerContext(Type, Method, Http);
                var Filters = Context.TargetType.GetCustomAttributes()
                    .Select(X => X as IHttpActionFilter).Where(X => X != null);

                await (Filters.MakeFilterDelegate() ?? EmptyFilterImpl)(Context, () =>
                {
                    if (Context.TargetInstance is Controller Controller)
                        return Controller.OnEndpointExecutionInternal(Context);

                    return Task.CompletedTask;
                });

                return Context.Action;
            };
        }

        /// <summary>
        /// Empty filter implementation.
        /// </summary>
        /// <param name="_"></param>
        /// <param name="Next"></param>
        /// <returns></returns>
        [DebuggerHidden]
        private static Task EmptyFilterImpl(ControllerContext _, Func<Task> Next) => Next();

        /// <summary>
        /// Make the controller context.
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Method"></param>
        /// <param name="Http"></param>
        /// <returns></returns>
        private static ControllerContext MakeControllerContext(Type Type, MethodInfo Method, IHttpContext Http)
        {
            var Request = Http.Request;
            var Context = new ControllerContext(Http);
            var Instance = Method.IsStatic ? null : Request.GetController(Type);

            Context.TargetType = Type;
            Context.TargetMethod = Method;
            Context.TargetInstance = Instance;
            Context.Action = new MethodInvoke(Context);
            return Context;
        }

        /// <summary>
        /// Make filter delegate.
        /// </summary>
        /// <param name="Filters"></param>
        /// <returns></returns>
        internal static Func<ControllerContext, Func<Task>, Task> MakeFilterDelegate(this IEnumerable<IHttpActionFilter> Filters)
        {
            var Filter = null as Func<ControllerContext, Func<Task>, Task>;

            foreach (var Each in Filters)
            {
                if (Filter != null)
                    Filter = new Middleware<ControllerContext>(Filter, Each.OnFilterAsync).InvokeAsync;

                else
                    Filter = Each.OnFilterAsync;
            }

            return Filter;
        }
    }
}
