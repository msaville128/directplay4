using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace DirectPlay4;

/// <summary>
///  A thread-safe container of active DirectPlay sessions.
/// </summary>
/// <remarks>
///  Sessions may be registered directly with the service collection to be seeded at startup.
/// </remarks>
class ActiveSessions : IEnumerable<Session>
{
    readonly ILogger logger;
    readonly ConcurrentBag<Session> sessions;

    public ActiveSessions(ILogger<ActiveSessions> logger, IEnumerable<Session> seedSessions)
    {
        this.logger = logger;

        sessions = [.. seedSessions];
        foreach (Session seed in seedSessions)
        {
            LogSessionInfo(seed);
        }
    }

    // TODO: Add | Remove | Update

    void LogSessionInfo(Session session)
    {
        logger.LogInformation(
            "Added session '{name}' (app: {app} | max: {max})",
            session.Name,
            session.Application,
            session.MaxPlayers
        );
    }

    // IEnumerable<Session>
    IEnumerator<Session> IEnumerable<Session>.GetEnumerator() => sessions.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => sessions.GetEnumerator();
}
