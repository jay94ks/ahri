using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ahri.Hosting
{
    public static class IHostExtensions
    {
        /// <summary>
        /// Run the <see cref="IHost"/> instance saynchronously.
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static async Task RunAsync(this IHost This, CancellationToken Token = default)
        {
            var Lifetime = This.Services.GetRequiredService<IHostLifetime>();
            using var Cts = CancellationTokenSource
                .CreateLinkedTokenSource(Lifetime.Stopping, Token);

            if (This.Services.TryGetRequiredService<IHostEnvironment>(out var Env))
            {
                try { await Env.PrepareAsync(Cts.Token); }
                catch (OperationCanceledException)
                {
                    await Env.FinishAsync();
                    return;
                }
            }

            await InternalRunAsync(This, Cts.Token);
            await Env.FinishAsync();
        }

        /// <summary>
        /// Run the <see cref="IHost"/> instance.
        /// </summary>
        /// <param name="This"></param>
        public static void Run(this IHost This) => This.RunAsync().GetAwaiter().GetResult();

        /// <summary>
        /// Run the <see cref="IHost"/> without considering the host lifetime and the host environment.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        private static async Task InternalRunAsync(IHost This, CancellationToken Token)
        {
            bool Canceled = false;
            try { await This.StartAsync(Token); }
            catch (OperationCanceledException)
            {
                Canceled = true;
            }

            if (!Canceled)
            {
                var Tcs = new TaskCompletionSource();

                using (Token.Register(Tcs.SetResult))
                    await Tcs.Task;
            }

            await This.StopAsync();
        }
    }
}
