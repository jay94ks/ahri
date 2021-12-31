using Ahri.Hosting;
using Ahri.Logging.Internals;
using Ahri.Logging.Internals.Defaults;
using System;
using System.Linq;

namespace Ahri.Logging
{
    public static class LoggingExtensions
    {
        private static readonly Type[] CTOR_ARGS = new Type[] { typeof(ILoggerFactory) };

        /// <summary>
        /// Make Generic Logger.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        private static object MakeGenericLogger(IServiceProvider Services, Type Type)
        {
            var Factory = Services.GetRequiredService<ILoggerFactory>();
            return typeof(Logger<>)
                .MakeGenericType(Type.GetGenericArguments().First())
                .GetConstructor(CTOR_ARGS).Invoke(new object[] { Factory });
        }

        /// <summary>
        /// Configure the logging utilities for the <see cref="IHostBuilder"/> instance.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Configure"></param>
        /// <returns></returns>
        public static IHostBuilder ConfigureLogging(this IHostBuilder This, Action<ILoggerFactoryBuilder> Configure = null)
        {
            This.Properties.TryGetValue(typeof(ILoggerFactoryBuilder), out var Temp);
            if (Temp is null)
            {
                This.Properties[typeof(ILoggerFactoryBuilder)] = Temp = new LoggerFactoryBuilder()
                    .Use<ConsoleLoggerFactory>();

                This.ConfigureServices(X =>
                {
                    X.AddSingleton<ILoggerFactory>(Y => (Temp as ILoggerFactoryBuilder).Build());
                    X.AddScoped(typeof(ILogger<>), MakeGenericLogger);
                });
            }

            Configure?.Invoke(Temp as ILoggerFactoryBuilder);
            return This;
        }

        /// <summary>
        /// Use the console logger to display log messages.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static ILoggerFactoryBuilder UseConsoleLogger(this ILoggerFactoryBuilder This) => This.Use<ConsoleLoggerFactory>();
    }
}
