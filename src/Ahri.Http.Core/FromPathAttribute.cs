using System;

namespace Ahri.Http.Core
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromPathAttribute : Attribute
    {
        /// <summary>
        /// Name of the path parameter.
        /// </summary>
        public string Name { get; set; }
    }
}
