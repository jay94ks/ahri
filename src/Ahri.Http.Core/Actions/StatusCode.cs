using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Actions
{
    public class StatusCode : IHttpAction
    {
        private int m_Status;
        private string m_Phrase;

        /// <summary>
        /// Initialize a new <see cref="StatusCode"/> instance.
        /// </summary>
        /// <param name="Status"></param>
        /// <param name="Phrase"></param>
        public StatusCode(int Status, string Phrase = null)
        {
            m_Status = Status;
            m_Phrase = Phrase;
        }

        /// <inheritdoc/>
        public Task InvokeAsync(IHttpContext Context)
        {
            var Response = Context.Response;

            /* Set Status-Code and its Phrase. */
            Response.Status = m_Status;
            Response.Phrase = m_Phrase;

            return Task.CompletedTask;
        }
    }
}
