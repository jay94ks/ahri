using System;
using System.Reflection;

namespace Ahri
{
    /// <summary>
    /// Abstracts the service dependency injector.
    /// </summary>
    public interface IServiceInjector
    {
        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="TargetType"></param>
        /// <param name="Arguments"></param>
        /// <returns></returns>
        object Create(Type TargetType, params object[] Arguments);

        /// <summary>
        /// Invoke an method with dependency injection.
        /// </summary>
        /// <param name="Method"></param>
        /// <param name="Target"></param>
        /// <param name="Arguments"></param>
        /// <returns></returns>
        object Invoke(MethodInfo Method, object Target, params object[] Arguments);

        /// <summary>
        /// Resolve an instance for covering the parameter information.
        /// </summary>
        /// <param name="Parameter"></param>
        /// <returns></returns>
        object Resolve(ParameterInfo Parameter);
    }
}
