using Ahri.Hosting;
using Ahri.Hosting.Builders;
using Ahri.Logging;
using Ahri.Orp;
using Ahri.Orp.Hosting;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Ahri.Examples.Orp.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new HostBuilder()
                .ConfigureLogging()
                .ConfigureOrpClientHost(Orp =>
                {
                    Orp
                        .UseOptions(Options =>
                        {
                            Options.RemoteEndPoint = new IPEndPoint(
                                IPAddress.Loopback, 9000);
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


                                return Next();
                            });

                            App.UseGreetings(async Conn =>
                            {
                                var Result = await Conn.EmitAsync(new Message
                                {
                                    Text = "Hello World!"
                                });

                                if (Result.State == OrpMessageState.Success)
                                {
                                    if (Result.Reply is Message Message)
                                        Console.WriteLine(Message.Text);
                                }
                                
                                Result = await Conn.EmitAsync(new Message
                                {
                                    Text = "Hello World 2!"
                                });

                                if (Result.State == OrpMessageState.Success)
                                {
                                    if (Result.Reply is Message Message)
                                        Console.WriteLine(Message.Text);
                                }

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
