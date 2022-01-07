using System.IO;

namespace Ahri.Http
{
    public interface IHttpOpaqueFeature
    {
        /// <summary>
        /// Sends response headers and make it to  a opaque transport.
        /// </summary>
        /// <returns></returns>
        Stream GetOpaqueStream();
    }
}