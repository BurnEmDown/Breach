namespace Breach.Core;

/// <summary>
/// Responsible for creating the standard starting configuration of a Breach game.
/// Use <see cref="CreateInitialState"/> to get a ready-to-play GameState.
/// </summary>
public static class GameSetup
{
    private static readonly GoalTile[] Level1GoalPool =
    [
        new GoalTile(
            "L1-ROW-OOG",
            "Row O-O-G",
            GoalLevel.Level1,
            [
                new GoalRequirementCell(0, 0, TileColor.Orange),
                new GoalRequirementCell(0, 1, TileColor.Orange),
                new GoalRequirementCell(0, 2, TileColor.Green)
            ]),
        new GoalTile(
            "L1-ROW-GGP",
            "Row G-G-P",
            GoalLevel.Level1,
            [
                new GoalRequirementCell(0, 0, TileColor.Green),
                new GoalRequirementCell(0, 1, TileColor.Green),
                new GoalRequirementCell(0, 2, TileColor.Purple)
            ]),
        new GoalTile(
            "L1-ROW-PPO",
            "Row P-P-O",
            GoalLevel.Level1,
            [
                new GoalRequirementCell(0, 0, TileColor.Purple),
                new GoalRequirementCell(0, 1, TileColor.Purple),
                new GoalRequirementCell(0, 2, TileColor.Orange)
            ]),
        new GoalTile(
            "L1-ROW-OGP",
            "Row O-G-P",
            GoalLevel.Level1,
            [
                new GoalRequirementCell(0, 0, TileColor.Orange),
                new GoalRequirementCell(0, 1, TileColor.Green),
                new GoalRequirementCell(0, 2, TileColor.Purple)
            ]),
        new GoalTile(
            "L1-ROW-GOP",
            "Row G-O-P",
            GoalLevel.Level1,
            [
                new GoalRequirementCell(0, 0, TileColor.Green),
                new GoalRequirementCell(0, 1, TileColor.Orange),
                new GoalRequirementCell(0, 2, TileColor.Purple)
            ])
    ];

    // -----------------------------------------------------------------------
    // Tile factories
    // -----------------------------------------------------------------------

    /// <summary>
    /// Creates an orange tile. Orange tiles are used for the opening diagonal.
    /// </summary>
    public static Tile OrangeTile() => new(TileColor.Orange);
    
    /// <summary>
    /// Creates a green tile. Green tiles are distributed 2 upper-right, 1 lower-left.
    /// </summary>
    public static Tile GreenTile()  => new(TileColor.Green);
    
    /// <summary>
    /// Creates a purple tile. Purple tiles are distributed 1 upper-right, 2 lower-left.
    /// </summary>
    public static Tile PurpleTile() => new(TileColor.Purple);

    // -----------------------------------------------------------------------
    // Initial state
    // -----------------------------------------------------------------------

    /// <summary>
    /// Creates the standard starting game state for a new Breach game.
    /// Sets up:
    /// - Orange diagonal tiles at (0,0), (1,1), (2,2)
    /// - Green tiles at (0,1), (1,2), (1,0) — 2 upper-right, 1 lower-left
    /// - Purple tiles at (1,2), (2,0), (2,1) — 1 upper-right, 2 lower-left
    /// - Player 1 agents at (0,0) and (2,2) — opposing corners
    /// - Player 2 agents at (0,2) and (2,0) — opposing corners
    /// - Each player board starts with 1 orange, 1 green, 1 purple tile
    /// - Player One gets 1 AP on this first turn; Player Two waits
    /// </summary>
    /// <returns>A ready-to-play GameState.</returns>
    public static GameState CreateInitialState(int? randomSeed = null)
    {
        var board = new Board();

        // Orange diagonal
        board[0, 0] = OrangeTile();
        board[1, 1] = OrangeTile();
        board[2, 2] = OrangeTile();

        // Green tiles (2 upper-right, 1 lower-left)
        board[0, 1] = GreenTile();
        board[1, 2] = GreenTile();
        board[2, 0] = GreenTile();

        // Purple tiles (1 upper-right, 2 lower-left)
        board[0, 2] = PurpleTile();
        board[1, 0] = PurpleTile();
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

        var random = randomSeed.HasValue ? new Random(randomSeed.Value) : Random.Shared;
        var goals = DrawRandomLevel1Goals(4, random);
        player1.GoalTiles.Add(goals[0]);
        player1.GoalTiles.Add(goals[1]);
        player2.GoalTiles.Add(goals[2]);
        player2.GoalTiles.Add(goals[3]);

        return new GameState(board, player1, player2, isFirstTurn: true);
    }

    private static GoalTile[] DrawRandomLevel1Goals(int count, Random random)
    {
        if (count > Level1GoalPool.Length)
            throw new InvalidOperationException("Goal pool is too small for requested draw count.");

        var pool = Level1GoalPool.ToArray();

        for (var i = pool.Length - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        return pool.Take(count).ToArray();
    }
}
