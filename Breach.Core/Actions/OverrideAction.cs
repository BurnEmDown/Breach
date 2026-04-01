namespace Breach.Core.Actions;

/// <summary>
/// Swap the tile under one of the current player's agents with a tile on their
/// player board. The agent's tile moves to the player board; the player-board
/// tile (if any) moves to the board under the agent.
/// </summary>
/// <param name="Player">The acting player.</param>
/// <param name="AgentIndex">0 or 1 — which agent's tile to swap out.</param>
/// <param name="PlayerBoardSlot">0–2 — slot index on the player board.</param>
public sealed record OverrideAction(PlayerId Player, int AgentIndex, int PlayerBoardSlot)
    : IGameAction;
