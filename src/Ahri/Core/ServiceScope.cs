using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ahri.Core
{
    public partial class ServiceScope : IServiceScope, IServiceScopeFactory
    {
        /* Hierarchy. */
        private ServiceScope m_Parent;
        private bool m_Disposed;

        /* Registrations and its instances. */
        private Dictionary<Type, IServiceDescriptor> m_Services = new();
        private Dictionary<Type, Task<object>> m_Instances = new();
        private Dictionary<Type, Task<object>> m_Overrides = new();

        /* Parameter resolves for the scope. */
        private Injector m_Injector;

        /* Lifetime.  */
        private Stack<object> m_Disposables = new();

        /// <summary>
        /// Initialize a new <see cref="ServiceScope"/> with the service registrations.
        /// </summary>
        /// <param name="Services"></param>
        internal ServiceScope(IServiceCollection Services)
        {
            if (Services != null)
            {
                foreach (var Each in Services)
                    m_Services[Each.ServiceType] = Each;
            }

            m_Injector = new Injector(this, Services);
            ServiceProvider = new Lookup(this);

            SetOverrides<IServiceScope>(this);
            SetOverrides<IServiceInjector>(m_Injector);
            SetOverrides<IServiceScopeFactory>(this);
            SetOverrides<IServiceProvider>(ServiceProvider);
        }

        /// <summary>
        /// Initialize a new <see cref="ServiceScope"/> with the service registrations.
        /// </summary>
        /// <param name="Parent"></param>
        /// <param name="Services"></param>
        private ServiceScope(ServiceScope Parent, IServiceCollection Services) : this(Services)
        {
            if ((m_Parent = Parent) != null)
                m_Parent.m_Disposables.Push(this);
        }

        /// <inheritdoc/>
        public IServiceProvider ServiceProvider { get; }

        /// <inheritdoc/>
        public IServiceScope CreateScope() => new ServiceScope(this, null);

        /// <inheritdoc/>
        public IServiceScope CreateScope(IServiceCollection Services) => new ServiceScope(this, Services);

        /// <summary>
        /// Try to get the overriden instance of the service.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="Instance"></param>
        /// <returns></returns>
        internal bool TryGetOverrides(Type ServiceType, out Task<object> Instance)
            => m_Overrides.TryGetValue(ServiceType, out Instance);

        /// <summary>
        /// Set Overrides that specialized for the scope.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="Instance"></param>
        internal ServiceScope SetOverrides<T>(object Instance)
        {
            m_Overrides[typeof(T)] = Task.FromResult(Instance);
            m_Services.Remove(typeof(T));
            return this;
        }

        /// <summary>
        /// Try to get an instance of the service.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="Instance"></param>
        /// <returns></returns>
        internal bool TryGetService(Type ServiceType, out Task<object> Instance)
        {
            lock (this)
            {
                if (m_Instances.TryGetValue(ServiceType, out Instance))
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Try to set an instance of the service.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="InstanceSource"></param>
        /// <returns></returns>
        internal bool TrySetService(Type ServiceType, out TaskCompletionSource<object> InstanceSource)
        {
            lock (this)
            {
                if (m_Instances.TryGetValue(ServiceType, out _))
                {
                    InstanceSource = null;
                    return false;
                }

                m_Instances[ServiceType] = (InstanceSource = new TaskCompletionSource<object>()).Task;
                return true;
            }
        }

        /// <summary>
        /// Find an <see cref="IServiceDescriptor"/> instance and its origin.
        /// </summary>
        /// <param name="Service"></param>
        /// <param name="Origin"></param>
        /// <returns></returns>
        internal bool TryFind(Type Service, out IServiceDescriptor Descriptor, out ServiceScope Origin)
        {
            if (m_Services.TryGetValue(Service, out Descriptor))
            {
                Origin = this;
                return true;
            }

            if (Service.IsConstructedGenericType)
            {
                var Generic = Service.GetGenericTypeDefinition();

                if (m_Services.TryGetValue(Generic, out Descriptor))
                {
                    Origin = this;
                    return true;
                }
            }

            if (m_Parent != null)
                return m_Parent.TryFind(Service, out Descriptor, out Origin);

            Origin = null;
            return false;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (this)
            {
                m_Disposed = true;
                m_Overrides.Clear();
            }

            WaitIncompletedInstances();
            KillInstances();
        }

        /// <summary>
        /// Wait incompleted instance holders.
        /// </summary>
        private void WaitIncompletedInstances()
        {
            var Queue = new Queue<Task<object>>();
            while (true)
            {
                lock (this)
                {
                    foreach (var Each in m_Instances.Values)
                        Queue.Enqueue(Each);

                    if (Queue.Count <= 0)
                        break;

                    m_Instances.Clear();
                }

                while (Queue.TryDequeue(out var Each))
                {
                    try { Each.Wait(); }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Then, kill instance.
        /// </summary>
        private void KillInstances()
        {
            object Instance;
            while (true)
            {
                lock (this)
                {
                    if (!m_Disposables.TryPop(out Instance))
                        break;
                }

                if (Instance is IDisposable Sync) Sync.Dispose();
                else if (Instance is IAsyncDisposable Async)
                    Async.DisposeAsync().GetAwaiter().GetResult();
            }
        }
    }
}
