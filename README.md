# DirectPlay4.NET
This is a [DirectPlay 4](https://learn.microsoft.com/en-us/openspecs/windows_protocols/mc-dpl4cs/10eeb2a2-da0e-4ce2-98dc-ba1a87092a68) game session enumeration server.

DirectPlay 4 is an obsolete networking API that was released in 1998 and used until the mid-2000s.

This server exists to support the preservation and restoration of PC games from this time period. It listens for requests over UDP and responds with a configured list of sessions, allowing games to connect to remote hosts.

## Standalone server
Coming soon

## .NET Generic Host / ASP.NET
Add the server to your host's service collection.

```csharp
services.AddDirectPlaySessions(
    new Session
    {
        Name = "My Game Server",
        Application = Guid.Parse("3E328398-284D-430C-9585-23665E9A26E5"),
        Endpoint = IPEndPoint.Parse("127.0.0.1:2300"),
        MaxPlayers = 100
    });
```
