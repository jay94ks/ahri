namespace Ahri.Orp
{
    public struct OrpMessage
    {
        /// <summary>
        /// Indicates the message handling state.
        /// </summary>
        public OrpMessageState State { get; }

        /// <summary>
        /// Reply instance.
        /// </summary>
        public object Reply { get; }

        /// <summary>
        /// Initialize a new <see cref="OrpMessage"/>.
        /// </summary>
        /// <param name="State"></param>
        /// <param name="Reply"></param>
        public OrpMessage(OrpMessageState State, object Reply)
        {
            this.State = State;
            this.Reply = Reply;
        }
    }
}
