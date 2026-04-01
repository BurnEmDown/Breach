namespace Breach.Core;

public sealed class Player
{
    public PlayerId Id { get; }

    /// <summary>Index 0 and 1 — both agents belonging to this player.</summary>
    public Agent[] Agents { get; }

    public PlayerBoard Board { get; }

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

    public Player Clone() =>
        new(Id, [Agents[0].Clone(), Agents[1].Clone()], Board.Clone());
}
