using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace Ahri.Networks.Http
{
    public class HttpRequest
    {
        /// <summary>
        /// Hides the constructor of the <see cref="HttpRequest"/> instance.
        /// </summary>
        internal HttpRequest() { }

        /// <summary>
        /// A location where stores temporary datas that is used to handle the request.
        /// </summary>
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        /// <summary>
        /// Service Provider that is created for the current request.
        /// </summary>
        public IServiceProvider Services { get; internal set; }

        /// <summary>
        /// The local endpoint that accepted this request.
        /// </summary>
        public EndPoint LocalEndpoint { get; internal set; }

        /// <summary>
        /// The remote endpoint that sent this request.
        /// </summary>
        public EndPoint RemoteEndpoint { get; internal set; }

        /// <summary>
        /// Triggered when the request is aborted because the connection lost before completed.
        /// </summary>
        public CancellationToken Aborted { get; internal set; }

        /// <summary>
        /// Request Method.
        /// </summary>
        public string Method { get; internal set; }

        /// <summary>
        /// Request Protocol.
        /// </summary>
        public string Protocol { get; internal set; }

        /// <summary>
        /// Path String.
        /// </summary>
        public string PathString { get; set; }

        /// <summary>
        /// Query String
        /// </summary>
        public string QueryString { get; set; }

        /// <summary>
        /// Request Headers.
        /// </summary>
        public List<HttpHeader> Headers { get; } = new List<HttpHeader>();

        /// <summary>
        /// Content Stream.
        /// </summary>
        public Stream Content { get; internal set; }
    }
}
