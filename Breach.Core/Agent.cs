namespace Breach.Core;

/// <summary>
/// Represents a game agent (game token) belonging to one player. Each player has
/// exactly 2 agents placed at opposing corners of the board. Agents can move
/// orthogonally and carry tiles, which are swapped or overridden via actions.
/// </summary>
public sealed class Agent
{
    /// <summary>The player who owns and controls this agent.</summary>
    public PlayerId Owner { get; }
    
    /// <summary>The current position of this agent on the 3×3 board.</summary>
    public Position Position { get; set; }

    /// <summary>
    /// Initializes a new agent.
    /// </summary>
    /// <param name="owner">The player who owns this agent.</param>
    /// <param name="position">The starting position for this agent.</param>
    public Agent(PlayerId owner, Position position)
    {
        Owner    = owner;
        Position = position;
    }

    /// <summary>Creates a copy of this agent with the same owner and position.</summary>
    public Agent Clone() => new(Owner, Position);
}
