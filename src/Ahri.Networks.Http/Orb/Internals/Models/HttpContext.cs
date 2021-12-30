namespace Ahri.Http.Orb.Internals.Models
{
    public class HttpContext : IHttpContext
    {
        internal HttpContext(IHttpRequest Request, IHttpResponse Response)
        {
            this.Request = Request;
            this.Response = Response;
        }

        /// <summary>
        /// Request.
        /// </summary>
        public IHttpRequest Request { get; }

        /// <summary>
        /// Response.
        /// </summary>
        public IHttpResponse Response { get; }
    }
}
