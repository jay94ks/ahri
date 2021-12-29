using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ahri.Networks.Utilities
{
    public class ChannelOutput<T>
    {
        private ChannelInput<T> m_Input;

        /// <summary>
        /// Initialize a new <see cref="ChannelOutput{T}"/> instance.
        /// </summary>
        /// <param name="Input"></param>
        internal ChannelOutput(ChannelInput<T> Input) => m_Input = Input;

        /// <summary>
        /// Indicates whether the channel has completed or not.
        /// </summary>
        public bool Completed => m_Input.Completed;

        /// <summary>
        /// Read an item from the channel.
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        public async Task<T> ReadAsync(CancellationToken Token = default)
        {
            while(true)
            {
                if (m_Input.TryRead(out var Item))
                    return Item;

                if (m_Input.Completed)
                    throw new InvalidOperationException("The channel has been completed.");

                var Cancel = new TaskCompletionSource();
                using (Token.Register(Cancel.SetCanceled))
                    await Task.WhenAny(m_Input.WaitRead(), Cancel.Task).Unwrap();
            }
        }
    }
}
