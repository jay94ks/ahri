using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Orp.Internals
{
    internal struct OrpMiddleware
    {
        private Func<IOrpContext, Func<Task>, Task> m_Prev;
        private Func<IOrpContext, Func<Task>, Task> m_Next;

        public OrpMiddleware(
            Func<IOrpContext, Func<Task>, Task> Prev,
            Func<IOrpContext, Func<Task>, Task> Next)
        {
            m_Prev = Prev;
            m_Next = Next;
        }

        struct MakeNext
        {
            private Func<IOrpContext, Func<Task>, Task> m_Next;
            private Func<Task> m_FinalNext;
            private IOrpContext m_Context;

            public MakeNext(
                Func<IOrpContext, Func<Task>, Task> Next,
                Func<Task> FinalNext, IOrpContext Context)
            {
                m_Next = Next;
                m_FinalNext = FinalNext;
                m_Context = Context;
            }

            [DebuggerHidden]
            public Task InvokeAsync()
                => m_Next(m_Context, m_FinalNext);
        }

        [DebuggerHidden]
        public Task InvokeAsync(IOrpContext Context, Func<Task> Next)
            => m_Prev(Context, new MakeNext(m_Next, Next, Context).InvokeAsync);
    }
}
