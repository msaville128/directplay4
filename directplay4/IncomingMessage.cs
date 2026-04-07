using System;
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
    ///  An indication of whether this message contains a DirectPlay envelope.
    /// </summary>
    public bool IsValid => Data.Length >= sizeof(DPSP_MSG_HEADER) && Header.HasValidMagic;

    /// <remarks>
    ///  Use <see cref="Create"/> to create an <see cref="IncomingMessage"/> instance.
    /// </remarks>
    IncomingMessage(ReadOnlySpan<byte> data)
    {
        Data = data;
        if (Data.Length >= sizeof(DPSP_MSG_HEADER))
        {
            Header = ref MemoryMarshal.AsRef<DPSP_MSG_HEADER>(Data[..sizeof(DPSP_MSG_HEADER)]);
        }
    }

    /// <summary>
    ///  Decodes the incoming packet as a DirectPlay message.
    /// </summary>
    public static IncomingMessage Create(ReadOnlySpan<byte> data)
    {
        return new IncomingMessage(data);
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
    public readonly ref readonly T GetPayload<T>() where T : unmanaged
    {
        if (!HasPayloadSizeFor<T>())
        {
            throw new InvalidOperationException(
                $"Message is too small to contain a payload of {typeof(T)}.");
        }

        ref readonly T payload = ref MemoryMarshal.AsRef<T>(Data[sizeof(DPSP_MSG_HEADER)..]);
        return ref payload;
    }
}
