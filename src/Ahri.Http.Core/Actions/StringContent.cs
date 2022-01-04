using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Actions
{
    public class StringContent : IHttpAction
    {
        private int m_Status;
        private string m_Content;
        private string m_MimeType;
        private Encoding m_Encoding;

        /// <summary>
        /// Initialize a new <see cref="StringContent"/> instance.
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="MimeType"></param>
        /// <param name="Encoding">(Default: <see cref="Encoding.UTF8"/>)</param>
        public StringContent(string Content, string MimeType = "text/plain", Encoding Encoding = null)
            : this(200, Content, MimeType, Encoding) { }

        /// <summary>
        /// Initialize a new <see cref="StringContent"/> instance.
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="MimeType"></param>
        /// <param name="Encoding">(Default: <see cref="Encoding.UTF8"/>)</param>
        public StringContent(int Status, string Content, string MimeType = "text/plain", Encoding Encoding = null)
        {
            m_Status = Status; m_Content = Content ?? "";
            m_MimeType = HttpMimeTypes.SetCharset(MimeType ?? "text/plain",
                m_Encoding = Encoding ?? Encoding.UTF8);
        }

        /// <inheritdoc/>
        public Task InvokeAsync(IHttpContext Context)
        {
            var Response = Context.Response;
            var ContentBytes = m_Encoding.GetBytes(m_Content);

            /* Replace `Content-Type` header to indicate the mime-type. */
            Response.Headers.Set("Content-Type", m_MimeType);

            /* Replace the content stream. */
            Response.Status = m_Status;
            Response.Content = new MemoryStream(ContentBytes, false);

            return Task.CompletedTask;
        }
    }
}
