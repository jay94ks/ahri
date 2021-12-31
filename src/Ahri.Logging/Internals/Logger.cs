using System;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Logging.Internals
{
    /// <summary>
    /// Broadcast a log to all logger implementations.
    /// </summary>
    public class Logger : ILogger, IAsyncDisposable
    {
        private ILogger[] m_Loggers;

        /// <summary>
        /// Initialize a new <see cref="Logger"/> interface.
        /// </summary>
        /// <param name="Loggers"></param>
        public Logger(ILogger[] Loggers) => m_Loggers = Loggers;

        /// <inheritdoc/>
        public void Log(LogLevel Level, string Message, Exception Error = null)
        {
            foreach (var Each in m_Loggers)
                Each.Log(Level, Message, Error);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var Each in m_Loggers)
            {
                if (Each is IDisposable Sync)
                    Sync.Dispose();

                else if (Each is IAsyncDisposable Async)
                    Async.DisposeAsync().GetAwaiter().GetResult();
            }
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            foreach (var Each in m_Loggers)
            {
                if (Each is IAsyncDisposable Async)
                    await Async.DisposeAsync();

                else if (Each is IDisposable Sync)
                    Sync.Dispose();
            }
        }
    }

    /// <summary>
    /// Generic Logger.
    /// </summary>
    /// <typeparam name="TCategory"></typeparam>
    public class Logger<TCategory> : ILogger<TCategory>, IAsyncDisposable
    {
        private ILogger m_Logger;

        /// <summary>
        /// Initialize a new <see cref="ILogger{TCategory}"/>.
        /// </summary>
        /// <param name="Factory"></param>
        public Logger(ILoggerFactory Factory)
            => m_Logger = Factory.CreateLogger(typeof(TCategory).FullName);

        /// <inheritdoc/>
        public void Log(LogLevel Level, string Message, Exception Error = null)
            => m_Logger.Log(Level, Message, Error);

        /// <inheritdoc/>
        public void Dispose()
        {
            if (m_Logger is IDisposable Sync)
                Sync.Dispose();

            else if (m_Logger is IAsyncDisposable Async)
                Async.DisposeAsync().GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (m_Logger is IAsyncDisposable Async)
                await Async.DisposeAsync();

            else if (m_Logger is IDisposable Sync)
                Sync.Dispose();
        }
    }
}
