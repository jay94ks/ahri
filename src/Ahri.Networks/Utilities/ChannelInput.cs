using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ahri.Networks.Utilities
{
    public class ChannelInput<T>
    {
        private int m_Counter = 0;
        private Queue<T> m_Queue = new();

        private TaskCompletionSource m_Event;
        private TaskCompletionSource m_EventRead;

        private bool m_Completed;

        public ChannelInput(int MaxCount = 16)
        {
            Output = new ChannelOutput<T>(this);

            m_Completed = false;
            m_Counter = MaxCount;
        }

        /// <summary>
        /// Output of the channel.
        /// </summary>
        public ChannelOutput<T> Output { get; }

        /// <summary>
        /// Indicates whether the channel has completed or not.
        /// </summary>
        public bool Completed
        {
            get
            {
                lock (this)
                    return m_Completed;
            }
        }

        /// <summary>
        /// Write an item to the channel.
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        public async Task WriteAsync(T Item, CancellationToken Token = default)
        {
            while(true)
            {
                Task Event;
                lock (this)
                {
                    if (m_Completed)
                        throw new InvalidOperationException("The channel has been completed.");

                    if (m_Queue.Count <= m_Counter)
                    {
                        m_Queue.Enqueue(Item);
                        m_EventRead?.TrySetResult();
                        return;
                    }

                    if (m_Event is null || m_Event.Task.IsCompleted)
                        m_Event = new TaskCompletionSource();

                    Event = m_Event.Task;
                }

                var Cancel = new TaskCompletionSource();
                using (Token.Register(Cancel.SetCanceled))
                    await Task.WhenAny(Cancel.Task, Event).Unwrap();
            }
        }

        /// <summary>
        /// Complete the channel input.
        /// </summary>
        public void Complete()
        {
            lock(this)
            {
                m_Completed = true;
                m_Event?.TrySetResult();
                m_EventRead?.TrySetResult();
            }
        }

        /// <summary>
        /// Try to read an item.
        /// </summary>
        /// <param name="Item"></param>
        /// <returns></returns>
        internal bool TryRead(out T Item)
        {
            lock (this)
            {
                if (m_Queue.TryDequeue(out Item))
                {
                    m_Event?.TrySetResult();
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Wait to read.
        /// </summary>
        /// <returns></returns>
        internal Task WaitRead()
        {
            lock(this)
            {
                if (m_Queue.Count > 0 || m_Completed)
                    return Task.CompletedTask;
                
                if (m_EventRead is null || m_EventRead.Task.IsCompleted)
                    m_EventRead = new TaskCompletionSource();

                return m_EventRead.Task;
            }
        }
    }
}
