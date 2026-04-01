namespace Breach.Core;

public sealed class GameState
{
    public Board Board { get; }
    public Player[] Players { get; }

    /// <summary>Index into <see cref="Players"/> for whose turn it is.</summary>
    public int CurrentPlayerIndex { get; set; }

    /// <summary>Action points remaining for the current turn.</summary>
    public int ActionPointsRemaining { get; set; }

    /// <summary>True until the first player has completed their first turn.</summary>
    public bool IsFirstTurn { get; set; }

    public Player CurrentPlayer => Players[CurrentPlayerIndex];
    public Player OpponentPlayer => Players[CurrentPlayerIndex == 0 ? 1 : 0];

    public GameState(Board board, Player player1, Player player2, bool isFirstTurn = true)
    {
        Board       = board;
        Players     = [player1, player2];
        CurrentPlayerIndex      = 0;
        ActionPointsRemaining   = isFirstTurn ? 1 : 2;
        IsFirstTurn             = isFirstTurn;
    }
}
