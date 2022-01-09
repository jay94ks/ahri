using System.Net;
using System.Threading.Tasks;

namespace Ahri.Orp
{
    public interface IOrpConnection
    {
        /// <summary>
        /// Remote Endpoint.
        /// </summary>
        EndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Local Endpoint.
        /// </summary>
        EndPoint LocalEndPoint { get; }

        /// <summary>
        /// Emit the <paramref name="Message"/> asynchronously.
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
        Task<OrpMessage> EmitAsync(object Message);

        /// <summary>
        /// Notify the <paramref name="Message"/> asynchronously.
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
        Task NotifyAsync(object Message);

        /// <summary>
        /// Close the connection immediately.
        /// </summary>
        void Close();
    }

    public interface IOrpConnectionAccessor
    {
        /// <summary>
        /// Connection t
        /// </summary>
        Task<IOrpConnection> Connection { get; }
    }
}
