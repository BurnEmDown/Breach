namespace Breach.Core;

/// <summary>
/// Produces the standard starting <see cref="GameState"/> for a game of Breach.
/// </summary>
public static class GameSetup
{
    // -----------------------------------------------------------------------
    // Tile factories
    // NOTE: Exact secondary color values are TBD and can be replaced later
    //       without affecting any other game logic.
    // -----------------------------------------------------------------------

    public static Tile OrangeTile() => new(TileColor.Orange, TileColor.Green,  TileColor.Purple);
    public static Tile GreenTile()  => new(TileColor.Green,  TileColor.Orange, TileColor.Purple);
    public static Tile PurpleTile() => new(TileColor.Purple, TileColor.Orange, TileColor.Green);

    // -----------------------------------------------------------------------
    // Initial state
    // -----------------------------------------------------------------------

    /// <summary>
    /// Creates the standard starting game state:
    /// <list type="bullet">
    ///   <item>Orange diagonal: (0,0), (1,1), (2,2)</item>
    ///   <item>Green tiles: (0,1), (0,2), (1,0)  — 2 upper-right, 1 lower-left</item>
    ///   <item>Purple tiles: (1,2), (2,0), (2,1) — 1 upper-right, 2 lower-left</item>
    ///   <item>Player 1 agents: top-left (0,0) and bottom-right (2,2)</item>
    ///   <item>Player 2 agents: top-right (0,2) and bottom-left (2,0)</item>
    ///   <item>Player boards: each player starts with one tile of each color</item>
    /// </list>
    /// First player's first turn costs only 1 AP.
    /// </summary>
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
