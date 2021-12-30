using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Hosting.Internals
{
    internal class HttpApplicationAccessor
    {
        /// <summary>
        /// <see cref="IHttpApplication"/> Instance.
        /// </summary>
        public IHttpApplication Instance { get; set; }
    }
}
