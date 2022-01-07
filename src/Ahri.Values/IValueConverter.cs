using System;

namespace Ahri.Values
{
    public delegate bool TryConvertDelegate(object Input, Type OutputType, out object Value);
    public delegate bool TryConvertDelegate<TInput>(TInput Input, Type OutputType, out object Value);

    public interface IValueConverter : ICloneable
    {
        /// <summary>
        /// Extends the value converter using <paramref name="Delegate"/>.
        /// </summary>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        IValueConverter Extends(TryConvertDelegate Delegate);

        /// <summary>
        /// Try to convert <paramref name="Input"/> to <paramref name="OutputType"/>.
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        bool TryConvert(object Input, Type OutputType, out object Value);

        /// <summary>
        /// Clone the <see cref="IValueConverter"/> instance.
        /// </summary>
        /// <returns></returns>
        new IValueConverter Clone();
    }

    public interface IValueConverter<TInput> : IValueConverter
    {
        /// <summary>
        /// Extends the value converter using <paramref name="Delegate"/>.
        /// </summary>
        /// <param name="Delegate"></param>
        /// <returns></returns>
        IValueConverter<TInput> Extends(TryConvertDelegate<TInput> Delegate);

        /// <summary>
        /// Try to convert <paramref name="Input"/> to <paramref name="OutputType"/>.
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        bool TryConvert(TInput Input, Type OutputType, out object Value);

        /// <summary>
        /// Clone the <see cref="IValueConverter<TInput>"/> instance.
        /// </summary>
        /// <returns></returns>
        new IValueConverter<TInput> Clone();
    }
}
