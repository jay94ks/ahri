using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Orp
{
    public interface IOrpContext
    {
        /// <summary>
        /// Indicates whether the context is notification or not.
        /// </summary>
        bool IsNotification { get; }

        /// <summary>
        /// Connection instance that received this message.
        /// </summary>
        IOrpConnection Connection { get; }

        /// <summary>
        /// Service Provider for the context.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Properties that generated until processing the context.
        /// </summary>
        IDictionary<object, object> Properties { get; }

        /// <summary>
        /// Message Id.
        /// </summary>
        Guid MessageId { get; }

        /// <summary>
        /// Message Instance.
        /// </summary>
        object Message { get; }

        /// <summary>
        /// Reply a message to remote host.
        /// If the <see cref="IOrpContext"/> is notification,
        /// This will throw <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
        Task ReplyTo(object Message);
    }
}
