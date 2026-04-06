using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DirectPlay4;

/// <summary>
///  A persistent service that sends outgoing messages over TCP/IP to remote clients.
/// </summary>
class OutboundService
    (
        ILogger<OutboundService> logger,
        Channel<OutgoingMessage> output
    )
    : BackgroundService
{
    /// <summary>
    ///  Stands by for outgoing messages and then sends them.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken cancellation)
    {
        logger.LogInformation("Outbound message service started");

        await foreach (OutgoingMessage message in output.Reader.ReadAllAsync(cancellation))
        {
            try
            {
                await message.SendAsync(cancellation);
            }
            catch (SocketException exception)
            {
                logger.LogDebug(exception,
                    "Exception thrown when sending message to {remote}", message.Destination);
            }
        }

        logger.LogInformation("Outbound message service stopped");
    }
}
