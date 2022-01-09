using System;

namespace Ahri.Orp
{
    public interface IOrpMappings
    {
        /// <summary>
        /// Test whether the hash value is mapped to type or not.
        /// </summary>
        /// <param name="Hash"></param>
        /// <returns></returns>
        bool IsMapped(uint Hash);

        /// <summary>
        /// Test whether the type is mapped to hash value or not.
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        bool IsMapped(Type Type);

        /// <summary>
        /// Get Type from the hash value.
        /// </summary>
        /// <param name="Hash"></param>
        /// <returns></returns>
        Type FromHash(uint Hash);

        /// <summary>
        /// Get Hash for the type.
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        uint? FromType(Type Type);
    }
}
