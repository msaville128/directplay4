using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DirectPlay4;

/// <summary>
///  A persistent service for handling DirectPlay session enumeration requests from clients.
/// </summary>
class EnumerationService
    (
        ILogger<EnumerationService> logger,
        IEnumerable<Session> sessions
    )
    : BackgroundService
{
    static readonly IPEndPoint BroadcastEndpoint = new(IPAddress.Any, 47624);

    /// <summary>
    ///  Starts the DirectPlay session enumeration service.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken cancellation)
    {
        logger.LogInformation("DirectPlay enumeration service started");

        // DPSP_MSG_ENUMSESSIONS messages are broadcast over UDP
        using Socket inbound = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        inbound.Bind(BroadcastEndpoint);

        Memory<byte> buffer = new byte[0x10000]; // max datagram size
        while (!cancellation.IsCancellationRequested)
        {
            var result = await inbound.ReceiveFromAsync(buffer, BroadcastEndpoint, cancellation);
            var message = IncomingMessage.Create(buffer.Span[..result.ReceivedBytes]);

            bool isValidMessage =
                message.IsValid &&
                message.Header.CommandId == DPSP_MSG_ENUMSESSIONS.CommandId &&
                message.HasPayloadSizeFor<DPSP_MSG_ENUMSESSIONS>();

            if (!isValidMessage)
            {
                logger.LogDebug("Discarded invalid message from {remote}", result.RemoteEndPoint);
                continue;
            }

            SessionFilter filter = CreateFilter(in message.GetPayload<DPSP_MSG_ENUMSESSIONS>());
            logger.LogDebug("Received from {remote} ({filter})", result.RemoteEndPoint, filter);

            // send to the IP address and port provided in the incoming message's header
            IPEndPoint destination = new
            (
                address: ((IPEndPoint)result.RemoteEndPoint).Address,
                port: (ushort)IPAddress.NetworkToHostOrder((short)message.Header.SockAddr.Port)
            );

            try
            {
                using Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync(destination, cancellation);

                foreach (Session session in filter.Apply(sessions))
                {
                    DPSP_MSG_ENUMSESSIONSREPLY reply = new();
                    unsafe
                    {
                        reply.SessionDesc.StructSize = sizeof(DPSESSIONDESC2);
                        reply.SessionDesc.Reserved[0] = 1 << 16;
                    }
                    reply.SessionDesc.Flags = DPSESSIONDESC2.FLAGS.CLIENT_SERVER;

                    reply.SessionDesc.Application = session.Application;
                    reply.SessionDesc.CurrentPlayers = session.CurrentPlayers;
                    reply.SessionDesc.MaxPlayers = session.MaxPlayers;
                    reply.SessionDesc.Instance = session.SessionId;

                    var response = OutgoingMessage.Create(session.Endpoint, ref reply,
                        (ref DPSP_MSG_ENUMSESSIONSREPLY reply, int offset, BinaryWriter writer) =>
                        {
                            reply.NameOffset = offset;
                            writer.Write(Encoding.Unicode.GetBytes(session.Name + '\0'));
                        });

                    await socket.SendAsync(response.Data, cancellation);
                    logger.LogDebug("Sent '{session}' to {destination}", session.Name, destination);
                }
            }
            catch (SocketException ex)
            {
                // SocketException isn't exceptional in this context, so log only when debugging
                logger.LogDebug(ex, "Exception thrown when sending sessions to {destination}", destination);
            }
            catch (OperationCanceledException)
            {
                // server is shutting down
            }
        }

        logger.LogInformation("DirectPlay enumeration service stopped");
    }

    /// <summary>
    ///  Creates a session filter for the incoming request.
    /// </summary>
    SessionFilter CreateFilter(in DPSP_MSG_ENUMSESSIONS request)
    {
        SessionFilter filter = SessionFilter.Default
            .WithApplication(request.Application);

        // password protected sessions are unsupported
        if (request.PasswordOffset is not 0)
        {
            return SessionFilter.Empty;
        }

        // the ALL flag isn't checked here because it's presumed to be the default
        if (request.Flags.HasFlag(DPSP_MSG_ENUMSESSIONS.FLAGS.AVAILABLE))
        {
            filter = filter.WithJoinableOnly();
        }

        return filter;
    }
}
