using Ahri.Networks.Http;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

        class EchoServer : HttpServer<EchoSession>
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
        }

        class EchoSession : HttpSession
        {
            protected override async Task OnReceiveAsync(HttpRequest Request, HttpResponse Response)
            {
                var Buffer = new byte[2048];
                while (true)
                {
                    int Length = await Request.Content.ReadAsync(Buffer);
                    if (Length <= 0)
                        break;

                    if (Response.Headers.FindIndex(X => X.Key == "Content-Type") < 0)
                        Response.Headers.Add(new HttpHeader("Content-Type", "application/json"));

                    await Response.Content.WriteAsync(new ArraySegment<byte>(Buffer, 0, Length));
                }

            }
        }
    }
}
