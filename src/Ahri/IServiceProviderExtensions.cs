using System;

namespace Ahri
{
    public static class IServiceProviderExtensions
    {
        /// <summary>
        /// Get the service instance by its service type and throws exception if it couldn't be resolved.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="ServiceType"></param>
        /// <returns></returns>
        public static object GetRequiredService(this IServiceProvider This, Type ServiceType) 
            => This.GetService(ServiceType) ?? throw new NotSupportedException($"No {ServiceType.FullName} service is required but, couldn't be resolved.");

        /// <summary>
        /// Get the service instance and cast it to <typeparamref name="TService"/> quietly.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="This"></param>
        /// <returns></returns>
        public static TService GetService<TService>(this IServiceProvider This)
        {
            if (This.GetService(typeof(TService)) is TService Instance)
                return Instance;

            return default;
        }

        /// <summary>
        /// Get the service instance by its service type and throws exception if it couldn't be resolved.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static TService GetRequiredService<TService>(this IServiceProvider This)
        {
            if (This.GetService(typeof(TService)) is TService Instance)
                return Instance;

            throw new NotSupportedException($"No {typeof(TService).FullName} service is required but, couldn't be resolved.");
        }

        /// <summary>
        /// Get the service instance by its service type.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static bool TryGetRequiredService(this IServiceProvider This, Type ServiceType, out object Instance) => (Instance = This.GetService(ServiceType)) != null;

        /// <summary>
        /// Get the service instance by its service type and throws exception if it couldn't be resolved.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static bool TryGetRequiredService<TService>(this IServiceProvider This, out TService Instance) => (Instance = This.GetService<TService>()) != null;
    }
}
