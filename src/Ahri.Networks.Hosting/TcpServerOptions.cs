using System.Net;

namespace Ahri.Networks.Hosting
{
    public class TcpServerOptions
    {
        /// <summary>
        /// Address to listen.
        /// </summary>
        public IPAddress Address { get; set; }

        /// <summary>
        /// Port to listen.
        /// </summary>
        public int Port { get; set; }
    }
}
