using System.Net;

namespace Ahri.Orp.Internals
{
    public class OrpServerOptions
    {
        /// <summary>
        /// Local Endpoint.
        /// </summary>
        public IPEndPoint LocalEndPoint { get; set; } = new IPEndPoint(IPAddress.Any, 9000);
    }
}
