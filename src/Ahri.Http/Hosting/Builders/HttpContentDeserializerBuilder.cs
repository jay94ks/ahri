using Ahri.Core;
using Ahri.Http.Hosting.Internals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ahri.Http.Hosting.Builders
{
    public class HttpContentDeserializerBuilder : IHttpContentDeserializerBuilder
    {
        private static IServiceProvider NULL_SERVICES = new ServiceCollection().BuildServiceProvider();
        private List<Func<IHttpContentDeserializer>> m_Deserializers = new();

        /// <inheritdoc/>
        public IHttpContentDeserializerBuilder Use(Func<IHttpContentDeserializer> Factory)
        {
            m_Deserializers.Add(Factory);
            return this;
        }

        /// <inheritdoc/>
        public IHttpContentDeserializerBuilder Use(Type Type, params object[] Arguments)
        {
            m_Deserializers.Add(() =>
            {
                var Injector = NULL_SERVICES.GetRequiredService<IServiceInjector>();
                return (IHttpContentDeserializer) Injector.Create(Type, Arguments);
            });

            return this;
        }

        /// <inheritdoc/>
        public IHttpContentDeserializer Build()
        {
            var Deserializers = m_Deserializers.Select(X => X.Invoke());
            var Distincts = new Dictionary<Type, IHttpContentDeserializer>();

            foreach(var Each in Deserializers)
            {
                if (Each is null)
                    continue;

                Distincts[Each.GetType()] = Each;
            }

            return new HttpContentDeserializer(Distincts.Values);
        }
    }
}
