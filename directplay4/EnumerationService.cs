using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DirectPlay4;

/// <summary>
///  A persistent service for handling session enumeration requests from clients.
/// </summary>
class EnumerationService(ILogger<EnumerationService> logger) : BackgroundService
{
    const int BroadcastPort = 47624;
    const int BufferSize = 0x10000; // max datagram size

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

            logger.LogInformation("{Bytes}", Convert.ToHexString(buffer.Span[..result.ReceivedBytes]));
            // TODO: Process packet
        }

        logger.LogInformation("DirectPlay enumeration service stopped.");
    }
}
