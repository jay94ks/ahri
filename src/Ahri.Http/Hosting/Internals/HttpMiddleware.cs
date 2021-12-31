using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ahri.Http.Hosting.Internals
{
    internal struct HttpMiddleware
    {
        private Func<IHttpContext, Func<Task>, Task> m_Prev;
        private Func<IHttpContext, Func<Task>, Task> m_Next;

        public HttpMiddleware(
            Func<IHttpContext, Func<Task>, Task> Prev,
            Func<IHttpContext, Func<Task>, Task> Next)
        {
            m_Prev = Prev;
            m_Next = Next;
        }

        struct MakeNext
        {
            private Func<IHttpContext, Func<Task>, Task> m_Next;
            private Func<Task> m_FinalNext;
            private IHttpContext m_Context;

            public MakeNext(
                Func<IHttpContext, Func<Task>, Task> Next,
                Func<Task> FinalNext, IHttpContext Context)
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
        public Task InvokeAsync(IHttpContext Context, Func<Task> Next)
            => m_Prev(Context, new MakeNext(m_Next, Next, Context).InvokeAsync);
    }
}
