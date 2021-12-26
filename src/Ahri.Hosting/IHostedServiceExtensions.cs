using Ahri.Hosting.Internals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ahri.Hosting
{
    /// <summary>
    /// Adds extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class IHostedServiceExtensions
    {
        private static readonly object KEY_TARGET_TYPES = new object();

        /// <summary>
        /// Get the list that manages the target hosted service types.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        private static List<Type> GetTargetTypes(this IServiceCollection This)
        {
            This.Properties.TryGetValue(KEY_TARGET_TYPES, out var Temp);
            if (Temp is null)
            {
                This.Properties[KEY_TARGET_TYPES] = Temp = new List<Type>();
                This.AddSingleton<IHostedService>(Services => new HostedService(Services, Temp as List<Type>));
            }

            return Temp as List<Type>;
        }

        /// <summary>
        /// Adds an <see cref="IHostedService"/> to be hosted on the <see cref="IHost"/> instance.
        /// </summary>
        /// <typeparam name="THostedService"></typeparam>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IServiceCollection AddHostedService<THostedService>(this IServiceCollection This) where THostedService : IHostedService
        {
            if (This.FirstOrDefault(X => X.ServiceType == typeof(THostedService)) is null)
            {
                This.GetTargetTypes().Add(typeof(THostedService));
                return This.AddTransient<THostedService>();
            }

            return This;
        }

        /// <summary>
        /// Adds an <see cref="IHostedService"/> which built by the <paramref name="Factory"/> to be hosted on the <see cref="IHost"/> instance.
        /// </summary>
        /// <typeparam name="THostedService"></typeparam>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IServiceCollection AddHostedService<THostedService>(this IServiceCollection This, Func<IServiceProvider, object> Factory) where THostedService : IHostedService
        {
            var Original = This.FirstOrDefault(X => X.ServiceType == typeof(THostedService));
            if (Original is null)
                This.GetTargetTypes().Add(typeof(THostedService));

            else
                This.Remove(Original);

            return This.AddTransient<THostedService>(Factory);
        }
    }
}
