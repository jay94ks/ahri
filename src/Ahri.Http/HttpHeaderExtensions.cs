using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http
{
    public static class HttpHeaderExtensions
    {
        /// <summary>
        /// Set Header.
        /// </summary>
        /// <param name="Headers"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static List<HttpHeader> Set(this List<HttpHeader> Headers, string Key, string Value) => Headers.Remove(Key).Add(Key, Value);

        /// <summary>
        /// Get header with default fallback.
        /// </summary>
        /// <param name="Headers"></param>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public static string Get(this List<HttpHeader> Headers, string Key, string Default = null)
        {
            var Header = Headers.Find(X => X.Key.Equals(Key, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(Header.Value))
                return Header.Value;

            return Default;
        }

        /// <summary>
        /// Add header.
        /// </summary>
        /// <param name="Headers"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static List<HttpHeader> Add(this List<HttpHeader> Headers, string Key, string Value)
        {
            Headers.Add(new HttpHeader(Key, Value));
            return Headers;
        }

        /// <summary>
        /// Remove header by its key.
        /// </summary>
        /// <param name="Headers"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static List<HttpHeader> Remove(this List<HttpHeader> Headers, string Key)
        {
            Headers.RemoveAll(X => X.Key.Equals(Key, StringComparison.OrdinalIgnoreCase));
            return Headers;
        }
    }
}
