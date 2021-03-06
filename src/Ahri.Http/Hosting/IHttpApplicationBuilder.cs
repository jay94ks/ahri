using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ahri.Http.Hosting
{
    public interface IHttpApplicationBuilder
    {
        /// <summary>
        /// A central location that is used to share datas between host building process.
        /// </summary>
        IDictionary<object, object> Properties { get; }

        /// <summary>
        /// Application Services.
        /// </summary>
        IServiceProvider ApplicationServices { get; }

        /// <summary>
        /// Use the content deserializer to interpret request content.
        /// </summary>
        /// <param name="Configure"></param>
        /// <returns></returns>
        IHttpApplicationBuilder UseContent(Action<IHttpContentDeserializerBuilder> Configure);

        /// <summary>
        /// Adds a delegate that handles the <see cref="IHttpContext"/> instance.
        /// </summary>
        /// <param name="Middleware"></param>
        /// <returns></returns>
        IHttpApplicationBuilder Use(Func<IHttpContext, Func<Task>, Task> Middleware);

        /// <summary>
        /// Adds a delegate that creates the middleware delegate.
        /// </summary>
        /// <param name="Factory"></param>
        /// <returns></returns>
        IHttpApplicationBuilder Use(Func<Func<IHttpContext, Func<Task>, Task>> Factory);

        /// <summary>
        /// Adds a delegate that configure the services that related with middlewares.
        /// </summary>
        /// <param name="Configure"></param>
        /// <returns></returns>
        IHttpApplicationBuilder Configure(Action<IServiceProvider> Configure);

        /// <summary>
        /// Builds the <see cref="IHttpApplication"/> instance.
        /// </summary>
        /// <returns></returns>
        IHttpApplication Build();
    }
}
