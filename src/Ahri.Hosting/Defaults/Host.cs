using Ahri.Hosting.Internals;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ahri.Hosting.Defaults
{
    /// <summary>
    /// The default implementation of the IHost interface.
    /// Usually this doesn't be required to override, but it can if it should be.
    /// </summary>
    public class Host : IHost
    {
        private IHostedService m_HostedService;
        private HostLifetime m_HostLifetime;
        private bool m_Disposed = false;

        /// <summary>
        /// Initialize a new <see cref="Host"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        public Host(IServiceProvider Services)
        {
            this.Services = Services;
            if (!Services.TryGetRequiredService(out m_HostedService))
            {
                throw new InvalidOperationException(
                    "No hosted service are registered for the host.");
            }

            if (!(Services.GetRequiredService<IHostLifetime>() is HostLifetime Lifetime))
            {
                throw new InvalidOperationException(
                    "IHostLifetime instance shouldn't be replaced to other implementation.");
            }

            m_HostLifetime = Lifetime;
        }

        /// <summary>
        /// If the instance has not been disposed, call it.
        /// </summary>
        ~Host() => Dispose(true);

        /// <inheritdoc/>
        public IServiceProvider Services { get; }

        /// <inheritdoc/>
        public virtual async Task StartAsync(CancellationToken Token = default)
        {
            await m_HostedService.StartAsync(Token);
            m_HostLifetime.OnStarted();
        }

        /// <inheritdoc/>
        public virtual async Task StopAsync()
        {
            try { await m_HostedService.StopAsync(); }
            finally { m_HostLifetime.OnStopped(); }
        }

        /// <inheritdoc/>
        public void Dispose() => Dispose(false);

        /// <summary>
        /// Invokes internal dispose routines that receives <paramref name="Critical"/> state.
        /// </summary>
        /// <param name="Critical"></param>
        private void Dispose(bool Critical)
        {
            lock (this)
            {
                if (m_Disposed)
                    return;

                m_Disposed = true;
            }

            bool Disposed = false;
            Dispose(() => Dispose(ref Disposed), Critical);
            Dispose(ref Disposed);
        }

        /// <summary>
        /// Invokes <see cref="IDisposable.Dispose"/> of <see cref="IServiceProvider"/> 
        /// instance if <paramref name="Disposed"/> is not true.
        /// </summary>
        /// <param name="Disposed"></param>
        private void Dispose(ref bool Disposed)
        {
            lock (this)
            {
                if (Disposed)
                    return;

                Disposed = true;
            }

            if (Services is IDisposable Sync)
                Sync.Dispose();

            else if (Services is IAsyncDisposable Async)
                Async.DisposeAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Perform custom <see cref="Dispose"/> behavior. Users can use the DisposeDelegate delegate
        /// to place tasks before and after the destruction of host services.
        /// </summary>
        /// <param name="DisposeDelegate"></param>
        /// <param name="Critical">
        /// Represents the <see cref="Dispose"/> is called under critical state or not. 
        /// Note that critical state means the call is under the finalizer that is already in destroying.
        /// </param>
        protected virtual void Dispose(Action DisposeDelegate, bool Critical) { }
    }
}
