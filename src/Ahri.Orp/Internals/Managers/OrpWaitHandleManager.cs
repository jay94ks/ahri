using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Orp.Internals.Managers
{
    internal class OrpWaitHandleManager : IDisposable
    {
        private Dictionary<Guid, TaskCompletionSource<OrpMessage>> m_Tcss = new();
        private bool m_Disposed = false;

        /// <summary>
        /// Reserve a new <see cref="Guid"/> to receive the reply from the remote host.
        /// </summary>
        /// <returns></returns>
        public Guid Reserve()
        {
            while(true)
            {
                var NewId = Guid.NewGuid();
                lock (this)
                {
                    if (m_Tcss.TryGetValue(NewId, out var Tcs))
                        continue;

                    m_Tcss[NewId] = new TaskCompletionSource<OrpMessage>();

                    if (m_Disposed)
                        m_Tcss[NewId].TrySetResult(new OrpMessage(OrpMessageState.Aborted, null));

                    return NewId;
                }
            }
        }

        /// <summary>
        /// Take the wait handle for the reserved <paramref name="Guid"/>.
        /// </summary>
        /// <param name="Guid"></param>
        /// <returns></returns>
        public Task<OrpMessage> GetWaitHandle(Guid Guid)
        {
            lock(this)
            {
                if (!m_Tcss.TryGetValue(Guid, out var Tcs))
                    return Task.FromResult(new OrpMessage(OrpMessageState.InvalidMessageId, null));

                return Tcs.Task;
            }
        }

        /// <summary>
        /// Set completion for the wait handle.
        /// </summary>
        /// <param name="Guid"></param>
        /// <param name="Message"></param>
        public void SetCompletionToWaitHandle(Guid Guid, OrpMessage Message)
        {
            lock (this)
            {
                if (!m_Tcss.Remove(Guid, out var Tcs))
                    return;

                Tcs.TrySetResult(Message);
            }
        }

        /// <summary>
        /// Dispose the <see cref="OrpWaitHandleManager"/> instance and
        /// notify abortion to all wait handles.
        /// </summary>
        public void Dispose()
        {
            lock(this)
            {
                if (m_Disposed)
                    return;

                m_Disposed = true;


            }

            while (true)
            {
                var Queue = new Queue<TaskCompletionSource<OrpMessage>>();
                lock(this)
                {
                    foreach (var Each in m_Tcss.Values)
                        Queue.Enqueue(Each);
                    if (Queue.Count <= 0)
                        break;

                    m_Tcss.Clear();
                }

            }
        }
    }
}
