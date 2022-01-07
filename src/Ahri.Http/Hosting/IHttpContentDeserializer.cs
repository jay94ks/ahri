using System;
using System.Threading.Tasks;

namespace Ahri.Http
{
    public interface IHttpContentDeserializer
    {
        /// <summary>
        /// Test whether the request content is deserializable or not.
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        bool CanHandle(IHttpRequest Request);

        /// <summary>
        /// Test whether the request content to target type or not.
        /// </summary>
        /// <param name="From"></param>
        /// <param name="To"></param>
        /// <returns></returns>
        bool CanConvert(object From, Type To);

        /// <summary>
        /// Deserialize the request content to object.
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        Task<object> Handle(IHttpRequest Request);

        /// <summary>
        /// Convert the request content to target type.
        /// </summary>
        /// <param name="From"></param>
        /// <param name="To"></param>
        /// <returns></returns>
        object Convert(object From, Type To);
    }
}
