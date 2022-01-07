using Ahri.Http.Core.Middlewares;
using Ahri.Http.Core.Routing;
using Ahri.Http.Core.Routing.Internals;
using Ahri.Http.Hosting;
using Ahri.Values;
using System;
using System.Reflection;

namespace Ahri.Http.Core
{
    public static class RoutingExtensions
    {
        /// <summary>
        /// Use the <typeparamref name="TBuilder"/> to handle the request.
        /// </summary>
        /// <typeparam name="TBuilder"></typeparam>
        /// <returns></returns>
        public static TBuilder Use<TBuilder>(this IHttpApplicationBuilder App, Action<TBuilder> Configure = null)
            where TBuilder : IHttpMiddlewareBuilder
        {
            App.Properties.TryGetValue(typeof(TBuilder), out var Temp);
            if (!(Temp is TBuilder Builder))
            {
                var Injector = App.ApplicationServices.GetRequiredService<IServiceInjector>();
                App.Properties[typeof(TBuilder)] = Builder = (TBuilder)Injector.Create(typeof(TBuilder));
                App.Use(Builder.Build);
            }

            Configure?.Invoke(Builder);
            return Builder;
        }

        /// <summary>
        /// Use the <see cref="StaticFiles"/> to provide static files.
        /// </summary>
        /// <param name="App"></param>
        /// <param name="Configure"></param>
        /// <returns></returns>
        public static StaticFiles UseStaticFiles(this IHttpApplicationBuilder App, Action<StaticFiles> Configure = null) => App.Use(Configure);

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
        /// Enable Http Parameter Injection on the <see cref="IServiceInjector"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IServiceCollection EnableHttpParameterInjection(this IServiceCollection This)
        {
            if (This.Resolvers.Contains(OnResolveHttpParameters))
                return This;

            This.Resolvers.Add(OnResolveHttpParameters);
            return This;
        }

        /// <summary>
        /// Resolve Http Parameters.
        /// </summary>
        /// <param name="Param"></param>
        /// <param name="Services"></param>
        /// <returns></returns>
        private static object OnResolveHttpParameters(ParameterInfo Param, IServiceProvider Services)
        {
            var ParamType = Param.ParameterType;

            var Content = Param.GetCustomAttribute<FromContentAttribute>();
            var Path = Param.GetCustomAttribute<FromPathAttribute>();
            var Query = Param.GetCustomAttribute<FromQueriesAttribute>();

            if (Content != null)
            {
                var Context = Services.GetRequiredService<IHttpContextAccessor>().Instance;
                var Deserializer = Services.GetRequiredService<IHttpContentDeserializer>();

                var Value = Context.Request.Content;
                if (Value != null && Deserializer.CanConvert(Value, ParamType))
                {
                    var Result = Deserializer.Convert(Value, ParamType);
                    if (Result != null) return Result;
                }
            }

            if (Path != null)
            {
                var Context = Services.GetRequiredService<IHttpContextAccessor>().Instance;
                var Name = string.IsNullOrWhiteSpace(Path.Name) ? Param.Name : Path.Name;
                var State = Context.GetRouterState();

                if (State.PathParameters.TryGetValue(Name, out var RetVal))
                    return RetVal ?? "";
            }

            if (Query != null)
            {
                var Context = Services.GetRequiredService<IHttpContextAccessor>().Instance;
                var Name = string.IsNullOrWhiteSpace(Query.Name) ? Param.Name : Query.Name;

                var RetVal = FromQueryResult(ParamType, Context, Name);
                if (RetVal != null)
                    return RetVal;
            }

            return null;
        }

        /// <summary>
        /// Resolve the parameter from the query result.
        /// </summary>
        /// <param name="ParamType"></param>
        /// <param name="Context"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        private static object FromQueryResult(Type ParamType, IHttpContext Context, string Name)
        {
            if (Context.Request.Queries.TryGetValue(Name, out var Value) &&
                ValueConverter.Default.TryConvert(Value, ParamType, out var RetVal))
            {
                if (ParamType == typeof(string))
                    RetVal = RetVal ?? "";

                return RetVal;
            }

            return null;
        }

        /// <summary>
        /// Get the <see cref="IRouterState"/> instance from <see cref="IHttpRequest"/> instance.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IRouterState GetRouterState(this IHttpContext This) => RouterState.Get(This);
    }
}
