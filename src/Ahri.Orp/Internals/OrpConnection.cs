using Ahri.Networks;
using Ahri.Networks.Tcp;
using Ahri.Networks.Utilities;
using Ahri.Orp.Internals.Accessors;
using Ahri.Orp.Internals.Managers;
using Ahri.Orp.Internals.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Ahri.Orp.Internals
{
    public class OrpConnection : TcpSession, IOrpConnection
    {
        private static readonly Task<OrpMessageState> ABORTED = Task.FromResult(OrpMessageState.Aborted);
        private static readonly Task<OrpMessageState> NOT_SUPPORTED = Task.FromResult(OrpMessageState.NotSupported);
        private static readonly Task<OrpMessageState> LOCAL_EMIT_ERROR = Task.FromResult(OrpMessageState.LocalEmitError);

        private OrpWaitHandleManager m_WaitHandles = new();

        private int m_State = 0;
        private int m_Length = 0;

        private PacketFragment m_LengthBytes;
        private PacketFragment m_PayloadBytes;

        private IOrpMappings m_Mappings;
        private Func<IOrpContext, Task> m_Endpoint;

        private Queue<(PacketFragment Packet, TaskCompletionSource Tcs)> m_EmitQueue = new();
        private Task m_TaskEmit;

        private IEnumerable<Func<IOrpConnection, Task>> m_Greetings;
        private IEnumerable<Func<IOrpConnection, Task>> m_Farewells;

        /// <summary>
        /// Set Mappings for the connection.
        /// </summary>
        /// <param name="Mappings"></param>
        public void SetMappings(IOrpMappings Mappings) => m_Mappings = Mappings;

        /// <summary>
        /// Set Endpoint for the connection.
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

        /// <inheritdoc/>
        protected override async Task OnCreateAsync()
        {
            if (m_Greetings != null)
            {
                foreach (var Each in m_Greetings)
                    await Each(this);
            }
        }

        /// <inheritdoc/>
        protected override async Task OnDestroyAsync(SocketError Error)
        {
            m_WaitHandles.Dispose();

            if (m_Farewells != null)
            {
                foreach (var Each in m_Farewells)
                    await Each(this);
            }
        }

        /// <inheritdoc/>
        protected override async Task OnReceiveAsync(PacketFragment Fragment)
        {
            while(true)
            {
                var State = m_State;
                var Again = false;

                switch(State)
                {
                    case 0: /* Receiving Length bytes. */
                        {
                            var Remains = sizeof(ushort) - m_LengthBytes.Length;
                            if (Remains <= 0)
                            {
                                if (!BitConverter.IsLittleEndian)
                                     Array.Reverse(m_LengthBytes.Bytes);

                                m_Length = BitConverter.ToUInt16(m_LengthBytes.Bytes);
                                m_State++;
                                break;
                            }

                            m_LengthBytes = PacketFragment.Join(m_LengthBytes, Fragment.Take(Remains));
                            Fragment = Fragment.Skip(Remains); Again = Fragment.Length > 0;
                        }
                        break;

                    case 1: /* Receiving Payload bytes.*/
                        if (m_Length <= m_PayloadBytes.Length)
                        {
                            await OnPacketReceived(m_PayloadBytes);

                            m_LengthBytes = PacketFragment.Empty;
                            m_PayloadBytes = PacketFragment.Empty;
                            m_State = 0;
                            break;
                        }

                        m_PayloadBytes = PacketFragment.Join(m_PayloadBytes, Fragment.Take(m_Length));
                        Fragment = Fragment.Skip(m_Length); Again = Fragment.Length > 0 || (m_Length <= m_PayloadBytes.Length);
                        break;

                }

                if (m_State != State || Again)
                    continue;

                

                break;
            }
        }

        /// <summary>
        /// Called when Packet received.
        /// </summary>
        /// <param name="Payload"></param>
        /// <returns></returns>
        private Task OnPacketReceived(PacketFragment Payload)
        {
            var  Packet = new FixedBinaryReader(Payload);
            if (!Packet.TryReadByte(out var Opcode))
                return Task.CompletedTask;

            switch(Opcode)
            {
                case 0x01: /* Message. */
                case 0x02: /* Notification. */
                    {
                        var Context = Deserialize(Packet);
                        if (Context != null)
                        {
                            Context.IsNotification = (Opcode == 0x02);

                            if (Context.State != OrpMessageState.Success)
                            {
                                if (Context.IsNotification)
                                    break;

                                return ReplyTo(Context.MessageId, null, Context.State);
                            }

                            else
                            {
                                _ = HandleAsync(Context);
                            }
                        }
                    }
                    break;

                case 0x10: /* Message Reply.*/
                    {
                        if (DeserializeReply(Packet, out var Message, out var Guid))
                        {
                            m_WaitHandles.SetCompletionToWaitHandle(Guid, Message);
                        }
                    }
                    break;
            }

            return Task.CompletedTask;
        }

        private async Task HandleAsync(IOrpContext Context)
        {
            using var Scope = Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

            if (!(Context is OrpContext Orp))
                return;

            Orp.Services = Scope.ServiceProvider;

            if (Orp.Services.GetService<IOrpContextAccessor>() is OrpContextAccessor Accessor)
                Accessor.Instance = Context;

            await m_Endpoint(Context);
            await Orp.TryReplyError();
        }

        /// <summary>
        /// Deserialize the received packet to <see cref="OrpContext"/>.
        /// </summary>
        /// <param name="Packet"></param>
        /// <returns></returns>
        private OrpContext Deserialize(FixedBinaryReader Packet)
        {
            var New = new OrpContext();
            var Bytes = new byte[16];

            if (Packet.Read(Bytes, 0, 16) != 16 ||
               !Packet.TryReadUInt32(out var Hash))
                return null;

            New.Connection = this;
            New.MessageId = new Guid(Bytes);

            var Type = m_Mappings.FromHash(Hash);
            if (Type is null)
                New.State = OrpMessageState.NotImplemented;

            else
            {
                try
                {
                    using (var Reader = new BsonDataReader(new MemoryStream(
                        Packet.Array, Packet.Index, Packet.Length, false)))
                    {
                        New.State = OrpMessageState.Success;
                        New.Message = (new JsonSerializer()).Deserialize(Reader, Type);
                    }
                }
                catch { New.State = OrpMessageState.RemoteError; }
            }

            return New;
        }
        
        /// <summary>
        /// Deserialize the reply message.
        /// </summary>
        /// <param name="Packet"></param>
        /// <returns></returns>
        private bool DeserializeReply(FixedBinaryReader Packet, out OrpMessage Message, out Guid Guid)
        {
            var Bytes = new byte[16];

            if (Packet.Read(Bytes, 0, 16) != 16 ||
               !Packet.TryReadUInt32(out var Hash) ||
               !Packet.TryReadByte(out var Status))
            {
                Guid = Guid.Empty;
                Message = default;
                return false;
            }

            var Type = m_Mappings.FromHash(Hash);
            var Reply = null as object;

            var State = (OrpMessageState) Status;
            if (State == OrpMessageState.Success)
            {
                if (Type is null)
                    State = OrpMessageState.NotImplemented;

                else
                {
                    try
                    {
                        using (var Reader = new BsonDataReader(new MemoryStream(
                            Packet.Array, Packet.Index, Packet.Length, false)))
                        {
                            Reply = (new JsonSerializer()).Deserialize(Reader, Type);
                        }
                    }
                    catch { State = OrpMessageState.LocalError; }
                }
            }

            Guid = new Guid(Bytes);
            Message = new OrpMessage(State, Reply);
            return true;
        }

        /// <inheritdoc/>
        public void Close() => CloseAsync().GetAwaiter().GetResult();

        /// <inheritdoc/>
        public async Task<OrpMessage> EmitAsync(object Message)
        {
            var Guid = m_WaitHandles.Reserve();
            var Handle = m_WaitHandles.GetWaitHandle(Guid);

            var State = await TransmitTo(false, Guid, Message);
            if (State != OrpMessageState.Success)
            {
                var Temp = new OrpMessage(State, null);
                m_WaitHandles.SetCompletionToWaitHandle(Guid, Temp);
                return Temp;
            }

            return await Handle;
        }

        /// <inheritdoc/>
        public Task NotifyAsync(object Message) => TransmitTo(true, Guid.Empty, Message);

        /// <summary>
        /// Transmit the message to remote host.
        /// </summary>
        /// <param name="IsNotify"></param>
        /// <param name="MessageId"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        private Task<OrpMessageState> TransmitTo(bool IsNotify, Guid MessageId, object Message)
        {
            if (Completion.IsCompleted)
                return ABORTED;

            var Hash = Message != null
                ? m_Mappings.FromType(Message.GetType())
                : null;

            if (!Hash.HasValue)
                return NOT_SUPPORTED;

            var Guid = MessageId.ToByteArray();
            var HashBytes = BitConverter.GetBytes(Hash.HasValue ? Hash.Value : uint.MinValue);

            PacketFragment Packet = new byte[1] { IsNotify ? (byte)0x02 : (byte)0x01 }.Concat(Guid.Concat(HashBytes)).ToArray();
            if (Message != null)
            {
                try
                {
                    using var Stream = new MemoryStream();
                    using (var Writer = new BsonDataWriter(Stream))
                        (new JsonSerializer()).Serialize(Writer, Message);

                    Packet = PacketFragment.Join(Packet, Stream.ToArray());
                }

                catch { return LOCAL_EMIT_ERROR; }
            }

            Packet = PacketFragment.Join((PacketFragment) BitConverter.GetBytes((ushort)Packet.Length), Packet);
            return EmitPacketAsync(Packet).ContinueWith(X =>
            {
                if (X.IsCompletedSuccessfully)
                    return OrpMessageState.Success;

                return OrpMessageState.Aborted;
            });
        }

        /// <inheritdoc/>
        internal Task ReplyTo(Guid MessageId, object MessageReply, OrpMessageState MessageState)
        {
            if (Completion.IsCompleted)
                return Task.CompletedTask;

            var Guid = MessageId.ToByteArray();
            var Hash = MessageReply != null
                ? m_Mappings.FromType(MessageReply.GetType())
                : null;

            var HashBytes = BitConverter.GetBytes(Hash.HasValue ? Hash.Value : uint.MinValue);
            var State = (byte)(Hash.HasValue
                ? MessageState : OrpMessageState.NotImplemented);

            PacketFragment Packet = new byte[1] { 0x10 }.Concat(Guid.Concat(HashBytes).Append(State)).ToArray();
            if (MessageReply != null && State == (byte)OrpMessageState.Success)
            {
                try
                {
                    using var Stream = new MemoryStream();
                    using (var Writer = new BsonDataWriter(Stream))
                        (new JsonSerializer()).Serialize(Writer, MessageReply);

                    Packet = PacketFragment.Join(Packet, Stream.ToArray());
                }

                catch { Packet.Bytes[1 + 16 + sizeof(uint)] = (byte)OrpMessageState.RemoteError; }
            }

            Packet = PacketFragment.Join((PacketFragment)BitConverter.GetBytes((ushort)Packet.Length), Packet);
            return EmitPacketAsync(Packet);
        }

        /// <summary>
        /// Emit the <see cref="PacketFragment"/> to remote host.
        /// </summary>
        /// <param name="Packet"></param>
        /// <returns></returns>
        private async Task EmitPacketAsync(PacketFragment Packet)
        {
            var Tcs = new TaskCompletionSource();

            lock (this)
            {
                if (Completion.IsCompleted)
                    Tcs.SetResult();

                else
                {
                    m_EmitQueue.Enqueue((Packet, Tcs));

                    if (m_TaskEmit is null || m_TaskEmit.IsCompleted)
                        m_TaskEmit = EmitQueuedPackets();
                }
            }

            await Tcs.Task;
        }

        private async Task EmitQueuedPackets()
        {
            while(true)
            {
                PacketFragment Packet;
                TaskCompletionSource Tcs;

                lock(this)
                {
                    if (!m_EmitQueue.TryDequeue(out var Temp))
                    {
                        m_TaskEmit = null;
                        break;
                    }

                    Packet = Temp.Packet;
                    Tcs = Temp.Tcs;
                }

                if (!Completion.IsCompleted)
                    await SendAsync(Packet);

                Tcs?.TrySetResult();
            }
        }
    }
}
