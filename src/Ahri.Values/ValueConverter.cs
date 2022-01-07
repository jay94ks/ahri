using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Ahri.Values
{
    public partial class ValueConverter : ValueConverter<object>
    {
        private static readonly Type[] PRIMITIVES = new Type[]
        {
            typeof(char), typeof(byte), typeof(short), typeof(int),
            typeof(long), typeof(ushort), typeof(uint), typeof(ulong),
            typeof(float), typeof(double), typeof(Guid), typeof(DateTime)
        };

        /// <summary>
        /// Default instance.
        /// </summary>
        public static IValueConverter Default { get; } = new ValueConverter();

        /// <summary>
        /// <see cref="object.ToString"/> wrapper.
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="OutputType"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        private static bool TryConvertToString(object Input, Type OutputType, out object Value)
        {
            if ((Input is null || PRIMITIVES.Contains(Input.GetType())) &&
                OutputType.IsAssignableFrom(typeof(string)))
            {
                if (Input != null)
                {
                    if (Input is DateTime Date)
                        Value = Date.ToString("O");

                    else
                        Value = Input.ToString();
                }
                else
                    Value = null;

                return true;
            }


            Value = null;
            return false;
        }

        /// <summary>
        /// <see cref="DateTime.TryParse"/> wrapper.
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="OutputType"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        private static bool TryConvertToDateTime(object Input, Type OutputType, out object Value)
        {
            if (OutputType.IsAssignableFrom(typeof(DateTime)))
            {
                var Culture = CultureInfo.CurrentCulture;
                var InputString = Input != null ? Input.ToString() : "";

                if (DateTime.TryParse(InputString, Culture, DateTimeStyles.RoundtripKind, out var _Value) ||
                    DateTime.TryParse(InputString, Culture, DateTimeStyles.None, out _Value) ||
                    DateTime.TryParse(InputString, out _Value))
                {
                    Value = _Value;
                    return true;
                }
            }

            Value = null;
            return false;
        }

        /// <summary>
        /// Initialize the default value converters.
        /// </summary>
        static ValueConverter() => Default
            .Extends(new Parser<char>(char.TryParse).TryConvert)
            .Extends(new Parser<byte>(byte.TryParse).TryConvert)
            .Extends(new Parser<short>(short.TryParse).TryConvert)
            .Extends(new Parser<int>(int.TryParse).TryConvert)
            .Extends(new Parser<long>(long.TryParse).TryConvert)
            .Extends(new Parser<ushort>(ushort.TryParse).TryConvert)
            .Extends(new Parser<uint>(uint.TryParse).TryConvert)
            .Extends(new Parser<ulong>(ulong.TryParse).TryConvert)
            .Extends(new Parser<float>(float.TryParse).TryConvert)
            .Extends(new Parser<double>(double.TryParse).TryConvert)
            .Extends(new Parser<Guid>(Guid.TryParse).TryConvert)
            .Extends(TryConvertToDateTime)
            .Extends(TryConvertToString);

        /// <summary>
        /// Extends the value converter using <paramref name="Delegate"/>.
        /// </summary>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        public IValueConverter Extends<TInput>(TryConvertDelegate<TInput> Delegate)
            => Extends(new Adapter<TInput>(Delegate).TryConvert);

    }

    public partial class ValueConverter<TInput> : IValueConverter<TInput>
    {
        private TryConvertDelegate<TInput> m_Delegate;

        /// <summary>
        /// Initialize a new <see cref="ValueConverter"/> instance.
        /// </summary>
        public ValueConverter() { }

        /// <inheritdoc/>
        public IValueConverter<TInput> Clone()
        {
            return new ValueConverter<TInput>
            {
                m_Delegate = TryConvert
            };
        }

        /// <inheritdoc/>
        public IValueConverter<TInput> Extends(TryConvertDelegate<TInput> Delegate)
        {
            lock(this)
            {
                if (m_Delegate != null)
                    m_Delegate = new Glue(m_Delegate, Delegate).TryConvert;

                else
                    m_Delegate = Delegate;
            }

            return this;
        }

        /// <inheritdoc/>
        public bool TryConvert(TInput Input, Type OutputType, out object Value)
        {
            TryConvertDelegate<TInput> Delegate;

            lock (this)
                Delegate = m_Delegate;

            if (Delegate != null)
                return Delegate(Input, OutputType, out Value);

            Value = null;
            return false;
        }

        /// <inheritdoc/>
        bool IValueConverter.TryConvert(object Input, Type OutputType, out object Value)
        {
            if (Input is TInput _Input)
                return TryConvert(_Input, OutputType, out Value);

            Value = null;
            return false;
        }

        /// <inheritdoc/>
        IValueConverter IValueConverter.Extends(TryConvertDelegate Delegate) => Extends(new Adapter(Delegate).TryConvert);

        /// <inheritdoc/>
        object ICloneable.Clone() => Clone();

        /// <inheritdoc/>
        IValueConverter IValueConverter.Clone() => Clone();

    }
}
