using System;

namespace Ahri.Http.Orb.Internals.Models
{
    public class HttpContext : IHttpContext
    {
        private Func<Type, object> m_FeatureRequest;

        /// <summary>
        /// Initialize a new <see cref="HttpContext"/> instance.
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="Response"></param>
        /// <param name="FeatureRequest"></param>
        internal HttpContext(
            IHttpRequest Request, IHttpResponse Response,
            Func<Type, object> FeatureRequest)
        {
            this.Request = Request;
            this.Response = Response;

            m_FeatureRequest = FeatureRequest;
        }

        /// <inheritdoc/>
        public IHttpRequest Request { get; }

        /// <inheritdoc/>
        public IHttpResponse Response { get; }

        /// <inheritdoc/>
        public TFeature GetFeature<TFeature>()
        {
            if (m_FeatureRequest != null)
                return (TFeature) m_FeatureRequest(typeof(TFeature));

            return default;
        }
    }
}
