using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DirectPlay4;

/// <summary>
///  A thread-safe container of active DirectPlay sessions.
/// </summary>
class ActiveSessions : IEnumerable<Session>
{
    readonly ConcurrentBag<Session> sessions = [];

    // TODO: Add | Remove | Update

    // IEnumerable<Session>
    IEnumerator<Session> IEnumerable<Session>.GetEnumerator() => sessions.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => sessions.GetEnumerator();
}
