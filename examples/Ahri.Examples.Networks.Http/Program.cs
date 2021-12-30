using Ahri.Hosting;
using Ahri.Hosting.Builders;
using Ahri.Http.Hosting;
using Ahri.Http.Orb;
using System.Net;
using System.Threading.Tasks;

namespace Ahri.Examples.Networks.Http
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new HostBuilder()
                .ConfigureHttpHost(Http =>
                {
                    Http.UseOrb() // --> Use `Orb` as Http Server.
                        .ConfigureHttpServer(Server =>
                        {
                            Server.Endpoint = new IPEndPoint(IPAddress.Any, 5000);
                        })

                        .ConfigureServices(Registry =>
                        {
                            // TODO: Register services here.
                        })

                        .Configure(App =>
                        {
                            // TODO: Adds Middlewares here.
                            App.Use((Context, Next) =>
                            {

                                return Next();
                            });

                            App.Configure(Services =>
                            {
                                // TODO: Do migration if needed. 
                            });
                        });

                })
                .Build()
                .RunAsync();
        }
    }
}
