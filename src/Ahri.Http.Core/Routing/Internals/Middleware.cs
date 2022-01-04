using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Routing.Internals
{
    internal class Middleware<TContext>
    {
        private Func<TContext, Func<Task>, Task> m_Prev;
        private Func<TContext, Func<Task>, Task> m_Next;

        public Middleware(
            Func<TContext, Func<Task>, Task> Prev,
            Func<TContext, Func<Task>, Task> Next)
        {
            m_Prev = Prev;
            m_Next = Next;
        }

        struct MakeNext
        {
            private Func<TContext, Func<Task>, Task> m_Next;
            private Func<Task> m_FinalNext;
            private TContext m_Context;

            public MakeNext(
                Func<TContext, Func<Task>, Task> Next,
                Func<Task> FinalNext, TContext Context)
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
        public Task InvokeAsync(TContext Context, Func<Task> Next)
            => m_Prev(Context, new MakeNext(m_Next, Next, Context).InvokeAsync);
    }

    internal class Middleware : Middleware<IHttpContext>
    {
        public Middleware(
            Func<IHttpContext, Func<Task>, Task> Prev,
            Func<IHttpContext, Func<Task>, Task> Next) : base(Prev, Next)
        {
        }
    }
}
