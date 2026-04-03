using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DirectPlay4;

/// <summary>
///  A persistent service for handling session enumeration requests from clients over UDP.
/// </summary>
class EnumerationService(ILogger<EnumerationService> logger)
    : BackgroundService
{
    const int BroadcastPort = 47624;
    const int BufferSize = 0x10000; // max datagram size
    const int MaxPasswordLength = 16;

    /// <summary>
    ///  Continuously listens for DPSP_MSG_ENUMSESSIONS requests over UDP and responds back to the
    ///  request with sessions over both UDP and TCP.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken cancellation)
    {
        logger.LogInformation("DirectPlay enumeration service started.");

        IPEndPoint localEndpoint = new(IPAddress.Any, BroadcastPort);

        using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Bind(localEndpoint);

        Memory<byte> buffer = new byte[BufferSize];
        while (!cancellation.IsCancellationRequested)
        {
            var result = await socket.ReceiveFromAsync(buffer, localEndpoint, cancellation);
            var message = IncomingMessage.Create(
                (IPEndPoint)result.RemoteEndPoint,
                buffer.Span[..result.ReceivedBytes]
            );

            if (!message.IsValid)
            {
                LogRejection(message, "invalid message");
                continue;
            }

            HandleMessage(message);
        }

        logger.LogInformation("DirectPlay enumeration service stopped.");
    }

    /// <summary>
    ///  Handles an incoming DirectPlay message.
    /// </summary>
    void HandleMessage(IncomingMessage message)
    {
        if (message.Header.CommandId != DPSP_MSG_ENUMSESSIONS.CommandId)
        {
            LogRejection(message, "invalid command " + message.Header.CommandId);
            return;
        }

        if (!message.HasPayloadSizeFor<DPSP_MSG_ENUMSESSIONS>())
        {
            LogRejection(message, "payload too small");
            return;
        }

        EnumerateSessions(message.WithPayload<DPSP_MSG_ENUMSESSIONS>());
    }

    /// <summary>
    ///  Enumerates sessions matching the request.
    /// </summary>
    void EnumerateSessions(IncomingMessage<DPSP_MSG_ENUMSESSIONS> message)
    {
        DPSP_MSG_ENUMSESSIONS request = message.Payload;

        // TODO: Refactor this method to be called BuildSessionCriteria.
        // TODO: var criteria = SessionCriteria.Default.WithApplication()

        if (request.Flags.HasFlag(DPSP_MSG_ENUMSESSIONS.FLAGS.PASSWORD_REQUIRED))
        {
            if (request.PasswordOffset is 0)
            {
                LogRejection(message.Base, "PR flag set but no password provided");
                return;
            }

            if (!message.TryReadCString(request.PasswordOffset, out ReadOnlySpan<char> password))
            {
                LogRejection(message.Base, "invalid password offset");
                return;
            }

            if (password.Length > MaxPasswordLength)
            {
                LogRejection(message.Base, $"invalid password length ({password.Length} chars)");
                return;
            }

            // TODO: criteria = criteria.WithPassword(password);
        }

        if (request.Flags.HasFlag(DPSP_MSG_ENUMSESSIONS.FLAGS.AVAILABLE))
        {
            // TODO: criteria = criteria.WithJoinableOnly(true);
        }

        // TODO: return criteria;
    }

    void LogRejection(IncomingMessage message, string reason)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            string dataHexString = Convert.ToHexString(message.Data);
            logger.LogDebug("Rejected packet (reason: {reason})\n{data}", reason, dataHexString);
        }
    }
}
