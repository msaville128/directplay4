using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace DirectPlay4;

/// <summary>
///  A batch of DirectPlay commands to be sent to a remote endpoint.
/// </summary>
class OutgoingMessage
{
    readonly List<byte[]> batch = [];

    /// <remarks>
    ///  Use <see cref="To"/> to create an <see cref="OutgoingMessage"/> instance.
    /// </remarks>
    OutgoingMessage(IPEndPoint destination)
    {
        Destination = destination;
    }

    /// <summary>
    ///  The intended recipient of this message.
    /// </summary>
    public IPEndPoint Destination { get; }

    /// <summary>
    ///  Begins constructing a batch of DirectPlay commands to send to a remote endpoint.
    /// </summary>
    public static OutgoingMessage To(IPEndPoint destination)
    {
        return new OutgoingMessage(destination);
    }

    /// <summary>
    ///  Adds a DirectPlay command without variable-length data to the batch.
    /// </summary>
    public OutgoingMessage Enqueue<T>(T command)
        where T : unmanaged, ICommand<T>
    {
        return Enqueue(command, _ => { });
    }

    /// <summary>
    ///  Adds a DirectPlay command with variable-length data to the batch.
    /// </summary>
    public unsafe OutgoingMessage Enqueue<T>(T command, Action<BinaryWriter> writeVariableData)
        where T : unmanaged, ICommand<T>
    {
        int fixedLength = sizeof(DPSP_MSG_HEADER) + sizeof(T);

        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);

        void write<TStruct>(ref TStruct value) where TStruct : unmanaged
        {
            Span<TStruct> span = MemoryMarshal.CreateSpan(ref value, 1);
            writer.Write(MemoryMarshal.Cast<TStruct, byte>(span));
        }

        stream.SetLength(fixedLength);
        stream.Position = fixedLength;
        writeVariableData(writer);

        DPSP_MSG_HEADER header = new()
        {
            CommandId = T.CommandId,
            Version = 14,
            Size = (int)stream.Length,
            Token = 0xBAB
        };

        header.Magic[0] = (byte)'p';
        header.Magic[1] = (byte)'l';
        header.Magic[2] = (byte)'a';
        header.Magic[3] = (byte)'y';

        header.SockAddr = new()
        {
            Family = (short)AddressFamily.InterNetwork,
            Port = (ushort)IPAddress.HostToNetworkOrder((short)Destination.Port)
        };

        byte[] address = Destination.Address.GetAddressBytes();
        header.SockAddr.Address[0] = address[0];
        header.SockAddr.Address[1] = address[1];
        header.SockAddr.Address[2] = address[2];
        header.SockAddr.Address[3] = address[3];

        stream.Position = 0;
        write(ref header);
        write(ref command);

        batch.Add(stream.ToArray());
        return this;
    }

    /// <summary>
    ///  Establishes a TCP/IP connection to the destination and then sends each command in this
    ///  batch sequentially. Socket exceptions are unhandled.
    /// </summary>
    public async Task SendAsync(CancellationToken cancellation)
    {
        using Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(Destination);

        foreach (ReadOnlyMemory<byte> command in batch)
        {
            await socket.SendAsync(command, cancellation);
        }
    }
}
