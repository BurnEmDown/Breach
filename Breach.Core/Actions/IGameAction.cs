namespace Breach.Core.Actions;

/// <summary>
/// Base interface for all game actions. Implementations represent discrete moves
/// that a player can take on their turn: Move, Switch, or Override.
/// </summary>
public interface IGameAction
{
    /// <summary>The player performing this action.</summary>
    PlayerId Player { get; }
}
