using Ahri.Hosting;
using Ahri.Hosting.Builders;
using Ahri.Logging;
using Ahri.Orp;
using Ahri.Orp.Hosting;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Ahri.Examples.Orp.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new HostBuilder()
                .ConfigureLogging()
                .ConfigureOrpHost(Orp =>
                {
                    Orp
                        .UseOptions(Options =>
                        {
                            Options.LocalEndPoint = new IPEndPoint(IPAddress.Any, 9000);
                        })

                        .ConfigureServices(Registry =>
                        {
                            // TODO: Register services here.
                        })

                        .ConfigureMappings(Mappings =>
                        {
                            Mappings.Add(typeof(Message));
                        })

                        .Configure(App =>
                        {
                            // TODO: Adds Middlewares here.
                            App.Use((Context, Next) =>
                            {
                                if (Context.Message is Message Message)
                                {
                                    Console.WriteLine(Message.Text);
                                    return Context.ReplyTo(Message);
                                }

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

        [OrpMapped(Name = "message")]
        public class Message
        {
            [JsonProperty("text")]
            public string Text { get; set; }
        }
    }
}
