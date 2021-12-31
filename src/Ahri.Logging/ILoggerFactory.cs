namespace Ahri.Logging
{
    /// <summary>
    /// Creates a logger with its category name.
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// Create a logger.
        /// </summary>
        /// <returns></returns>
        ILogger CreateLogger(string Category);
    }
}
