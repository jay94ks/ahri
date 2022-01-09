namespace Ahri.Orp
{
    public enum OrpMessageState
    {
        /// <summary>
        /// Success to send the message and, received the reply message from remote host.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The message object isn't supported to be sent.
        /// </summary>
        NotSupported,

        /// <summary>
        /// The reply message object isn't implemented for the connection.
        /// </summary>
        NotImplemented,

        /// <summary>
        /// Failed to handle the message due to remote host has thrown the exception.
        /// </summary>
        RemoteError,

        /// <summary>
        /// Failed to restore the message to object because of the exception occurred.
        /// </summary>
        LocalError,

        /// <summary>
        /// Failed to emit due to message couldn't be serialized.
        /// </summary>
        LocalEmitError,

        /// <summary>
        /// No message id reserved.
        /// </summary>
        InvalidMessageId,

        /// <summary>
        /// Aborted because the connection has been lost.
        /// </summary>
        Aborted
    }
}
