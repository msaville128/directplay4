using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace DirectPlay4;

/// <summary>
///  A DirectPlay message to be sent to a client.
/// </summary>
class OutgoingMessage
{
    public delegate void WriteMoreData<T>(ref T command, BinaryWriter writer);

    /// <remarks>
    ///  Use <see cref="Create"/> to create an <see cref="OutgoingMessage"/> instance.
    /// </remarks>
    OutgoingMessage(ReadOnlyMemory<byte> data)
    {
        Data = data;
    }

    /// <summary>
    ///  The data contained in this message.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; }

    /// <summary>
    ///  Creates a DirectPlay message to be sent to a client.
    /// </summary>
    public static unsafe OutgoingMessage Create<T>
        (IPEndPoint sessionEndpoint, ref T command, WriteMoreData<T>? writeMoreData = null)
        where T : unmanaged, ICommand<T>
    {
        int fixedLength = sizeof(DPSP_MSG_HEADER) + sizeof(T);

        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);

        void writeStruct<TStruct>(ref TStruct value) where TStruct : unmanaged
        {
            Span<TStruct> span = MemoryMarshal.CreateSpan(ref value, 1);
            writer.Write(MemoryMarshal.Cast<TStruct, byte>(span));
        }

        stream.SetLength(fixedLength);
        stream.Position = fixedLength;
        writeMoreData?.Invoke(ref command, writer);

        DPSP_MSG_HEADER header = new()
        {
            CommandId = T.CommandId,
            Version = 14,
            Size = (int)stream.Length,
            Token = 0xFAB
        };

        header.Magic[0] = (byte)'p';
        header.Magic[1] = (byte)'l';
        header.Magic[2] = (byte)'a';
        header.Magic[3] = (byte)'y';

        header.SockAddr = new()
        {
            Family = (short)AddressFamily.InterNetwork,
            Port = (ushort)IPAddress.HostToNetworkOrder((short)sessionEndpoint.Port)
        };

        byte[] address = sessionEndpoint.Address.GetAddressBytes();
        header.SockAddr.Address[0] = address[0];
        header.SockAddr.Address[1] = address[1];
        header.SockAddr.Address[2] = address[2];
        header.SockAddr.Address[3] = address[3];

        stream.Position = 0;
        writeStruct(ref header);
        writeStruct(ref command);

        return new OutgoingMessage(stream.ToArray());
    }
}
