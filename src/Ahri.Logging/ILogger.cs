using System;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Logging
{
    /// <summary>
    /// Log a message.
    /// </summary>
    public interface ILogger : IDisposable
    {
        /// <summary>
        /// Log a message.
        /// </summary>
        /// <param name="Level"></param>
        /// <param name="Message"></param>
        /// <param name="Error"></param>
        void Log(LogLevel Level, string Message, Exception Error = null);
    }

    /// <summary>
    /// To get a logger that uses <typeparamref name="TCategory"/> as category name.
    /// </summary>
    /// <typeparam name="TCategory"></typeparam>
    public interface ILogger<TCategory> : ILogger
    {
    }
}
