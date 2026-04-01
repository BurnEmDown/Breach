namespace Breach.Core.Actions;

/// <summary>
/// Move one of the current player's agents to an orthogonally adjacent tile.
/// </summary>
/// <param name="Player">The acting player.</param>
/// <param name="AgentIndex">0 or 1 — which of the player's agents to move.</param>
/// <param name="Target">The destination position.</param>
public sealed record MoveAction(PlayerId Player, int AgentIndex, Position Target)
    : IGameAction;
