namespace Breach.Core.Actions;

/// <summary>
/// Swaps the tile under one of the current player's agents with a tile on their
/// player board. The agent's board tile (if any) takes the place of the player-board tile,
/// and the player-board tile goes to the main board under the agent.
/// Costs 1 AP. Essential for retrieving tiles held on the player board and cycling new tiles in.
/// </summary>
/// <param name="Player">The player performing the override.</param>
/// <param name="AgentIndex">Which agent's tile to swap (0 or 1).</param>
/// <param name="PlayerBoardSlot">Which player board slot to swap with (0–2).</param>
public sealed record OverrideAction(PlayerId Player, int AgentIndex, int PlayerBoardSlot)
    : IGameAction;
