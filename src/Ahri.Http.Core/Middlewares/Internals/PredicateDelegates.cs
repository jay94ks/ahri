using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Core.Middlewares.Internals
{
    internal class PredicateDelegates
    {
        /// <summary>
        /// Merge predicates into single delegate.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="Delegates"></param>
        /// <returns></returns>
        public static Func<TInput, bool> MergeAnd<TInput>(IEnumerable<Func<TInput, bool>> Delegates)
        {
            Func<TInput, bool> Built = null;

            foreach (var Each in Delegates)
            {
                if (Each is null)
                    continue;

                if (Built != null)
                    Built = new And<TInput>(Built, Each).Predicate;

                else
                    Built = Each;
            }

            return Built;
        }

        /// <summary>
        /// Merge predicates into single delegates.
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <param name="Delegates"></param>
        /// <returns></returns>
        public static Func<TInput, bool> MergeOr<TInput>(IEnumerable<Func<TInput, bool>> Delegates)
        {
            Func<TInput, bool> Built = null;

            foreach (var Each in Delegates)
            {
                if (Each is null)
                    continue;

                if (Built != null)
                    Built = new Or<TInput>(Built, Each).Predicate;

                else
                    Built = Each;
            }

            return Built;
        }

        private struct And<TInput>
        {
            private Func<TInput, bool> m_Left;
            private Func<TInput, bool> m_Right;

            public And(
                Func<TInput, bool> Left,
                Func<TInput, bool> Right)
            {
                m_Left = Left;
                m_Right = Right;
            }

            public bool Predicate(TInput Input)
                => m_Left(Input) && m_Right(Input);
        }


        private struct Or<TInput>
        {
            private Func<TInput, bool> m_Left;
            private Func<TInput, bool> m_Right;

            public Or(
                Func<TInput, bool> Left,
                Func<TInput, bool> Right)
            {
                m_Left = Left;
                m_Right = Right;
            }

            public bool Predicate(TInput Input)
                => m_Left(Input) || m_Right(Input);
        }
    }
}
