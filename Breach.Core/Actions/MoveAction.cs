namespace Breach.Core.Actions;

/// <summary>
/// Moves one of the current player's agents to an orthogonally adjacent tile.
/// Costs 1 AP normally, or 2 AP if the target tile is occupied by a rival agent (surcharge).
/// The agent can move up, down, left, or right, but not diagonally.
/// </summary>
/// <param name="Player">The player performing the move.</param>
/// <param name="AgentIndex">Which agent to move (0 or 1).</param>
/// <param name="Target">The orthogonally adjacent destination position.</param>
public sealed record MoveAction(PlayerId Player, int AgentIndex, Position Target)
    : IGameAction;
