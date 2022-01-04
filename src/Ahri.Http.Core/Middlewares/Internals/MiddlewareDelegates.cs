using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Middlewares.Internals
{
    internal class MiddlewareDelegates
    {
        /// <summary>
        /// Merge multiple middlewares to single delegate.
        /// </summary>
        /// <param name="Middlewares"></param>
        /// <returns></returns>
        public static Func<IHttpContext, Func<Task>, Task> Merge(IEnumerable<Func<IHttpContext, Func<Task>, Task>> Middlewares)
        {
            Func<IHttpContext, Func<Task>, Task> Built = null;

            foreach(var Each in Middlewares)
            {
                if (Built != null)
                    Built = new Glue(Built, Each).InvokeAsync;

                else
                    Built = Each;
            }

            return Built;
        }

        internal struct Glue
        {
            private Func<IHttpContext, Func<Task>, Task> m_Prev;
            private Func<IHttpContext, Func<Task>, Task> m_Next;

            public Glue(
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
}
