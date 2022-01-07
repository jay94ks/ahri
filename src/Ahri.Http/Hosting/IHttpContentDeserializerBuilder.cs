using System;

namespace Ahri.Http
{
    public interface IHttpContentDeserializerBuilder
    {
        /// <summary>
        /// Adds a factory delegate that creates the <see cref="IHttpContentDeserializer"/> instance.
        /// </summary>
        /// <param name="Factory"></param>
        /// <returns></returns>
        IHttpContentDeserializerBuilder Use(Func<IHttpContentDeserializer> Factory);

        /// <summary>
        /// Adds a deserializer type to be instantiated.
        /// </summary>
        /// <returns></returns>
        IHttpContentDeserializerBuilder Use(Type Type, params object[] Arguments);

        /// <summary>
        /// Build the <see cref="IHttpContentDeserializer"/>.
        /// </summary>
        /// <returns></returns>
        IHttpContentDeserializer Build();
    }
}
