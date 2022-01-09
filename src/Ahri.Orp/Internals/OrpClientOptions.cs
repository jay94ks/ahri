using System;
using System.Net;

namespace Ahri.Orp.Internals
{
    public class OrpClientOptions
    {
        /// <summary>
        /// Local Endpoint.
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; set; } = new IPEndPoint(IPAddress.Any, 9000);

        /// <summary>
        /// Recovery Term.
        /// </summary>
        public TimeSpan RecoveryTerm { get; set; } = TimeSpan.FromSeconds(5);
    }
}
