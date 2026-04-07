using System;
using System.Collections.Generic;
using System.Linq;

namespace DirectPlay4;

/// <summary>
///  A filter criteria for a collection of DirectPlay sessions.
/// </summary>
readonly struct SessionFilter
{
    /// <summary>
    ///  A filter that excludes all sessions.
    /// </summary>
    public static readonly SessionFilter Empty = new(_ => false);

    /// <summary>
    ///  A filter that matches all sessions.
    /// </summary>
    public static readonly SessionFilter Default = new(_ => true);

    readonly Predicate<Session> predicate;
    readonly string display = "";

    /// <remarks>
    ///  To construct a filter, start with <see cref="Default"/>.
    /// </remarks>
    SessionFilter(Predicate<Session> predicate, string display = "")
    {
        this.predicate = predicate;
        this.display = display;
    }

    /// <summary>
    ///  Applies this filter to a collection of sessions.
    /// </summary>
    public IEnumerable<Session> Apply(IEnumerable<Session> sessions)
    {
        // copy because lambdas in structs cannot use 'this'
        Predicate<Session> predicate = this.predicate;
        return sessions.Where(session => predicate(session));
    }

    /// <summary>
    ///  Extends the current filter to match sessions for a specific application.
    /// </summary>
    public SessionFilter WithApplication(Guid application)
    {
        Predicate<Session> predicate = this.predicate;
        return new SessionFilter(
            session => session.Application == application && predicate(session),
            AppendDisplay(display, $"app: {application}")
        );
    }

    /// <summary>
    ///  Extends the current filter to match sessions that have space for additional players.
    /// </summary>
    public SessionFilter WithJoinableOnly()
    {
        Predicate<Session> predicate = this.predicate;
        return new SessionFilter(
            session => session.CurrentPlayers < session.MaxPlayers && predicate(session),
            AppendDisplay(display, "joinable")
        );
    }

    /// <summary>
    ///  Gets a human-readable display string for this filter.
    /// </summary>
    public override string ToString()
    {
        return string.IsNullOrEmpty(display) ? "none" : display;
    }

    static string AppendDisplay(string? display, string additionalDisplay)
    {
        if (string.IsNullOrWhiteSpace(display))
        {
            return additionalDisplay;
        }

        return $"{display} | {additionalDisplay}";
    }
}
