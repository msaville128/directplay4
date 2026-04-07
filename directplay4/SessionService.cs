using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DirectPlay4;

/// <summary>
///  A persistent service for handling DirectPlay session requests from clients.
/// </summary>
/// <remarks>
///  Sessions may be registered directly with the service collection to be seeded at startup.
/// </remarks>
class SessionService
    (
        ILogger<SessionService> logger,
        ServerInfo serverInfo,
        IEnumerable<Session> seedSessions,
        ActiveSessions sessions
    )
    : BackgroundService
{
    /// <summary>
    ///  Starts the DirectPlay session service.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken cancellation)
    {
        logger.LogInformation("DirectPlay session service started");

        foreach (Session seed in seedSessions)
        {
            sessions.AddOrUpdate(seed);
        }

        using TcpListener listener = new(serverInfo.ServiceEndpoint);
        listener.Start();

        while (!cancellation.IsCancellationRequested)
        {
            // TEMPORARY CODE =================================================
            // TODO: wrap in try/catch SocketException
            logger.LogDebug("Waiting for connection...");
            using TcpClient client = await listener.AcceptTcpClientAsync(cancellation);
            logger.LogDebug("New connection!");
            using NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[4096];
            while (client.Connected)
            {
                int bytes = await stream.ReadAsync(buffer, cancellation);
                logger.LogDebug("Received {bytes} bytes from client {client}\n{data}",
                    bytes, client.Client.RemoteEndPoint, Convert.ToHexString(buffer[..bytes]));
            }

            logger.LogDebug("Client disconnected!");
            // ================================================================
        }

        logger.LogInformation("DirectPlay session service stopped");
    }
}
