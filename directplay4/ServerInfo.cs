using System.Net;

namespace DirectPlay4;

public class ServerInfo
{
    public IPEndPoint ServiceEndpoint { get; init; } = new(IPAddress.Any, 2300);

    public IPEndPoint BroadcastEndpoint { get; init; } = new(IPAddress.Any, 47624);
}
