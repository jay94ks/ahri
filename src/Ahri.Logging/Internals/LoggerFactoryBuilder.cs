using Ahri.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Logging.Internals
{
    public class LoggerFactoryBuilder : ILoggerFactoryBuilder
    {
        private List<Action<IServiceCollection>> m_Configure = new();
        private List<Type> m_ImplTypes = new();

        /// <inheritdoc/>
        public ILoggerFactoryBuilder Clear()
        {
            m_Configure.Clear();
            m_ImplTypes.Clear();
            return this;
        }

        /// <inheritdoc/>
        public IEnumerable<Type> ImplementationTypes => m_ImplTypes;

        /// <inheritdoc/>
        public ILoggerFactoryBuilder Use<TImpl>() where TImpl : ILoggerFactory
        {
            m_Configure.Add(Services => Services.AddSingleton<TImpl>());
            m_ImplTypes.Add(typeof(TImpl));
            return this;
        }

        /// <inheritdoc/>
        public ILoggerFactoryBuilder Use<TImpl>(Func<IServiceProvider, TImpl> Factory) where TImpl : ILoggerFactory
        {
            m_Configure.Add(Services => Services.AddSingleton<TImpl>(X => Factory(X)));
            m_ImplTypes.Add(typeof(TImpl));
            return this;
        }

        /// <summary>
        /// Build a logging service container and returns its front-end, <see cref="LoggerFactory"/> instance.
        /// </summary>
        /// <param name="Context"></param>
        /// <returns></returns>
        public LoggerFactory Build()
        {
            var Collection = new ServiceCollection();

            foreach (var Configure in m_Configure)
                Configure?.Invoke(Collection);

            return new LoggerFactory(Collection.BuildServiceProvider(), m_ImplTypes.Distinct());
        }

        /// <inheritdoc/>
        ILoggerFactory ILoggerFactoryBuilder.Build() => Build();

    }
}
