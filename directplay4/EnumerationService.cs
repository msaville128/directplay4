using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DirectPlay4;

/// <summary>
///  A persistent service for handling session enumeration requests from clients over UDP.
/// </summary>
class EnumerationService
    (
        ILogger<EnumerationService> logger,
        Channel<OutgoingMessage> output,
        ActiveSessions sessions
    )
    : BackgroundService
{
    const int BroadcastPort = 47624;

    /// <summary>
    ///  Continuously listens for DPSP_MSG_ENUMSESSIONS requests over UDP and responds with sessions
    ///  matching the request.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken cancellation)
    {
        logger.LogInformation("DirectPlay enumeration service started");

        // DPSP_MSG_ENUMSESSIONS messages are broadcast over UDP
        using Socket inbound = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        IPEndPoint localEndpoint = new(IPAddress.Any, BroadcastPort);
        inbound.Bind(localEndpoint);

        Memory<byte> buffer = new byte[0x10000]; // max datagram size
        while (!cancellation.IsCancellationRequested)
        {
            var result = await inbound.ReceiveFromAsync(buffer, localEndpoint, cancellation);
            IncomingMessage message = IncomingMessage.Create(buffer.Span[..result.ReceivedBytes]);

            bool isValidMessage =
                message.IsValid &&
                message.Header.CommandId == DPSP_MSG_ENUMSESSIONS.CommandId &&
                message.HasPayloadSizeFor<DPSP_MSG_ENUMSESSIONS>();

            if (!isValidMessage)
            {
                logger.LogDebug("Discarded invalid message from {remote}", result.RemoteEndPoint);
                continue;
            }

            SessionFilter filter = CreateFilter(message.WithPayload<DPSP_MSG_ENUMSESSIONS>().Payload);
            logger.LogInformation("Received from {remote} ({filter})", result.RemoteEndPoint, filter);

            OutgoingMessage response = OutgoingMessage.To((IPEndPoint)result.RemoteEndPoint);
            foreach (Session session in filter.Apply(sessions))
            {
                DPSP_MSG_ENUMSESSIONSREPLY reply = new();
                unsafe { reply.SessionDesc.StructSize = sizeof(DPSESSIONDESC2); }

                reply.SessionDesc.Application = session.Application;
                reply.SessionDesc.CurrentPlayers = session.CurrentPlayers;
                reply.SessionDesc.MaxPlayers = session.MaxPlayers;

                response.Enqueue(reply, writer =>
                {
                    reply.NameOffset = (int)writer.BaseStream.Position;
                    writer.Write(Encoding.Unicode.GetBytes(session.Name + '\0'));
                });
            }

            // if the outgoing message queue is full, then just drop the message
            _ = output.Writer.TryWrite(response);
        }

        logger.LogInformation("DirectPlay enumeration service stopped");
    }

    /// <summary>
    ///  Creates a session filter for the incoming request.
    /// </summary>
    SessionFilter CreateFilter(DPSP_MSG_ENUMSESSIONS request)
    {
        SessionFilter filter = SessionFilter.Default
            .WithApplication(request.Application);

        // password protected sessions are unsupported
        if (request.PasswordOffset is not 0)
        {
            return SessionFilter.Empty;
        }

        // the ALL flag isn't checked because it's presumed to be the default
        if (request.Flags.HasFlag(DPSP_MSG_ENUMSESSIONS.FLAGS.AVAILABLE))
        {
            filter = filter.WithJoinableOnly();
        }

        return filter;
    }
}
