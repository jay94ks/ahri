using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Routing
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
    public class RouteAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new <see cref="RouteAttribute"/> that will be mapped on the router.
        /// </summary>
        /// <param name="Path"></param>
        public RouteAttribute(string Path) => this.Path = Path;

        /// <summary>
        /// Path String to be mapped.
        /// </summary>
        public string Path { get; }
    }
}
