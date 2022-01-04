using System;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Actions
{
    public class RedirectTo : IHttpAction
    {
        private string m_Where;
        private bool m_Permanent;

        /// <summary>
        /// Initialize a new <see cref="RedirectTo"/> instance.
        /// </summary>
        /// <param name="Where"></param>
        /// <param name="Permanent"></param>
        public RedirectTo(string Where, bool Permanent = false)
        {
            m_Where = Where ?? throw new ArgumentNullException(nameof(Where));
            m_Permanent = Permanent;
        }

        /// <inheritdoc/>
        public Task InvokeAsync(IHttpContext Context)
        {
            Context.Response.Status = m_Permanent ? 301 : 302;
            Context.Response.Headers.Set("Location", m_Where);
            return Task.CompletedTask;
        }
    }
}
