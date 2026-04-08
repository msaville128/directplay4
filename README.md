# DirectPlay4.NET
DirectPlay 4 is an obsolete multiplayer protocol that was released in 1998 and used until the mid-2000s. This .NET library exists to support the preservation and restoration of PC games from this time period.

## Features

### Session Enumeration Server
The enumeration server listens for requests over UDP and responds with a configured list of sessions, allowing games to connect to remote hosts.

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

### Message Encoder/Decoder
If you need to implement other parts of the protocol, the `IncomingMessage` and `OutgoingMessage` structs make it easy to encode and decode DirectPlay messages.

```csharp
// decode
var incoming = IncomingMessage.Create(buffer);
var command = incoming.GetPayload<DPSP_MSG_REQUESTPLAYERID>();

// encode
var reply = new DPSP_MSG_REQUESTPLAYERREPLY();
var outgoing = OutgoingMessage.Create(IPEndPoint.Parse("127.0.0.1:2300"), ref reply);

// encode with variable-length data
var replyWithVariableData = new DPSP_MSG_ENUMSESSIONSREPLY();
var outgoing = OutgoingMessage.Create(IPEndPoint.Parse("127.0.0.1:2300"), ref reply, WriteVariableData);

void WriteVariableData(ref DPSP_MSG_ENUMSESSIONSREPLY reply, int offset, BinaryWriter writer)
{
    reply.NameOffset = offset;
    writer.Write(Encoding.Unicode.GetBytes(session.Name + '\0'));
}
```
