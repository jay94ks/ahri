using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ahri.Orp.Hosting
{
    public interface IOrpApplicationBuilder
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
        /// Adds a delegate that invokes Greetings operations that initiate the communication states.
        /// </summary>
        /// <param name="Greetings"></param>
        /// <returns></returns>
        IOrpApplicationBuilder UseGreetings(Func<IOrpConnection, Task> Greetings);

        /// <summary>
        /// Adds a delegate that invokes Farewell operations that finialize the communication states.
        /// When this delegates are called, <see cref="OrpClient"/> instance may be aborted.
        /// </summary>
        /// <param name="Farewells"></param>
        /// <returns></returns>
        IOrpApplicationBuilder UseFarewells(Func<IOrpConnection, Task> Farewells);

        /// <summary>
        /// Adds a delegate that process the orp context in middle.
        /// </summary>
        /// <param name="Middleware"></param>
        /// <returns></returns>
        IOrpApplicationBuilder Use(Func<IOrpContext, Func<Task>, Task> Middleware);

        /// <summary>
        /// Adds a delegate that configure the services that related with middlewares.
        /// </summary>
        /// <param name="Configure"></param>
        /// <returns></returns>
        IOrpApplicationBuilder Configure(Action<IServiceProvider> Configure);

        /// <summary>
        /// Builds the orp application to single delegate.
        /// </summary>
        /// <returns></returns>
        Func<IOrpContext, Task> Build();
    }
}
