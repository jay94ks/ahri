using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Ahri.Core
{
    /// <summary>
    /// Registry of the <see cref="IServiceDescriptor"/>s.
    /// </summary>
    public class ServiceCollection : IServiceCollection
    {
        private readonly List<IServiceDescriptor> m_Services = new();

        /// <inheritdoc/>
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        /// <inheritdoc/>
        public ICollection<Func<ParameterInfo, IServiceProvider, object>> Resolvers { get; } = new List<Func<ParameterInfo, IServiceProvider, object>>();

        /// <inheritdoc/>
        public int Count => m_Services.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public IServiceCollection Clear()
        {
            m_Services.Clear();
            return this;
        }

        /// <inheritdoc/>
        public IServiceCollection Add(IServiceDescriptor item)
        {
            m_Services.Add(item);
            return this;
        }

        /// <inheritdoc/>
        public bool Contains(IServiceDescriptor item) => m_Services.Contains(item);

        /// <inheritdoc/>
        public bool Remove(IServiceDescriptor item) => m_Services.Remove(item);

        /// <inheritdoc/>
        public void CopyTo(IServiceDescriptor[] array, int arrayIndex) => m_Services.CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        public IEnumerator<IServiceDescriptor> GetEnumerator() => m_Services.GetEnumerator();

        /// <inheritdoc/>
        void ICollection<IServiceDescriptor>.Add(IServiceDescriptor item) => m_Services.Add(item);

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => m_Services.GetEnumerator();

        /// <inheritdoc/>
        void ICollection<IServiceDescriptor>.Clear() => m_Services.Clear();
    }
}
