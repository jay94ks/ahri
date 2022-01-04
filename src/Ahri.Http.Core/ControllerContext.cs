using System;
using System.Reflection;

namespace Ahri.Http.Core
{
    public sealed class ControllerContext
    {
        /// <summary>
        /// Initialize a new <see cref="ControllerContext"/> instance.
        /// </summary>
        /// <param name="HttpContext"></param>
        public ControllerContext(IHttpContext HttpContext)
            => this.HttpContext = HttpContext;

        /// <summary>
        /// Target Type.
        /// </summary>
        public Type TargetType { get; internal set; }

        /// <summary>
        /// Target Method to invoke.
        /// </summary>
        public MethodInfo TargetMethod { get; internal set; }

        /// <summary>
        /// Target Instance.
        /// Note that, If the target method is static, this will be null.
        /// </summary>
        public object TargetInstance { get; internal set; }

        /// <summary>
        /// Http Context.
        /// </summary>
        public IHttpContext HttpContext { get; }

        /// <summary>
        /// Action to invoke.
        /// </summary>
        public IHttpAction Action { get; set; }
    }
}
