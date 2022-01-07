using System;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace Ahri.Http
{
    public interface IHttpContext
    {
        /// <summary>
        /// Request.
        /// </summary>
        IHttpRequest Request { get; }

        /// <summary>
        /// Response.
        /// </summary>
        IHttpResponse Response { get; }

        /// <summary>
        /// Get Protocol Feature.
        /// When no feature supported, this will return null.
        /// </summary>
        /// <typeparam name="TFeature"></typeparam>
        /// <returns></returns>
        TFeature GetFeature<TFeature>();
    }
}