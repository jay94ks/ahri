using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Orp.Internals
{
    internal class OrpMappings : IOrpMappings
    {
        private Dictionary<uint, Type> m_Hash2Type = new();
        private Dictionary<Type, uint> m_Type2Hash = new();

        /// <summary>
        /// Initialize a new <see cref="OrpMappings"/> instance.
        /// </summary>
        /// <param name="Types"></param>
        public OrpMappings(IEnumerable<Type> Types)
        {
            foreach(var Each in Types)
            {
                var Attr = Each.GetCustomAttribute<OrpMappedAttribute>();
                var Hash = OrpHelpers.Hash(Attr != null ? Attr.Name : Each.FullName);

                m_Hash2Type[Hash] = Each;
                m_Type2Hash[Each] = Hash;
            }
        }

        /// <inheritdoc/>
        public Type FromHash(uint Hash)
        {
            m_Hash2Type.TryGetValue(Hash, out var Type);
            return Type;
        }

        /// <inheritdoc/>
        public uint? FromType(Type Type)
        {
            if (m_Type2Hash.TryGetValue(Type, out var Hash))
                return Hash;

            return null;
        }

        /// <inheritdoc/>
        public bool IsMapped(uint Hash) => m_Hash2Type.ContainsKey(Hash);

        /// <inheritdoc/>
        public bool IsMapped(Type Type) => m_Type2Hash.ContainsKey(Type);
    }
}
