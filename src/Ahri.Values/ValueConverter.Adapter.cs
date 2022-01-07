using System;

namespace Ahri.Values
{
    public partial class ValueConverter
    {
        private struct Adapter<TInput>
        {
            private TryConvertDelegate<TInput> m_Delegate;

            /// <summary>
            /// Initialize a new <see cref="Adapter"/>.
            /// </summary>
            /// <param name="Delegate"></param>
            public Adapter(TryConvertDelegate<TInput> Delegate) => m_Delegate = Delegate;

            /// <inheritdoc/>
            public bool TryConvert(object Input, Type OutputType, out object Value)
            {
                if (Input is TInput _Input)
                    return m_Delegate(_Input, OutputType, out Value);

                Value = null;
                return false;
            }
        }

    }

    public partial class ValueConverter<TInput>
    {
        private struct Adapter
        {
            private TryConvertDelegate m_Delegate;

            /// <summary>
            /// Initialize a new <see cref="Adapter"/>.
            /// </summary>
            /// <param name="Delegate"></param>
            public Adapter(TryConvertDelegate Delegate) => m_Delegate = Delegate;
            public bool TryConvert(TInput Input, Type OutputType, out object Value)
                => m_Delegate(Input, OutputType, out Value);
        }

    }
}
