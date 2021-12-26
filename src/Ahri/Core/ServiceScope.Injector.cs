using System;
using System.Linq;
using System.Reflection;

namespace Ahri.Core
{
    public partial class ServiceScope
    {
        internal class Injector : IServiceInjector
        {
            private ServiceScope m_Scope;
            private Func<ParameterInfo, IServiceProvider, object>[] m_Resolvers;

            /// <summary>
            /// Initialize a new <see cref="Injector"/> instance.
            /// </summary>
            /// <param name="Scope"></param>
            /// <param name="Services"></param>
            public Injector(ServiceScope Scope, IServiceCollection Services)
            {
                if (Services != null)
                    m_Resolvers = Services.Resolvers.ToArray();

                m_Scope = Scope;
            }

            /// <inheritdoc/>
            public object Create(Type TargetType, params object[] Arguments) 
                => Create(m_Scope.ServiceProvider, TargetType, Arguments);

            /// <inheritdoc/>
            public object Invoke(MethodInfo Method, object Target, params object[] Arguments)
                => Invoke(m_Scope.ServiceProvider, Method, Target, Arguments);

            /// <inheritdoc/>
            public object Resolve(ParameterInfo Parameter)
                => Resolve(m_Scope.ServiceProvider, Parameter);

            /// <summary>
            /// Resolve an instance by parameter.
            /// </summary>
            /// <param name="Services"></param>
            /// <param name="Parameter"></param>
            /// <returns></returns>
            internal object ResolveHalf(IServiceProvider Services, ParameterInfo Parameter)
            {
                if (m_Resolvers != null)
                {
                    foreach (var Each in m_Resolvers)
                    {
                        var Value = Each(Parameter, Services);
                        if (Value is null)
                            continue;

                        return Value;
                    }
                }

                if (m_Scope.m_Parent != null) /* Resolve from parents if unresolved. */
                    return m_Scope.m_Parent.m_Injector.ResolveHalf(Services, Parameter);

                return null;
            }

            /// <summary>
            /// Resolve an instance from the scope and parent scopes.
            /// </summary>
            /// <param name="Services"></param>
            /// <param name="Parameter"></param>
            /// <returns></returns>
            internal object Resolve(IServiceProvider Services, ParameterInfo Parameter) 
                => ResolveHalf(Services, Parameter) ?? Services.GetService(Parameter.ParameterType);

            /// <summary>
            /// Create an instance.
            /// </summary>
            /// <param name="Services"></param>
            /// <param name="TargetType"></param>
            /// <param name="Arguments"></param>
            /// <returns></returns>
            internal object Create(IServiceProvider Services, Type TargetType, params object[] Arguments)
                => TargetType.CreateWithInjection(Param => Resolve(Services, Param), Arguments);

            /// <summary>
            /// Invoke a method.
            /// </summary>
            /// <param name="Services"></param>
            /// <param name="Method"></param>
            /// <param name="Target"></param>
            /// <param name="Arguments"></param>
            /// <returns></returns>
            internal object Invoke(IServiceProvider Services, MethodInfo Method, object Target, params object[] Arguments)
                => Method.InvokeWithInjection(Target, Param => Resolve(Services, Param), Arguments);
        }
    }
}
