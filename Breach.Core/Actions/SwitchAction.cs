namespace Breach.Core.Actions;

/// <summary>
/// Swaps the tiles currently under the current player's two agents.
/// Costs 1 AP. This action is always valid and rearranges the tiles without
/// moving the agents themselves. Useful for repositioning tile sets on the board.
/// </summary>
/// <param name="Player">The player performing the switch.</param>
public sealed record SwitchAction(PlayerId Player) : IGameAction;
