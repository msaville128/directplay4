using System;
using System.IO;
using System.Net;

namespace DirectPlay4;

/// <summary>
///  A batch of DirectPlay commands to be sent to a remote endpoint.
/// </summary>
class OutgoingMessage
{
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
        return Enqueue<T>(command, (_, _) => { });
    }

    /// <summary>
    ///  Adds a DirectPlay command with variable-length data to the batch.
    /// </summary>
    public OutgoingMessage Enqueue<T>(T command, Action<T, BinaryWriter> additionalData)
        where T : unmanaged, ICommand<T>
    {
        // TODO
        return this;
    }

    /// <summary>
    ///  Invokes the provided action with a DirectPlay message for each command in the batch.
    /// </summary>
    public void Serialize(Action<ReadOnlySpan<byte>> serialize)
    {
        // TODO
    }
}
