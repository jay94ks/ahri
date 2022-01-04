using Ahri.Http.Core.Routing;
using Ahri.Http.Core.Routing.Internals;
using Ahri.Http.Hosting;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ahri.Http.Core
{
    public static class RoutingExtensions
    {
        /// <summary>
        /// Use the <see cref="IRouterBuilder"/> to handle the request.
        /// </summary>
        /// <param name="App"></param>
        /// <param name="Configure"></param>
        /// <returns></returns>
        public static IRouterBuilder UseRouting(this IHttpApplicationBuilder App, Action<IRouterBuilder> Configure = null)
        {
            App.Properties.TryGetValue(typeof(IRouterBuilder), out var Temp);
            if (!(Temp is RouterBuilder Route))
            {
                App.Properties[typeof(IRouterBuilder)] = Route = new RouterBuilder();
                App.Use(() =>
                {
                    IRouter Router = Route.Build();
                    return (Http, Next) => Router.InvokeAsync(Http);
                });
            }

            Configure?.Invoke(Route);
            return Route;
        }

        /// <summary>
        /// Map a controller to the route.
        /// </summary>
        /// <param name="Route"></param>
        /// <returns></returns>
        public static IRouterBuilder Map(this IRouterBuilder Route, Type Type)
        {
            var Paths   = Type.GetRoutePath();
            var Methods = Type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                  .Concat(Type.GetMethods(BindingFlags.Public | BindingFlags.Static));

            foreach(var Each in Methods)
            {
                var Endpath = Each.GetRoutePath();
                if (Endpath is null)
                    continue;

                var Endpoint = MakeEndpoint(Type, Each);
                if (!Endpath.StartsWith('/'))
                     Endpath = $"{Paths.TrimEnd('/')}/{Endpath}";

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
        public static IRouterBuilder OnAny(this IRouterBuilder Route, string Path, Func<IHttpContext, Task> Endpoint) 
            => Route.Path(Path, Subroute => Subroute.Map("*", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on HEAD method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnHead(this IRouterBuilder Route, string Path, Func<IHttpContext, Task> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("HEAD", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on OPTIONS method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnOptions(this IRouterBuilder Route, string Path, Func<IHttpContext, Task> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("OPTIONS", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on GET method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnGet(this IRouterBuilder Route, string Path, Func<IHttpContext, Task> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("GET", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on POST method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnPost(this IRouterBuilder Route, string Path, Func<IHttpContext, Task> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("POST", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on PUT method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnPut(this IRouterBuilder Route, string Path, Func<IHttpContext, Task> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("PUT", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on PATCH method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnPatch(this IRouterBuilder Route, string Path, Func<IHttpContext, Task> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("PATCH", Endpoint));

        /// <summary>
        /// Map an endpoint to specific path on DELETE method.
        /// </summary>
        /// <param name="Route"></param>
        /// <param name="Path"></param>
        /// <param name="Endpoint"></param>
        /// <returns></returns>
        public static IRouterBuilder OnDelete(this IRouterBuilder Route, string Path, Func<IHttpContext, Task> Endpoint)
            => Route.Path(Path, Subroute => Subroute.Map("DELETE", Endpoint));

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
        private static object GetController(IHttpRequest Request, Type Type)
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
        private static Func<IHttpContext, Task> MakeEndpoint(Type Type, MethodInfo Method)
        {
            return async Http =>
            {
                var Request = Http.Request;
                var Injector = Request.Services
                    .GetRequiredService<IServiceInjector>();

                var Instance = Method.IsStatic ? null : GetController(Request, Type);
                var Result = Injector.Invoke(Method, Instance);
                var Action = null as IHttpAction;

                if (Result is Task<IHttpAction> ActionAsync)
                     Action = await ActionAsync;
                else Action = Result as IHttpAction;

                if (Action != null)
                    await Action.InvokeAsync(Http);
            };
        }
    }
}
