using System.Collections.Generic;

namespace Ahri.Http.Core.Routing
{
    /// <summary>
    /// Router State interface.
    /// </summary>
    public interface IRouterState
    {
        /// <summary>
        /// Router instance.
        /// </summary>
        IRouter Router { get; }

        /// <summary>
        /// Current scoped path spaces.
        /// </summary>
        IEnumerable<string> CurrentPathSpaces { get; }

        /// <summary>
        /// Pending path spaces to route.
        /// </summary>
        IEnumerable<string> PendingPathSpaces { get; }

        /// <summary>
        /// Collected path parameters.
        /// </summary>
        IDictionary<string, string> PathParameters { get; }
    }
}
