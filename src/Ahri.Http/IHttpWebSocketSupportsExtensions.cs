using Ahri.Http.Internals;

namespace Ahri.Http
{
    public static class IHttpWebSocketSupportsExtensions
    {
        /// <summary>
        /// Get WebSocket feature instance.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IHttpWebSocketSupports GetWebSocketSupports(this IHttpContext This)
        {
            This.Request.Properties.TryGetValue(typeof(HttpWebSocketSupports), out var Temp);

            if (!(Temp is HttpWebSocketSupports Feature))
            {
                This.Request.Properties[typeof(HttpWebSocketSupports)] = Feature 
                    = new HttpWebSocketSupports(This);
            }

            return Feature;
        }

    }
}