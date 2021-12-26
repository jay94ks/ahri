using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Core.Descriptors
{
    public abstract class Base : IServiceDescriptor
    {
        /// <summary>
        /// Initialize a new <see cref="IServiceDescriptor"/>.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="ImplementationType"></param>
        public Base(Type ServiceType, Type ImplementationType = null)
        {
            this.ServiceType = ServiceType;
            this.ImplementationType = ImplementationType ?? ServiceType;
        }

        /// <summary>
        /// Initialize a new <see cref="IServiceDescriptor"/>.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="ImplementationInstance"></param>
        public Base(Type ServiceType, object ImplementationInstance)
        {
            this.ServiceType = ServiceType;
            this.ImplementationInstance = ImplementationInstance;
        }

        /// <summary>
        /// Initialize a new <see cref="IServiceDescriptor"/>.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        public Base(Type ServiceType, Func<IServiceProvider, object> Factory)
        {
            this.ServiceType = ServiceType;
            this.Factory = (Services, _) => Factory(Services);
        }

        /// <summary>
        /// Initialize a new <see cref="IServiceDescriptor"/>.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        public Base(Type ServiceType, Func<IServiceProvider, Type, object> Factory)
        {
            this.ServiceType = ServiceType;
            this.Factory = Factory;
        }

        /// <inheritdoc/>
        public virtual ServiceLifetime Lifetime => ServiceLifetime.Singleton;

        /// <inheritdoc/>
        public Type ServiceType { get; }

        /// <summary>
        /// Implementation Type.
        /// </summary>
        public Type ImplementationType { get; }

        /// <summary>
        /// Implementation Instance.
        /// </summary>
        public object ImplementationInstance { get; }

        /// <summary>
        /// Factory delegate.
        /// </summary>
        public Func<IServiceProvider, Type, object> Factory { get; }

        /// <inheritdoc/>
        public object Create(IServiceProvider Services, Type RequestedType)
        {
            if (Factory != null)
                return Factory(Services, RequestedType);

            if (ImplementationInstance != null)
                return ImplementationInstance;

            return (Services
                .GetService(typeof(IServiceInjector)) as IServiceInjector)
                .Create(ImplementationType);
        }
    }
}
