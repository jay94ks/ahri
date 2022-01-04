using System;

namespace Ahri.Http.Core.Routing
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
    public class MethodRouteAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new <see cref="MethodRouteAttribute"/>
        /// </summary>
        /// <param name="Method"></param>
        public MethodRouteAttribute(string Method) => this.Method = Method;

        /// <summary>
        /// Request method.
        /// </summary>
        public string Method { get; }
    }
}
