using Ahri.Networks.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Networks.Hosting
{
    public static class HostingExtensions
    {
        public static IServiceCollection AddTcpServer<TSession>(this IServiceCollection This, Action<TcpServerOptions> Options) where TSession : TcpSession
        {
            
        }
    }
}
