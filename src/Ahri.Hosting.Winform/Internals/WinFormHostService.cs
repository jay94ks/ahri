using Ahri.Hosting.Defaults;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ahri.Hosting.Winform.Internals
{
    /// <summary>
    /// WinForm Host Service that runs the windows message loop.
    /// This implementation creates an STA thread and launches a WinForm message loop.
    /// </summary>
    internal class WinFormHostService : IHostedService
    {
        private static CancellationTokenSource CANCELED = new CancellationTokenSource();
        private WinFormHostService() => CANCELED.Cancel();

        private TaskCompletionSource<Form> m_Dispatcher = new TaskCompletionSource<Form>();

        private IHostLifetime m_Lifetime;
        private TaskCompletionSource m_Ready;
        private TaskCompletionSource m_Stopped;
        private Thread m_STAThread;

        /// <summary>
        /// Initialize a new <see cref="WinFormHostService"/>.
        /// </summary>
        /// <param name="Lifetime"></param>
        public WinFormHostService(IHostLifetime Lifetime)
            => m_Lifetime = Lifetime;

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken Token = default) => Task.CompletedTask;

        /// <summary>
        /// Prepare the windows message loop.
        /// </summary>
        /// <returns></returns>
        private Task PrepareAsync()
        {
            if (m_STAThread is null || !m_STAThread.IsAlive)
            {
                if (m_Ready is null || m_Ready.Task.IsCompleted)
                    m_Ready = new TaskCompletionSource();

                if (m_Stopped is null || m_Stopped.Task.IsCompleted)
                    m_Stopped = new TaskCompletionSource();

                if (m_Dispatcher is null || m_Dispatcher.Task.IsCompleted)
                    m_Dispatcher = new TaskCompletionSource<Form>();

                m_STAThread = new Thread(OnSTAThread)
                {
                    Name = "STA Thread for WinForms",
                    IsBackground = true
                };

                m_STAThread.SetApartmentState(ApartmentState.STA);
                m_STAThread.Start();
            }

            return m_Ready.Task;
        }

        /// <inheritdoc/>
        public Task StopAsync()
        {
            if (m_Ready is null || m_Stopped is null)
                return Task.CompletedTask;

            return m_Stopped.Task;
        }

        /// <summary>
        /// Get the Form instance asynchronously.
        /// </summary>
        /// <returns></returns>
        internal async Task<Form> GetHiddenForm()
        {
            await PrepareAsync();
            return await m_Dispatcher.Task;
        }

        /// <summary>
        /// STA Thread that runs the windows message pump.
        /// </summary>
        /// <param name="obj"></param>
        private void OnSTAThread(object obj)
        {
            try
            {
                using (m_Lifetime.Stopping.Register(() => _ = KillMessageLoop()))
                {
                    Application.EnableVisualStyles();
                    Application.SetHighDpiMode(HighDpiMode.SystemAware);
                    Application.SetCompatibleTextRenderingDefault(false);
                    Form Form = CreateHiddenForm();

                    Application.Run(Form);
                }
            }

            finally
            {
                m_Lifetime.Terminate();
                m_Dispatcher.TrySetCanceled();
                m_Stopped.SetResult();
            }
        }

        /// <summary>
        /// Creates a hidden form that is used like the message dispatcher.
        /// </summary>
        /// <returns></returns>
        private Form CreateHiddenForm()
        {
            var Form = new Form();

            Form.Text = "";
            Form.Location = new Point(-1000, -1000);
            Form.Size = new Size(0, 0);
            Form.ShowInTaskbar = false;
            Form.FormBorderStyle = FormBorderStyle.None;
            Form.WindowState = FormWindowState.Minimized;
            Form.Shown += (X, E) =>
            {
                Form.Hide();
                m_Ready.TrySetResult();
                m_Dispatcher.TrySetResult(Form);
            };

            Form.FormClosing += (X, E) =>
            {
                if (E.CloseReason == CloseReason.UserClosing)
                {
                    E.Cancel = true;
                    Form.Hide();
                }
            };

            return Form;
        }

        /// <summary>
        /// Kills the <see cref="Application"/> message loop.
        /// </summary>
        /// <returns></returns>
        private async Task KillMessageLoop()
        {
            Task<Form> Dispatcher;
            try
            {
                lock (this)
                    Dispatcher = m_Dispatcher.Task;

                var Form = await Dispatcher;
                if (Form.IsDisposed)
                    return;

                if (Form.InvokeRequired)
                    Form.Invoke(new Action(Form.Dispose));

                else
                    Form.Close();
            }
            catch (ObjectDisposedException) { }
            catch (OperationCanceledException) { }
        }
    }
}
