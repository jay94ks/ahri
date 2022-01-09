using System;

namespace Ahri.Orp
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class OrpMappedAttribute : Attribute
    {
        /// <summary>
        /// Name of the type. (Optional)
        /// </summary>
        public string Name { get; set; }
    }
}
