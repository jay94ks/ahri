using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Orb.Internals
{
    internal class HttpOpaqueFeature : IHttpOpaqueFeature
    {
        private HttpConnection m_Connection;

        /// <summary>
        /// Initialize a new <see cref="HttpOpaqueFeature"/> instance
        /// </summary>
        /// <param name="Connection"></param>
        public HttpOpaqueFeature(HttpConnection Connection)
        {
            m_Connection = Connection;
        }

        /// <inheritdoc/>
        public Stream GetOpaqueStream()
        {
            return m_Connection.GetOpaqueStream();
        }
    }
}
