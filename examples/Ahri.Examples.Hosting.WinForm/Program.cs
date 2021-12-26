using Ahri.Hosting;
using Ahri.Hosting.Builders;
using Ahri.Hosting.Winform;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ahri.Examples.Hosting.WinForm
{
    class Program 
    {
        static async Task Main(string[] args)
        {
            await new HostBuilder()
                .ConfigureServices(Registry => {
                    // TODO: Register services here.
                    Registry.AddSingletonForm<MyForm>();
                })
                .Configure(Services =>
                {
                    var Pump = Services.GetRequiredService<IWinFormMessagePump>();
                    _ = Pump.InvokeAsync(() =>
                    {
                        var Form = Services.GetRequiredService<MyForm>();

                        /* TODO: Write logic to mediate forms. */
                        Form.FormClosed += (_, _) =>
                        {
                            Services
                                .GetRequiredService<IHostLifetime>()
                                .Terminate();
                        };

                        Form.Show();
                    });
                })
                .Build()
                .RunAsync();
        }

        public class MyForm : Form
        {
            // ........................
        }
    }
}
