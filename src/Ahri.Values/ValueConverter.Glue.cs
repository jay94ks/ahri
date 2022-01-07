using System;

namespace Ahri.Values
{
    public partial class ValueConverter<TInput>
    {
        private struct Glue
        {
            private TryConvertDelegate<TInput> m_Pre;
            private TryConvertDelegate<TInput> m_Post;

            public Glue(TryConvertDelegate<TInput> Pre, TryConvertDelegate<TInput> Post)
            {
                m_Pre = Pre;
                m_Post = Post;
            }

            public bool TryConvert(TInput Input, Type OutputType, out object Value)
            {
                if (m_Pre(Input, OutputType, out Value))
                    return true;

                return m_Post(Input, OutputType, out Value);
            }
        }
    }
}
