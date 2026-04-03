using System;
using System.Net;
using System.Runtime.InteropServices;

namespace DirectPlay4;

/// <summary>
///  An incoming DirectPlay message from a client.
/// </summary>
unsafe readonly ref struct IncomingMessage
{
    /// <summary>
    ///  The DirectPlay message header.
    /// </summary>
    public readonly ref readonly DPSP_MSG_HEADER Header;

    /// <summary>
    ///  Raw message data.
    /// </summary>
    public readonly ReadOnlySpan<byte> Data;

    /// <summary>
    ///  The endpoint of the client that sent this message.
    /// </summary>
    public readonly IPEndPoint? RemoteEndpoint;

    /// <summary>
    ///  An indication of whether this message contains a DirectPlay envelope.
    /// </summary>
    public bool IsValid => !Data.IsEmpty && Header.HasValidMagic;

    /// <remarks>
    ///  Use <see cref="Create"/> to create an <see cref="IncomingMessage"/> instance.
    /// </remarks>
    IncomingMessage(IPEndPoint remoteEndpoint, ReadOnlySpan<byte> data)
    {
        RemoteEndpoint = remoteEndpoint;

        Data = data;
        if (Data.Length >= sizeof(DPSP_MSG_HEADER))
        {
            Header = ref MemoryMarshal.AsRef<DPSP_MSG_HEADER>(Data[..sizeof(DPSP_MSG_HEADER)]);
        }
    }

    /// <summary>
    ///  Decodes the incoming packet as a DirectPlay message.
    /// </summary>
    public static IncomingMessage Create(IPEndPoint remoteEndpoint, ReadOnlySpan<byte> data)
    {
        return new IncomingMessage(remoteEndpoint, data);
    }

    /// <summary>
    ///  Checks whether this message has enough space to contain <typeparamref name="T"/>.
    /// </summary>
    public bool HasPayloadSizeFor<T>() where T : unmanaged
    {
        // also checks the declared size too as an additional safety check
        return Data.Length >= sizeof(DPSP_MSG_HEADER) + sizeof(T) && Header.Size <= Data.Length;
    }

    /// <summary>
    ///  Decodes the payload as <typeparamref name="T"/>.
    /// </summary>
    public IncomingMessage<T> WithPayload<T>() where T : unmanaged
    {
        if (!HasPayloadSizeFor<T>())
        {
            throw new InvalidOperationException(
                $"Message is too small to contain a payload of {typeof(T)}.");
        }

        ref readonly T payload = ref MemoryMarshal.AsRef<T>(Data[sizeof(DPSP_MSG_HEADER)..]);
        return new IncomingMessage<T>(this, in payload);
    }
}

/// <summary>
///  An incoming DirectPlay message from a client containing a payload of <typeparamref name="T"/>.
/// </summary>
readonly ref struct IncomingMessage<T>(IncomingMessage @base, ref readonly T payload)
    where T : unmanaged
{
    /// <summary>
    ///  The underlying message without payload information.
    /// </summary>
    public readonly IncomingMessage Base = @base;

    /// <summary>
    ///  The message payload.
    /// </summary>
    public readonly ref readonly T Payload = ref payload;

    /// <summary>
    ///  Attempts to read a null-terminated UTF-16LE string at the specified offset in the message.
    ///  Returns <c>false</c> if the offset doesn't point to a valid location or string.
    /// </summary>
    public unsafe bool TryReadCString(int offset, out ReadOnlySpan<char> @string)
    {
        // check for invalid offset
        int minOffset = sizeof(DPSP_MSG_HEADER) + sizeof(T);
        int maxOffset = Base.Data.Length - sizeof(char); // null-terminator
        if (offset < minOffset || offset > maxOffset)
        {
            @string = "";
            return false;
        }

        // truncate to even length and then reinterpret as UTF-16LE
        ReadOnlySpan<byte> remaining = Base.Data[offset..(offset + ((Base.Data.Length - offset) & ~1))];
        ReadOnlySpan<char> chars = MemoryMarshal.Cast<byte, char>(remaining);

        // check for null terminator
        int delimiterIndex = chars.IndexOf('\0');
        if (delimiterIndex < 0)
        {
            @string = "";
            return false;
        }

        @string = chars[..delimiterIndex];
        return true;
    }
}
