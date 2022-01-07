using System;

namespace Ahri.Values
{
    public partial class ValueConverter
    {
        private delegate bool TryParseDelegate<TOutput>(string Input, out TOutput Value);
        private struct Parser<TOutput>
        {
            private TryParseDelegate<TOutput> m_Delegate;

            /// <summary>
            /// Initialize a new <see cref="Parser{TOutput}"/>.
            /// </summary>
            /// <param name="Delegate"></param>
            public Parser(TryParseDelegate<TOutput> Delegate) => m_Delegate = Delegate;
            public bool TryConvert(object Input, Type OutputType, out object Value)
            {
                if (OutputType.IsAssignableFrom(typeof(TOutput)))
                {
                    if (Input is null)
                    {
                        Value = default(TOutput);
                        return true;
                    }

                    if (m_Delegate(Input.ToString(), out var _Value))
                    {
                        Value = _Value;
                        return true;
                    }
                }

                Value = null;
                return false;
            }
        }
    }
}
