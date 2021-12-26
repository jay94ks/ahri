using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ahri.Hosting.Windows
{
    [SupportedOSPlatform("windows")]
    internal class WindowsService : ServiceBase, IHostEnvironment
    {
        private TaskCompletionSource m_Executor = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private ManualResetEventSlim m_EventStop = new();
        private IHostLifetime m_Lifetime;

        /// <summary>
        /// Initialize a <see cref="WindowsService"/> instance.
        /// </summary>
        /// <param name="Lifetime"></param>
        public WindowsService(IHostLifetime Lifetime) 
            => m_Lifetime = Lifetime;

        /// <inheritdoc/>
        public Task PrepareAsync(CancellationToken Token = default)
        {
            (new Thread(Run) { IsBackground = true }).Start();
            return m_Executor.Task;
        }

        /// <inheritdoc/>
        public Task FinishAsync() => Task.CompletedTask;

        /// <summary>
        /// Run the service using <see cref="ServiceBase.Run(ServiceBase)"/> method.
        /// </summary>
        private void Run()
        {
            try
            {
                Run(this); // --> Blocked here.

                if (!m_Executor.Task.IsCompleted)
                    throw new InvalidOperationException("Fatal error: the host isn't started?");
            }
            catch (Exception ex)
            {
                m_Executor.TrySetException(ex);
            }
        }

        /// <inheritdoc/>
        protected override void OnStart(string[] args)
        {
            m_Executor.TrySetResult();
            base.OnStart(args);
        }

        /// <inheritdoc/>
        protected override void OnStop()
        {
            Terminate();
            base.OnStop();
        }

        /// <inheritdoc/>
        protected override void OnShutdown()
        {
            Terminate();
            base.OnShutdown();
        }

        /// <summary>
        /// Terminates the application by calling <see cref="IHostLifetime.Terminate"/>.
        /// </summary>
        private void Terminate()
        {
            m_Lifetime.Terminate();

            using (m_Lifetime.Stopped.Register(m_EventStop.Set))
                m_EventStop.Wait();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool Disposing)
        {
            if (Disposing)
                m_EventStop.Set();

            base.Dispose(Disposing);
        }
    }
}
