using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace Ahri.Http
{
    public interface IHttpRequest
    {
        /// <summary>
        /// A location where stores temporary datas that is used to handle the request.
        /// </summary>
        IDictionary<object, object> Properties { get; }

        /// <summary>
        /// Service Provider that is created for the current request.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// The local endpoint that accepted this request.
        /// </summary>
        EndPoint LocalEndpoint { get; }

        /// <summary>
        /// The remote endpoint that sent this request.
        /// </summary>
        EndPoint RemoteEndpoint { get; }

        /// <summary>
        /// Triggered when the request is aborted because the connection lost before completed.
        /// </summary>
        CancellationToken Aborted { get; }

        /// <summary>
        /// Request Method.
        /// </summary>
        string Method { get; }

        /// <summary>
        /// Request Protocol.
        /// </summary>
        string Protocol { get; }

        /// <summary>
        /// Path String.
        /// </summary>
        string PathString { get; set; }

        /// <summary>
        /// Query String
        /// </summary>
        string QueryString { get; set; }

        /// <summary>
        /// Request Headers.
        /// </summary>
        List<HttpHeader> Headers { get; }

        /// <summary>
        /// Content Stream.
        /// </summary>
        Stream Content { get; }
    }
}