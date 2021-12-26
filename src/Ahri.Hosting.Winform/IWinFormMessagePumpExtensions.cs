using Ahri.Hosting.Winform.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ahri.Hosting.Winform
{
    public static class IWinFormMessagePumpExtensions
    {
        /// <summary>
        /// Set the <see cref="IHostEnvironment"/> to WinFormHostEnvironment and,
        /// Adds the <see cref="IWinFormMessagePump"/> that invokes WinForm behaviours.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        private static IServiceCollection SetWinFormEnvironment(this IServiceCollection This)
        {
            if (This.FirstOrDefault(X => X.ServiceType == typeof(IWinFormMessagePump)) is null)
            {
                This.AddHostedService<WinFormHostService>();
                This.AddSingleton<IWinFormMessagePump, WinFormMessagePump>();
            }

            return This;
        }

        /// <summary>
        /// Invokes the factory delegate using <see cref="IWinFormMessagePump"/>.
        /// </summary>
        /// <param name="Factory"></param>
        /// <param name="Services"></param>
        /// <returns></returns>
        private static object Invoke(Func<IServiceProvider, Form> Factory, IServiceProvider Services)
        {
            var Pump = Services.GetRequiredService<IWinFormMessagePump>();
            var Form = null as Form;

            Pump.InvokeAsync(() => Form = Factory(Services))
                .GetAwaiter().GetResult();

            return Form;
        }

        /// <summary>
        /// Instantiate the <typeparamref name="TForm"/> using the dependency injection feature.
        /// </summary>
        /// <typeparam name="TForm"></typeparam>
        /// <param name="Services"></param>
        /// <returns></returns>
        private static Form Instantiate<TForm>(IServiceProvider Services) where TForm : Form
            => Services.GetRequiredService<IServiceInjector>().Create(typeof(TForm)) as Form;

        /// <summary>
        /// Adds a singleton form to service collection that created by the factory delegate.
        /// </summary>
        /// <typeparam name="TForm"></typeparam>
        /// <param name="This"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonForm<TForm>(this IServiceCollection This, Func<IServiceProvider, Form> Factory) where TForm : Form
            => This.SetWinFormEnvironment().AddSingleton<TForm>(Services => Invoke(Factory, Services));

        /// <summary>
        /// Adds a singleton form to service collection that created by the dependency injection feature.
        /// </summary>
        /// <typeparam name="TForm"></typeparam>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IServiceCollection AddSingletonForm<TForm>(this IServiceCollection This) where TForm : Form
            => This.AddSingletonForm<TForm>(Instantiate<TForm>);

        /// <summary>
        /// Adds a scoped form to service collection that created by the factory delegate.
        /// </summary>
        /// <typeparam name="TForm"></typeparam>
        /// <param name="This"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddScopedForm<TForm>(this IServiceCollection This, Func<IServiceProvider, Form> Factory) where TForm : Form
            => This.SetWinFormEnvironment().AddScoped<TForm>(Services => Invoke(Factory, Services));

        /// <summary>
        /// Adds a scoped form to service collection that created by the dependency injection feature.
        /// </summary>
        /// <typeparam name="TForm"></typeparam>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IServiceCollection AddScopedForm<TForm>(this IServiceCollection This) where TForm : Form
            => This.AddScopedForm<TForm>(Instantiate<TForm>);

        /// <summary>
        /// Adds a transient form to service collection that created by the factory delegate.
        /// </summary>
        /// <typeparam name="TForm"></typeparam>
        /// <param name="This"></param>
        /// <param name="Factory"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransientForm<TForm>(this IServiceCollection This, Func<IServiceProvider, Form> Factory) where TForm : Form
            => This.SetWinFormEnvironment().AddTransient<TForm>(Services => Invoke(Factory, Services));

        /// <summary>
        /// Adds a transient form to service collection that created by the dependency injection feature.
        /// </summary>
        /// <typeparam name="TForm"></typeparam>
        /// <param name="This"></param>
        /// <returns></returns>
        public static IServiceCollection AddTransientForm<TForm>(this IServiceCollection This) where TForm : Form
            => This.AddTransientForm<TForm>(Instantiate<TForm>);
    }
}
