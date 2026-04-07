using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DirectPlay4;

/// <summary>
///  A thread-safe container of active DirectPlay sessions.
/// </summary>
class ActiveSessions(ILogger<ActiveSessions> logger) : IEnumerable<Session>
{
    readonly ConcurrentDictionary<Guid, Session> sessions = [];

    /// <summary>
    ///  Adds or updates a session.
    /// </summary>
    public void AddOrUpdate(Session session)
    {
        sessions.AddOrUpdate
        (
            session.SessionId,
            addValueFactory: _ =>
            {
                logger.LogInformation(
                    "Added session '{name}' (app: {app} | max: {max} | cur: {current})",
                    session.Name,
                    session.Application,
                    session.MaxPlayers,
                    session.CurrentPlayers
                );

                return session;
            },
            updateValueFactory: (_, _) =>
            {
                logger.LogInformation(
                    "Updated session '{name}' (app: {app} | max: {max} | cur: {current})",
                    session.Name,
                    session.Application,
                    session.MaxPlayers,
                    session.CurrentPlayers
                );

                return session;
            }
        );
    }

    /// <summary>
    ///  Removes a session. Does nothing if a session with the specified id doesn't exist.
    /// </summary>
    public void Remove(Guid sessionId)
    {
        if (sessions.TryRemove(sessionId, out Session? removedSession))
        {
            logger.LogInformation("Removed session '{name}'", removedSession.Name);
        }
    }

    // IEnumerable<Session>
    IEnumerator<Session> IEnumerable<Session>.GetEnumerator() => sessions.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => sessions.Values.GetEnumerator();
}
