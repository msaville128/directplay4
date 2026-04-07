using System;

namespace DirectPlay4;

/// <summary>
///  An immutable DirectPlay session.
/// </summary>
public record Session
{
    /// <summary>
    ///  The name of this session.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///  The application associated with this session.
    /// </summary>
    public required Guid Application { get; init; }

    /// <summary>
    ///  Unique id for this session.
    /// </summary>
    public Guid SessionId { get; init; } = Guid.NewGuid();

    /// <summary>
    ///  Current number of players in this session.
    /// </summary>
    public int CurrentPlayers { get; init; }

    /// <summary>
    ///  Maximum number of players allowed in this session.
    /// </summary>
    public int MaxPlayers { get; init; }
}
