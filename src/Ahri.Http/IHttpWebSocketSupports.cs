using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Ahri.Http
{
    public interface IHttpWebSocketSupports
    {
        /// <summary>
        /// Test whether the request is web-socket request or not.
        /// </summary>
        bool IsWebSocketRequest { get; }

        /// <summary>
        /// Gets the requested subprotocols.
        /// </summary>
        IEnumerable<string> Subprotocols { get; }

        /// <summary>
        /// Accept WebSocket request.
        /// When not supported, this will return null instead of returning exception.
        /// </summary>
        /// <returns></returns>
        Task<WebSocket> AcceptAsync(string Subprotocol = null);
    }
}