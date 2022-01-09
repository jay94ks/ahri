using Ahri.Orp.Internals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Orp.Hosting.Builders
{
    public class OrpMappingsBuilder : IOrpMappingsBuilder
    {
        private List<Type> m_Types = new();

        /// <inheritdoc/>
        public Type this[int index] { get => m_Types[index]; set => m_Types[index] = value; }

        /// <inheritdoc/>
        public int Count => m_Types.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public IOrpMappingsBuilder Add(Type Type)
        {
            m_Types.Add(Type);
            return this;
        }

        /// <inheritdoc/>
        public IOrpMappings Build()
        {
            return new OrpMappings(m_Types);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            m_Types.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(Type item)
        {
            return m_Types.Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(Type[] array, int arrayIndex)
        {
            m_Types.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public IEnumerator<Type> GetEnumerator()
        {
            return m_Types.GetEnumerator();
        }

        /// <inheritdoc/>
        public int IndexOf(Type item)
        {
            return m_Types.IndexOf(item);
        }

        /// <inheritdoc/>
        public void Insert(int index, Type item)
        {
            m_Types.Insert(index, item);
        }

        /// <inheritdoc/>
        public bool Remove(Type item)
        {
            return m_Types.Remove(item);
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            m_Types.RemoveAt(index);
        }

        /// <inheritdoc/>
        void ICollection<Type>.Add(Type item) => Add(item);

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
