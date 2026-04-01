namespace Breach.Core;

/// <summary>
/// Represents the complete game state of a single Breach game. This is the source
/// of truth for the board, player information, and turn flow. The GameEngine mutates
/// this state as players take actions.
/// </summary>
public sealed class GameState
{
    /// <summary>The 3×3 main game board.</summary>
    public Board Board { get; }
    
    /// <summary>Both players in the game (index 0 = Player One, index 1 = Player Two).</summary>
    public Player[] Players { get; }

    /// <summary>
    /// Index into <see cref="Players"/> indicating whose turn it is.
    /// 0 = Player One's turn, 1 = Player Two's turn.
    /// </summary>
    public int CurrentPlayerIndex { get; set; }

    /// <summary>
    /// The number of action points the current player has remaining this turn
    /// (typically 1 or 2 depending on context).
    /// </summary>
    public int ActionPointsRemaining { get; set; }

    /// <summary>
    /// True if this is the very first turn of the game (Player One's initial turn).
    /// Player One gets only 1 AP on the first turn; everyone else gets 2 per turn.
    /// </summary>
    public bool IsFirstTurn { get; set; }

    /// <summary>Convenience property to access the current player without indexing.</summary>
    public Player CurrentPlayer => Players[CurrentPlayerIndex];
    
    /// <summary>Convenience property to access the opponent player.</summary>
    public Player OpponentPlayer => Players[CurrentPlayerIndex == 0 ? 1 : 0];

    /// <summary>
    /// Initializes a new game state.
    /// </summary>
    /// <param name="board">The main board (usually from <see cref="GameSetup.CreateInitialState()"/>).</param>
    /// <param name="player1">Player One (moves first).</param>
    /// <param name="player2">Player Two (responds after Player One's first turn).</param>
    /// <param name="isFirstTurn">If true, the current turn is the game's very first turn.</param>
    public GameState(Board board, Player player1, Player player2, bool isFirstTurn = true)
    {
        Board       = board;
        Players     = [player1, player2];
        CurrentPlayerIndex      = 0;
        ActionPointsRemaining   = isFirstTurn ? 1 : 2;
        IsFirstTurn             = isFirstTurn;
    }
}
