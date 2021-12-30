using Ahri.Http;
using Ahri.Http.Orb.Internals;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Ahri.Examples.Messangers.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var s = new EchoServer(new IPEndPoint(IPAddress.Any, 8000));

            await s.StartAsync();
            await Task.Delay(1000 * 60 * 5);
            await s.StopAsync();
        }

        class EchoServer : HttpServer
        {
            public EchoServer(IPEndPoint EndPoint) : base(EndPoint)
            {
            }

            protected override Task OnStartAsync()
            {
                return Task.CompletedTask;
            }

            protected override Task OnStopAsync()
            {
                return Task.CompletedTask;
            }

            protected override async Task OnRequestAsync(IHttpContext Context)
            {
                var Buffer = new byte[2048];
                while (true)
                {
                    int Length = await Context.Request.Content.ReadAsync(Buffer);
                    if (Length <= 0)
                        break;

                    if (Context.Response.Headers.FindIndex(X => X.Key == "Content-Type") < 0)
                        Context.Response.Headers.Add(new HttpHeader("Content-Type", "application/json"));

                    await Context.Response.Content.WriteAsync(new ArraySegment<byte>(Buffer, 0, Length));
                }
            }
        }
    }
}
