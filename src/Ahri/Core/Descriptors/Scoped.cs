using System;

namespace Ahri.Core.Descriptors
{
    public class Scoped : Base
    {
        /// <summary>
        /// Initialize a new <see cref="IServiceDescriptor"/> that describes <see cref="Scoped"/> lifetime.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="ImplementationType"></param>
        public Scoped(Type ServiceType, Type ImplementationType = null) : base(ServiceType, ImplementationType)
        {
        }

        /// <summary>
        /// Initialize a new <see cref="IServiceDescriptor"/> that describes <see cref="Scoped"/> lifetime.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        public Scoped(Type ServiceType, Func<IServiceProvider, object> Factory) : base(ServiceType, Factory)
        {
        }

        /// <summary>
        /// Initialize a new <see cref="IServiceDescriptor"/> that describes <see cref="Scoped"/> lifetime.
        /// </summary>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        public Scoped(Type ServiceType, Func<IServiceProvider, Type, object> Factory) : base(ServiceType, Factory)
        {
        }

        /// <inheritdoc/>
        public override ServiceLifetime Lifetime => ServiceLifetime.Scoped;
    }
}
