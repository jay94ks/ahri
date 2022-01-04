using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http
{
    public interface IHttpContextAccessor
    {
        /// <summary>
        /// Context instance.
        /// </summary>
        IHttpContext Instance { get; }
    }
}
