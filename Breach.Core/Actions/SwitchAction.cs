namespace Breach.Core.Actions;

/// <summary>
/// Swap the tiles currently under the current player's two agents.
/// </summary>
/// <param name="Player">The acting player.</param>
public sealed record SwitchAction(PlayerId Player) : IGameAction;
