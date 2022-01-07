using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http
{
    public static class IHttpContentDeserializerBuilderExtensions
    {
        /// <summary>
        /// Use <see cref="JsonConvert"/> to handle "application/json" content.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IHttpContentDeserializerBuilder UseJson(this IHttpContentDeserializerBuilder This) 
            => This.Use(typeof(JsonDeserializer));

        /// <summary>
        /// <see cref="JsonConvert"/> wrapper that handles "application/json" content.
        /// </summary>
        private class JsonDeserializer : IHttpContentDeserializer
        {
            /// <inheritdoc/>
            public bool CanHandle(IHttpRequest Request)
            {
                var ContentType = Request.Headers.Get("Content-Type");
                if (ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
                    return true;

                return false;
            }

            /// <inheritdoc/>
            public bool CanConvert(object From, Type To) => From is JObject || From is JToken;

            /// <inheritdoc/>
            public Task<object> Handle(IHttpRequest Request)
            {
                using (var Reader = new StreamReader(Request.ContentStream, Encoding.UTF8))
                    return Task.FromResult(JsonConvert.DeserializeObject(Reader.ReadToEnd()));
            }

            /// <inheritdoc/>
            public object Convert(object From, Type To)
            {
                if (From is JObject Object)
                    return Object.ToObject(To);

                if (From is JToken Token)
                    return Token.ToObject(To);

                return null;
            }
        }
    }
}
