using Ahri.Hosting;
using Ahri.Orp.Hosting.Builders;
using System;

namespace Ahri.Orp.Hosting
{
    public static class IHostBuilderExtensions
    {
        /// <summary>
        /// Configure the Orp application for <see cref="IHostBuilder"/>.
        /// </summary>
        /// <param name="Configure"></param>
        /// <returns></returns>
        public static IHostBuilder ConfigureOrpHost(this IHostBuilder This, Action<IOrpHostBuilder> Configure)
        {
            This.Properties.TryGetValue(typeof(IOrpHostBuilder), out var Orp);

            if (Orp is null)
                This.Properties[typeof(IOrpHostBuilder)] = Orp = new OrpHostBuilder(This);

            Configure?.Invoke(Orp as IOrpHostBuilder);
            return This;
        }

        /// <summary>
        /// Configure the Orp client application for <see cref="IHostBuilder"/>.
        /// </summary>
        /// <param name="Configure"></param>
        /// <returns></returns>
        public static IHostBuilder ConfigureOrpClientHost(this IHostBuilder This, Action<IOrpClientHostBuilder> Configure)
        {
            This.Properties.TryGetValue(typeof(IOrpClientHostBuilder), out var Orp);

            if (Orp is null)
                This.Properties[typeof(IOrpClientHostBuilder)] = Orp = new OrpClientHostBuilder(This);

            Configure?.Invoke(Orp as IOrpClientHostBuilder);
            return This;
        }
    }
}
