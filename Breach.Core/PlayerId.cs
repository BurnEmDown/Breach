namespace Breach.Core;

/// <summary>
/// Unique identifier for each of the two players in a Breach game.
/// Player One moves first with a reduced AP budget on their initial turn;
/// Player Two always starts with the standard 2 AP per turn.
/// </summary>
public enum PlayerId
{
    /// <summary>First player — makes the first move with only 1 AP.</summary>
    One,
    
    /// <summary>Second player — responds with 2 AP per turn.</summary>
    Two
}
