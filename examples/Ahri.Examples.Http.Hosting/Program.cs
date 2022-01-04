using Ahri.Hosting;
using Ahri.Hosting.Builders;
using Ahri.Http.Core;
using Ahri.Http.Core.Actions;
using Ahri.Http.Hosting;
using Ahri.Http.Orb;
using Ahri.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Ahri.Examples.Http.Hosting
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new HostBuilder()
                .ConfigureLogging()
                .ConfigureHttpHost(Host =>
                {
                    Host.UseOrb() // --> Use `Orb` as Http Server.
                        .ConfigureHttpServer(Server =>
                        {
                            Server.Endpoint = new IPEndPoint(IPAddress.Any, 5000);
                        })

                        .ConfigureServices(Registry =>
                        {
                            // TODO: Register services here.
                        })

                        .Configure(Http =>
                        {
                            // TODO: Adds Middlewares here.
                            Http.Use((Context, Next) =>
                            {
                                var Logger = Context.Request.Services.GetService<ILogger<Program>>();

                                Logger.Log(LogLevel.Info, Context.Request.PathString);
                                return Next();
                            });

                            Http.UseStaticFiles()
                                .Map("/", "./www");

                            Http.UseRouting()
                                .Map("GET", async Context =>
                                {

                                    return new StringContent("Hello World!");
                                })
                                .Path("/:name", Subroute =>
                                {
                                    Subroute.OnGet(async Context =>
                                    {
                                        var Route = Context.GetRouterState();

                                        Route.PathParameters.TryGetValue(":name", out var Name);

                                        return new StringContent($"Hello {Name}");
                                    });
                                });

                            Http.Configure(Services =>
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
