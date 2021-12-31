using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ahri.Logging.Internals
{
    public class LoggerFactory : ILoggerFactory, IAsyncDisposable, IDisposable
    {
        private ILoggerFactory[] m_LoggerFactories;
        private IServiceProvider m_LoggingServices;

        /// <summary>
        /// Initialize a new <see cref="LoggerFactory"/> instance.
        /// </summary>
        /// <param name="LoggingServices"></param>
        /// <param name="LoggerFactoryTypes"></param>
        public LoggerFactory(IServiceProvider LoggingServices, IEnumerable<Type> LoggerFactoryTypes)
        {
            m_LoggingServices = LoggingServices;
            m_LoggerFactories = LoggerFactoryTypes
                .Select(X => LoggingServices.GetService(X) as ILoggerFactory)
                .Where(X => X != null).ToArray();
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string Category)
            => new Logger(m_LoggerFactories.Select(X => X.CreateLogger(Category)).ToArray());

        /// <inheritdoc/>
        public void Dispose()
        {
            if (m_LoggingServices is IDisposable Sync)
                Sync.Dispose();

            else if (m_LoggingServices is IAsyncDisposable Async)
                Async.DisposeAsync().GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (m_LoggingServices is IAsyncDisposable Async)
                await Async.DisposeAsync();

            else if (m_LoggingServices is IDisposable Sync)
                Sync.Dispose();
        }
    }
}
