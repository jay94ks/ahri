using Ahri.Http.Internals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace Ahri.Http.Orb.Internals.Models
{
    public class HttpRequest : IHttpRequest
    {
        private object m_Content;
        private bool m_ContentInterpreted;

        /// <summary>
        /// Hides the constructor of the <see cref="HttpRequest"/> instance.
        /// </summary>
        internal HttpRequest()
        {
            Queries = new HttpRequestQueryString(this);
        }

        /// <inheritdoc/>
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        /// <inheritdoc/>
        public IServiceProvider Services { get; internal set; }

        /// <inheritdoc/>
        public EndPoint LocalEndpoint { get; internal set; }

        /// <inheritdoc/>
        public EndPoint RemoteEndpoint { get; internal set; }

        /// <inheritdoc/>
        public CancellationToken Aborted { get; internal set; }

        /// <inheritdoc/>
        public string Method { get; internal set; }

        /// <inheritdoc/>
        public string Protocol { get; internal set; }

        /// <inheritdoc/>
        public string PathString { get; set; }

        /// <inheritdoc/>
        public string QueryString { get; set; }

        /// <inheritdoc/>
        public IDictionary<string, string> Queries { get; }

        /// <inheritdoc/>
        public List<HttpHeader> Headers { get; } = new List<HttpHeader>();

        /// <inheritdoc/>
        public object Content
        {
            get
            {
                if (m_ContentInterpreted)
                    return m_Content;

                if (ContentStream != null)
                {
                    var Deserializer = Services.GetService<IHttpContentDeserializer>();
                    try
                    {
                        if (Deserializer != null && Deserializer.CanHandle(this))
                            m_Content = Deserializer.Handle(this);
                    }

                    catch { m_Content = null; }
                }

                m_ContentInterpreted = true;
                return m_Content;
            }

            set
            {
                m_Content = value;
                m_ContentInterpreted = true;
            }
        }

        /// <inheritdoc/>
        public Stream ContentStream { get; internal set; }

    }
}
