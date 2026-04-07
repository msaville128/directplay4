using DirectPlay4;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

await new HostBuilder()
    .ConfigureLogging(logging => logging
        .AddFilter("Microsoft", LogLevel.Warning)
        .AddFilter("DirectPlay4", LogLevel.Debug)
        .AddConsole())
    .ConfigureServices((host, services) => services
        .AddDirectPlaySessions(
            new Session
            {
                Name = "My Game Server",
                Application = Guid.Parse("3E328398-284D-430C-9585-23665E9A26E5"),
                Endpoint = IPEndPoint.Parse("127.0.0.1:2300"),
                MaxPlayers = 100
            }))
    .RunConsoleAsync();
