using System;

namespace Ahri.Core
{
    public sealed class ServiceProvider : IServiceProvider, IDisposable
    {
        private ServiceScope m_Scope;

        /// <summary>
        /// initialize a new <see cref="ServiceProvider"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        internal ServiceProvider(IServiceCollection Services)
        {
            (m_Scope = new ServiceScope(Services))
                .SetOverrides<IServiceProvider>(this);
        }

        /// <inheritdoc/>
        public object GetService(Type ServiceType)
            => m_Scope.ServiceProvider.GetService(ServiceType);

        /// <inheritdoc/>
        public void Dispose() => m_Scope.Dispose();
    }
}
