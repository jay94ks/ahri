using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ahri.Core
{
    public partial class ServiceScope
    {
        internal partial class Lookup : IServiceProvider
        {
            private ServiceScope m_Scope;

            /// <summary>
            /// Initialize a new <see cref="Lookup"/> instance. 
            /// </summary>
            /// <param name="Scope"></param>
            public Lookup(ServiceScope Scope) => m_Scope = Scope;

            /// <summary>
            /// Provides the service on the current scope.
            /// </summary>
            /// <param name="ServiceType"></param>
            /// <returns></returns>
            public object GetService(Type ServiceType) 
            {
                /* Is asynchronous service request? */
                if (ServiceType.IsConstructedGenericType)
                {
                    if (ServiceType.GetGenericTypeDefinition() == typeof(Task<>))
                        return GetServiceAsTask(ServiceType);

                }

                Task<object> Holder = null;

                if (m_Scope.TryGetOverrides(ServiceType, out var Temp))
                    return Temp.Result;

                if (m_Scope.TryFind(ServiceType, out var Descriptor, out var Origin))
                    Holder = GetService(ServiceType, Descriptor, Origin);

                return Holder != null ? Holder.Result : null;
            }

            /// <summary>
            /// Provides the service on the current scope as holder as-is.
            /// </summary>
            /// <param name="ServiceType"></param>
            /// <returns></returns>
            private object GetServiceAsTask(Type ServiceType)
            {
                var Holder = null as Task<object>;
                var RequestedType = ServiceType.GetGenericArguments().First();
                var Method = typeof(Lookup)
                    .GetMethod(nameof(MakeGeneric), BindingFlags.Static | BindingFlags.NonPublic)
                    .MakeGenericMethod(RequestedType);

                if (m_Scope.TryFind(RequestedType, out var Descriptor, out var Origin))
                    Holder = GetService(RequestedType, Descriptor, Origin);

                return Method.Invoke(null, new[] { Holder });
            }

            /// <summary>
            /// Make the plain object task to <typeparamref name="T"/> task.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="NonGeneric"></param>
            /// <returns></returns>
            private static async Task<T> MakeGeneric<T>(Task<object> NonGeneric)
            {
                if (NonGeneric is null)
                    return default;

                return (T)(await NonGeneric);
            }

            /// <summary>
            /// Provides the service on the current scope with circular dependency detection.
            /// </summary>
            /// <param name="Descriptor"></param>
            /// <returns></returns>
            private Task<object> GetService(Type RequestedType, IServiceDescriptor Descriptor, ServiceScope Origin)
            {
                lock (this)
                {
                    if (m_Scope.m_Disposed)
                        return null;

                    if (m_Scope.TryGetOverrides(RequestedType, out var Temp))
                        return Temp;

                    /* If it is singleton, go up.*/
                    if (Origin != m_Scope && Descriptor.Lifetime == ServiceLifetime.Singleton)
                        return (Origin.ServiceProvider as Lookup).GetService(RequestedType, Descriptor, Origin);

                    if (Descriptor.Lifetime != ServiceLifetime.Transient)
                    {
                        if (LookupInstances(RequestedType, Origin, out var Instance))
                            return Instance; /* Look-up parents first. */

                        if (Descriptor.Lifetime == ServiceLifetime.Scoped)
                            Origin = m_Scope; /* --> change the origin to current. */

                        TaskCompletionSource<object> Holder;
                        lock (Origin)
                        {
                            if (!Origin.TrySetService(RequestedType, out Holder))
                            {
                                /* Holder is already configured. */
                                Origin.TryGetService(RequestedType, out Instance);
                                return Instance;
                            }
                        }

                        try { Holder.SetResult(CreateInstance(RequestedType, Descriptor, Origin)); }
                        finally
                        {
                            if (!Holder.Task.IsCompleted)
                                 Holder.TrySetCanceled();
                        }

                        return Holder.Task;
                    }

                    return Task.FromResult(CreateInstance(RequestedType, Descriptor, m_Scope));
                }
            }

            /// <summary>
            /// Create an instance of the requested type.
            /// </summary>
            /// <param name="RequestedType"></param>
            /// <param name="Descriptor"></param>
            /// <param name="Origin"></param>
            /// <returns></returns>
            private object CreateInstance(Type RequestedType, IServiceDescriptor Descriptor, ServiceScope Origin)
            {
                var Value = Descriptor.Create(Origin.ServiceProvider, RequestedType);
                if (Value is IDisposable || Value is IAsyncDisposable)
                {
                    lock (Origin.m_Disposables)
                        Origin.m_Disposables.Push(Value);
                }

                return Value;
            }

            /// <summary>
            /// Lookup the service from parents.
            /// </summary>
            /// <param name="RequestedType"></param>
            /// <param name="Origin"></param>
            /// <returns></returns>
            private bool LookupInstances(Type RequestedType, ServiceScope Origin, out Task<object> Instance)
            {
                var Current = m_Scope;

                /* Lookup the instance from parents first. */
                while (Current != null)
                {
                    if (Current.TryGetService(RequestedType, out Instance))
                        return true;

                    if (Current == Origin)
                        break;

                    Current = Current.m_Parent;
                }

                Instance = null;
                return false;
            }
        }
    }
}
