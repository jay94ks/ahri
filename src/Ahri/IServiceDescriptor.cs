using System;

namespace Ahri
{
    /// <summary>
    /// Describes the service registration.
    /// </summary>
    public interface IServiceDescriptor
    {
        /// <summary>
        /// The service scope of the service instance and how to instantiate it.
        /// </summary>
        ServiceLifetime Lifetime { get; }

        /// <summary>
        /// The Type used when looking up the service object.
        /// </summary>
        Type ServiceType { get; }

        /// <summary>
        /// Creates (or gets) an instance of the service.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="RequestedType"></param>
        /// <returns></returns>
        object Create(IServiceProvider Services, Type RequestedType);
    }
}
