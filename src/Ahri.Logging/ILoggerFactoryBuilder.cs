using System;
using System.Collections.Generic;

namespace Ahri.Logging
{
    /// <summary>
    /// Logger Factory builder
    /// </summary>
    public interface ILoggerFactoryBuilder
    {
        /// <summary>
        /// Remove all configured delegates from the builder including properties.
        /// </summary>
        /// <returns></returns>
        ILoggerFactoryBuilder Clear();

        /// <summary>
        /// Get all implementation types.
        /// </summary>
        IEnumerable<Type> ImplementationTypes { get; }

        /// <summary>
        /// Adds the <typeparamref name="TImpl"/> as logger factory.
        /// </summary>
        /// <typeparam name="TImpl"></typeparam>
        /// <returns></returns>
        ILoggerFactoryBuilder Use<TImpl>() where TImpl : ILoggerFactory;

        /// <summary>
        /// Adds a delegate that creates an instance of the factory.
        /// </summary>
        /// <param name="Factory"></param>
        /// <returns></returns>
        ILoggerFactoryBuilder Use<TImpl>(Func<IServiceProvider, TImpl> Factory) where TImpl : ILoggerFactory;

        /// <summary>
        /// Build a logging service container and returns its front-end, <see cref="ILoggerFactory"/> instance.
        /// </summary>
        /// <returns></returns>
        ILoggerFactory Build();
    }
}
