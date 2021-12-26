using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Hosting.Winform.Internals
{
    /// <summary>
    /// Dispatches deleates that should be invoked on WinForm message pump.
    /// </summary>
    internal class WinFormMessagePump : IWinFormMessagePump
    {
        private WinFormHostService m_WinFormHostService;

        /// <summary>
        /// Initialize a new <see cref="WinFormMessagePump"/> instance.
        /// </summary>
        /// <param name="Env"></param>
        public WinFormMessagePump(WinFormHostService Service)
        {
            if (Service is WinFormHostService WinFormEnv)
                m_WinFormHostService = WinFormEnv;

            else
            {
                throw new InvalidOperationException(
                   "IHostEnvironment is not the WinForm specific host environment!");
            }
        }

        /// <inheritdoc/>
        public async Task InvokeAsync(Action Delegate)
        {
            var Form = await m_WinFormHostService.GetHiddenForm();

            if (Form.InvokeRequired)
            {
                var Tcs = new TaskCompletionSource();

                Form.Invoke(new Action(() =>
                {
                    try { Delegate?.Invoke(); }
                    catch (Exception e)
                    {
                        Tcs.SetException(e);
                        return;
                    }

                    Tcs.SetResult();
                }));

                await Tcs.Task;
                return;
            }

            Delegate?.Invoke();
        }
    }
}
