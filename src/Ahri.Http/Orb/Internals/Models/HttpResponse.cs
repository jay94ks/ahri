using System.Collections.Generic;
using System.IO;

namespace Ahri.Http.Orb.Internals.Models
{
    public class HttpResponse : IHttpResponse
    {
        /// <summary>
        /// Status.
        /// </summary>
        public int Status { get; set; } = 404;

        /// <summary>
        /// Status Phrase.
        /// </summary>
        public string Phrase { get; set; }

        /// <summary>
        /// Response Headers to be sent.
        /// </summary>
        public List<HttpHeader> Headers { get; } = new List<HttpHeader>();

        /// <summary>
        /// Content Stream to transfer the response body.
        /// When the user writes something on the stream,
        /// The request status and phrase, headers will be sent.
        /// </summary>
        public Stream Content { get; set; }
    }
}
