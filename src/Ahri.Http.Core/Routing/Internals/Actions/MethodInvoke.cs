using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Routing.Internals.Actions
{
    internal class MethodInvoke : IHttpAction
    {
        private ControllerContext m_Context;

        /// <summary>
        /// Initialize a new <see cref="MethodInvoke"/> instance
        /// </summary>
        /// <param name="Context"></param>
        public MethodInvoke(ControllerContext Context)
            => m_Context = Context;

        /// <summary>
        /// Invoke the controller's specific method.
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        public async Task InvokeAsync(IHttpContext _)
        {
            var Injector = m_Context.HttpContext.Request
                .Services.GetRequiredService<IServiceInjector>();

            var Result = Injector.Invoke(m_Context.TargetMethod, m_Context.TargetInstance);
            if (Result is Task<IHttpAction> ActionAsync)
                await (await ActionAsync).InvokeAsync(m_Context.HttpContext);

            else if (Result is IHttpAction Action)
                await Action.InvokeAsync(m_Context.HttpContext);
        }
    }
}
