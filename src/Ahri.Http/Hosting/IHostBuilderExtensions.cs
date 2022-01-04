using Ahri.Hosting;
using Ahri.Http.Hosting.Builders;
using System;

namespace Ahri.Http.Hosting
{
    public static class IHostBuilderExtensions
    {
        /// <summary>
        /// Configure the Http application for <see cref="IHostBuilder"/>.
        /// </summary>
        /// <param name="Configure"></param>
        /// <returns></returns>
        public static IHostBuilder ConfigureHttpHost(this IHostBuilder This, Action<IHttpHostBuilder> Configure)
        {
            This.Properties.TryGetValue(typeof(IHttpHostBuilder), out var _Http);

            if (_Http is null)
                This.Properties[typeof(IHttpHostBuilder)] = _Http = new HttpHostBuilder(This);

            Configure?.Invoke(_Http as IHttpHostBuilder);
            return This;
        }
    }
}
