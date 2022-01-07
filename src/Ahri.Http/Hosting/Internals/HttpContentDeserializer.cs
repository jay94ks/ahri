using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Http.Hosting.Internals
{
    internal class HttpContentDeserializer : IHttpContentDeserializer
    {
        private static readonly Task<object> NULL = Task.FromResult(null as object);
        private IHttpContentDeserializer[] m_Deserializers;

        /// <summary>
        /// Initialize a new <see cref="HttpContentDeserializer"/> instance.
        /// </summary>
        /// <param name="Deserializers"></param>
        public HttpContentDeserializer(IEnumerable<IHttpContentDeserializer> Deserializers)
            => m_Deserializers = Deserializers.ToArray();

        /// <inheritdoc/>
        public bool CanConvert(object From, Type To)
        {
            foreach (var Each in m_Deserializers)
            {
                if (Each.CanConvert(From, To))
                    return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public bool CanHandle(IHttpRequest Request)
        {
            foreach(var Each in m_Deserializers)
            {
                if (Each.CanHandle(Request))
                    return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public object Convert(object From, Type To)
        {
            foreach (var Each in m_Deserializers)
            {
                if (Each.CanConvert(From, To))
                    return Each.Convert(From, To);
            }

            return null;
        }

        /// <inheritdoc/>
        public Task<object> Handle(IHttpRequest Request)
        {
            foreach(var Each in m_Deserializers)
            {
                if (Each.CanHandle(Request))
                    return Each.Handle(Request);
            }

            return NULL;
        }
    }
}
