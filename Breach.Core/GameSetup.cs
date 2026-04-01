namespace Breach.Core;

/// <summary>
/// Responsible for creating the standard starting configuration of a Breach game.
/// Use <see cref="CreateInitialState"/> to get a ready-to-play GameState.
/// </summary>
public static class GameSetup
{
    // -----------------------------------------------------------------------
    // Tile factories
    // -----------------------------------------------------------------------

    /// <summary>
    /// Creates an orange-primary tile with green and purple secondary colors.
    /// Orange is used for the opening diagonal.
    /// </summary>
    public static Tile OrangeTile() => new(TileColor.Orange, TileColor.Green,  TileColor.Purple);
    
    /// <summary>
    /// Creates a green-primary tile with orange and purple secondary colors.
    /// Green tiles are distributed 2 upper-right, 1 lower-left.
    /// </summary>
    public static Tile GreenTile()  => new(TileColor.Green,  TileColor.Orange, TileColor.Purple);
    
    /// <summary>
    /// Creates a purple-primary tile with orange and green secondary colors.
    /// Purple tiles are distributed 1 upper-right, 2 lower-left.
    /// </summary>
    public static Tile PurpleTile() => new(TileColor.Purple, TileColor.Orange, TileColor.Green);

    // -----------------------------------------------------------------------
    // Initial state
    // -----------------------------------------------------------------------

    /// <summary>
    /// Creates the standard starting game state for a new Breach game.
    /// Sets up:
    /// - Orange diagonal tiles at (0,0), (1,1), (2,2)
    /// - Green tiles at (0,1), (0,2), (1,0) — 2 upper-right, 1 lower-left
    /// - Purple tiles at (1,2), (2,0), (2,1) — 1 upper-right, 2 lower-left
    /// - Player 1 agents at (0,0) and (2,2) — opposing corners
    /// - Player 2 agents at (0,2) and (2,0) — opposing corners
    /// - Each player board starts with 1 orange, 1 green, 1 purple tile
    /// - Player One gets 1 AP on this first turn; Player Two waits
    /// </summary>
    /// <returns>A ready-to-play GameState.</returns>
    public static GameState CreateInitialState()
    {
        var board = new Board();

        // Orange diagonal
        board[0, 0] = OrangeTile();
        board[1, 1] = OrangeTile();
        board[2, 2] = OrangeTile();

        // Green tiles (2 upper-right, 1 lower-left)
        board[0, 1] = GreenTile();
        board[0, 2] = GreenTile();
        board[1, 0] = GreenTile();

        // Purple tiles (1 upper-right, 2 lower-left)
        board[1, 2] = PurpleTile();
        board[2, 0] = PurpleTile();
        board[2, 1] = PurpleTile();

        // Players and agents
        var player1 = new Player(
            PlayerId.One,
            new Agent(PlayerId.One, new Position(0, 0)),
            new Agent(PlayerId.One, new Position(2, 2)));

        var player2 = new Player(
            PlayerId.Two,
            new Agent(PlayerId.Two, new Position(0, 2)),
            new Agent(PlayerId.Two, new Position(2, 0)));

        // Player boards: one tile of each remaining color per player (TBD final distribution)
        player1.Board[0] = OrangeTile();
        player1.Board[1] = GreenTile();
        player1.Board[2] = PurpleTile();

        player2.Board[0] = OrangeTile();
        player2.Board[1] = GreenTile();
        player2.Board[2] = PurpleTile();

        return new GameState(board, player1, player2, isFirstTurn: true);
    }
}
