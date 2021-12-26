using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Hosting.Winform
{
    /// <summary>
    /// Dispatches deleates that should be invoked on WinForm message pump.
    /// </summary>
    public interface IWinFormMessagePump
    {
        /// <summary>
        /// Invoke a delegate on the WinForm thread.
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
        Task InvokeAsync(Action Delegate);
    }
}
