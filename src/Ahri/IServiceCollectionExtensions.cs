using Ahri.Core;
using Ahri.Core.Descriptors;
using System;

namespace Ahri
{

    /// <summary>
    /// Provides <see cref="Singleton"/>, <see cref="Scoped"/> and <see cref="Transient"/> registration shortcuts.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Build the <see cref="ServiceProvider"/> instance from the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static ServiceProvider BuildServiceProvider(this IServiceCollection This) => new ServiceProvider(This);

        /// <summary>
        /// Add a singleton service with its implementation instance.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton(this IServiceCollection This, Type ServiceType, object ImplementationInstance)
            => This.Add(new Singleton(ServiceType, ImplementationInstance));

        /// <summary>
        /// Add a singleton service with its implementation.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="ImplementationType"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton(this IServiceCollection This, Type ServiceType, Type ImplementationType = null) => This.Add(new Singleton(ServiceType, ImplementationType));

        /// <summary>
        /// Add a singleton service with its implementation factory.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton(this IServiceCollection This, Type ServiceType, Func<IServiceProvider, object> Factory) => This.Add(new Singleton(ServiceType, Factory));

        /// <summary>
        /// Add a singleton service with its implementation factory.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton(this IServiceCollection This, Type ServiceType, Func<IServiceProvider, Type, object> Factory) => This.Add(new Singleton(ServiceType, Factory));

        /// <summary>
        /// Add a singleton service with its implementation.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection This, TService ImplementationInstance) => This.Add(new Singleton(typeof(TService), ImplementationInstance));

        /// <summary>
        /// Add a singleton service with its implementation.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection This) => This.Add(new Singleton(typeof(TService)));

        /// <summary>
        /// Add a singleton service with its implementation.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton<TService, TImplementation>(this IServiceCollection This) where TImplementation : TService => This.Add(new Singleton(typeof(TService), typeof(TImplementation)));

        /// <summary>
        /// Add a singleton service with its implementation factory.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingleton<TService>(this IServiceCollection This, Func<IServiceProvider, object> Factory) => This.Add(new Singleton(typeof(TService), Factory));

        /// <summary>
        /// Add a scoped service with its implementation.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="ImplementationType"></param>
        /// <returns></returns>
        public static IServiceCollection AddScoped(this IServiceCollection This, Type ServiceType, Type ImplementationType = null) => This.Add(new Scoped(ServiceType, ImplementationType));

        /// <summary>
        /// Add a scoped service with its implementation factory.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddScoped(this IServiceCollection This, Type ServiceType, Func<IServiceProvider, object> Factory) => This.Add(new Scoped(ServiceType, Factory));

        /// <summary>
        /// Add a scoped service with its implementation factory.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddScoped(this IServiceCollection This, Type ServiceType, Func<IServiceProvider, Type, object> Factory) => This.Add(new Scoped(ServiceType, Factory));

        /// <summary>
        /// Add a scoped service with its implementation.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IServiceCollection AddScoped<TService>(this IServiceCollection This) => This.Add(new Scoped(typeof(TService)));

        /// <summary>
        /// Add a scoped service with its implementation.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IServiceCollection AddScoped<TService, TImplementation>(this IServiceCollection This) where TImplementation : TService => This.Add(new Scoped(typeof(TService), typeof(TImplementation)));

        /// <summary>
        /// Add a scoped service with its implementation factory.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddScoped<TService>(this IServiceCollection This, Func<IServiceProvider, object> Factory) => This.Add(new Scoped(typeof(TService), Factory));

        /// <summary>
        /// Add a transient service with its implementation.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="ImplementationType"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransient(this IServiceCollection This, Type ServiceType, Type ImplementationType = null) => This.Add(new Transient(ServiceType, ImplementationType));

        /// <summary>
        /// Add a transient service with its implementation factory.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransient(this IServiceCollection This, Type ServiceType, Func<IServiceProvider, object> Factory) => This.Add(new Transient(ServiceType, Factory));

        /// <summary>
        /// Add a transient service with its implementation factory.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransient(this IServiceCollection This, Type ServiceType, Func<IServiceProvider, Type, object> Factory) => This.Add(new Transient(ServiceType, Factory));

        /// <summary>
        /// Add a transient service with its implementation.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransient<TService>(this IServiceCollection This) => This.Add(new Transient(typeof(TService)));

        /// <summary>
        /// Add a transient service with its implementation.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransient<TService, TImplementation>(this IServiceCollection This) where TImplementation : TService => This.Add(new Transient(typeof(TService), typeof(TImplementation)));

        /// <summary>
        /// Add a transient service with its implementation factory.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransient<TService>(this IServiceCollection This, Func<IServiceProvider, object> Factory) => This.Add(new Transient(typeof(TService), Factory));

    }
}
