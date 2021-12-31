namespace Ahri.Http
{
    public interface IHttpContext
    {
        /// <summary>
        /// Request.
        /// </summary>
        IHttpRequest Request { get; }

        /// <summary>
        /// Response.
        /// </summary>
        IHttpResponse Response { get; }
    }
}