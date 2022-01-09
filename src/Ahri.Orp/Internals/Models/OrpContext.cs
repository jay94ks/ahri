using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Orp.Internals.Models
{
    internal class OrpContext : IOrpContext
    {
        private bool m_Replied = false;
        
        /// <inheritdoc/>
        public bool IsNotification { get; set; }

        /// <inheritdoc/>
        public IOrpConnection Connection { get; set; }

        /// <inheritdoc/>
        public IServiceProvider Services { get; internal set; }

        /// <inheritdoc/>
        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();

        /// <inheritdoc/>
        public Guid MessageId { get; set; }

        /// <inheritdoc/>
        public object Message { get; set; }

        /// <summary>
        /// State of the context.
        /// </summary>
        public OrpMessageState State { get; set; }

        /// <inheritdoc/>
        public async Task ReplyTo(object Message)
        {
            if (IsNotification)
                throw new NotSupportedException("Notification cannot be replied.");

            if (Connection is OrpConnection Orp)
            {
                m_Replied = true;
                await Orp.ReplyTo(MessageId, Message, OrpMessageState.Success);
                return;
            }

            throw new InvalidOperationException("This connection is not repliable.");
        }

        /// <summary>
        /// Reply Error.
        /// </summary>
        /// <returns></returns>
        public async Task TryReplyError()
        {
            if (!IsNotification && !m_Replied && Connection is OrpConnection Orp)
            {
                m_Replied = true;
                await Orp.ReplyTo(MessageId, null, OrpMessageState.NotImplemented);
            }

        }
    }
}
