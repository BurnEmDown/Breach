namespace Breach.Core;

/// <summary>
/// Represents one of the two players in a Breach game. Each player owns 2 agents
/// and a personal player board. Actions are performed on behalf of the current
/// player (determined by <see cref="GameState.CurrentPlayerIndex"/>).
/// </summary>
public sealed class Player
{
    /// <summary>Unique identifier for this player (One or Two).</summary>
    public PlayerId Id { get; }

    /// <summary>
    /// This player's two agents. Index 0 and 1 allow independent control.
    /// Agents can be at different board positions and carry different tiles.
    /// </summary>
    public Agent[] Agents { get; }

    /// <summary>This player's personal board with 3 tile slots.</summary>
    public PlayerBoard Board { get; }

    /// <summary>
    /// Initializes a new player with two agents and an empty player board.
    /// </summary>
    /// <param name="id">The player's unique identifier.</param>
    /// <param name="agent0">The first agent.</param>
    /// <param name="agent1">The second agent.</param>
    public Player(PlayerId id, Agent agent0, Agent agent1)
    {
        Id     = id;
        Agents = [agent0, agent1];
        Board  = new PlayerBoard();
    }

    private Player(PlayerId id, Agent[] agents, PlayerBoard board)
    {
        Id     = id;
        Agents = agents;
        Board  = board;
    }

    /// <summary>
    /// Creates a deep copy of this player, including both agents and the player board.
    /// This is typically used when cloning the entire game state.
    /// </summary>
    public Player Clone() =>
        new(Id, [Agents[0].Clone(), Agents[1].Clone()], Board.Clone());
}
