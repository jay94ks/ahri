using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Core
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromQueriesAttribute : Attribute
    {
        /// <summary>
        /// Name of the query argument.
        /// </summary>
        public string Name { get; set; }
    }
}
