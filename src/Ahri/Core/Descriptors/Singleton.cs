using System;

namespace Ahri.Core.Descriptors
{
    public class Singleton : Base
    {
        /// <summary>
        /// Initialize a new <see cref="IServiceDescriptor"/> that describes <see cref="Singleton"/> lifetime.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="ImplementationType"></param>
        public Singleton(Type ServiceType, Type ImplementationType = null) : base(ServiceType, ImplementationType)
        {
        }

        /// <summary>
        /// Initialize a new <see cref="IServiceDescriptor"/> that describes <see cref="Singleton"/> lifetime.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="ImplementationInstance"></param>
        public Singleton(Type ServiceType, object ImplementationInstance) : base(ServiceType, ImplementationInstance)
        {
        }

        /// <summary>
        /// Initialize a new <see cref="IServiceDescriptor"/> that describes <see cref="Singleton"/> lifetime.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        public Singleton(Type ServiceType, Func<IServiceProvider, object> Factory) : base(ServiceType, Factory)
        {
        }

        /// <summary>
        /// Initialize a new <see cref="IServiceDescriptor"/> that describes <see cref="Singleton"/> lifetime.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        public Singleton(Type ServiceType, Func<IServiceProvider, Type, object> Factory) : base(ServiceType, Factory)
        {
        }

        /// <inheritdoc/>
        public override ServiceLifetime Lifetime => ServiceLifetime.Singleton;

    }
}
