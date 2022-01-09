using System;
using System.Collections.Generic;

namespace Ahri.Orp.Hosting
{
    public interface IOrpMappingsBuilder : IList<Type>
    {
        /// <summary>
        /// Add a type to mapping table.
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        new IOrpMappingsBuilder Add(Type Type);

        /// <summary>
        /// Build the <see cref="IOrpMappings"/> instance.
        /// </summary>
        /// <returns></returns>
        IOrpMappings Build();
    }
}
