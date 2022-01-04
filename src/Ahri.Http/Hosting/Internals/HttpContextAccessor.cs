namespace Ahri.Http.Hosting.Internals
{
    internal class HttpContextAccessor : IHttpContextAccessor
    {
        /// <inheritdoc/>
        public IHttpContext Instance { get; set; }
    }
}
