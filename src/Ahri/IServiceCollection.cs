using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ahri
{
    /// <summary>
    /// Registry of the <see cref="IServiceDescriptor"/>s.
    /// </summary>
    public interface IServiceCollection : ICollection<IServiceDescriptor>
    {
        /// <summary>
        /// A central location that is used to share datas between service registrations.
        /// </summary>
        IDictionary<object, object> Properties { get; }

        /// <summary>
        /// Removes all service descriptors from the collection.
        /// </summary>
        /// <returns></returns>
        new IServiceCollection Clear();

        /// <summary>
        /// Add a <see cref="IServiceDescriptor"/>
        /// </summary>
        /// <param name="item"></param>
        new IServiceCollection Add(IServiceDescriptor item);

        /// <summary>
        /// Resolvers that resolves an instance for the parameter.
        /// </summary>
        ICollection<Func<ParameterInfo, IServiceProvider, object>> Resolvers { get; }
    }
}
