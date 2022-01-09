using Ahri.Networks.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahri.Orp.Internals
{
    public class OrpServer : TcpServer<OrpConnection>
    {
        private IOrpMappings m_Mappings;
        private Func<IOrpContext, Task> m_Endpoint;

        private IEnumerable<Func<IOrpConnection, Task>> m_Greetings;
        private IEnumerable<Func<IOrpConnection, Task>> m_Farewells;

        /// <summary>
        /// Initialize a new <see cref="OrpServer"/> instance.
        /// </summary>
        /// <param name="Services"></param>
        /// <param name="Options"></param>
        public OrpServer(IServiceProvider Services, OrpServerOptions Options)
            : base(Services, Options.LocalEndPoint)
        {

        }

        /// <summary>
        /// Set Mappings for the server.
        /// </summary>
        /// <param name="Mappings"></param>
        public void SetMappings(IOrpMappings Mappings) => m_Mappings = Mappings;

        /// <summary>
        /// Set Endpoint for the server.
        /// </summary>
        /// <param name="Endpoint"></param>
        public void SetEndpoint(Func<IOrpContext, Task> Endpoint) => m_Endpoint = Endpoint;

        /// <summary>
        /// Set Greeting delegate enumerable.
        /// </summary>
        /// <param name="Greetings"></param>
        public void SetGreetings(IEnumerable<Func<IOrpConnection, Task>> Greetings) => m_Greetings = Greetings;

        /// <summary>
        /// Set Farewell delegate enumerable.
        /// </summary>
        /// <param name="Farewells"></param>
        public void SetFarewells(IEnumerable<Func<IOrpConnection, Task>> Farewells) => m_Farewells = Farewells;

        /// <summary>
        /// Configure the mapping and endpoint.
        /// </summary>
        /// <param name="Session"></param>
        /// <returns></returns>
        protected override Task OnConfigure(OrpConnection Session)
        {
            Session.SetMappings(m_Mappings);
            Session.SetEndpoint(m_Endpoint);
            Session.SetGreetings(m_Greetings);
            Session.SetFarewells(m_Farewells);

            return base.OnConfigure(Session);
        }
    }
}
