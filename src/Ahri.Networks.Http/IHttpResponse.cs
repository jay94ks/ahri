using System.Collections.Generic;
using System.IO;

namespace Ahri.Http
{
    public interface IHttpResponse
    {
        /// <summary>
        /// Status.
        /// </summary>
        int Status { get; set; }

        /// <summary>
        /// Status Phrase.
        /// </summary>
        string Phrase { get; set; }

        /// <summary>
        /// Response Headers to be sent.
        /// </summary>
        List<HttpHeader> Headers { get; }

        /// <summary>
        /// Content Stream to transfer the response body.
        /// When the user writes something on the stream,
        /// The request status and phrase, headers will be sent.
        /// </summary>
        Stream Content { get; set; }
    }
}