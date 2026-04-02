using DirectPlay4;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

await new HostBuilder()
    .ConfigureLogging(logging => logging
        .AddFilter("Microsoft", LogLevel.Warning)
        .AddConsole())
    .ConfigureServices((host, services) => services
        .AddDirectPlay4())
    .RunConsoleAsync();
