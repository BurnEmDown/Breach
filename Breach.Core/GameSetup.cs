namespace Breach.Core;

/// <summary>
/// Responsible for creating the standard starting configuration of a Breach game.
/// Use <see cref="CreateInitialState"/> to get a ready-to-play GameState.
/// </summary>
public static class GameSetup
{
    private static readonly GoalTile[] Level1GoalPool = GoalCatalog.GenerateAllLevel1Goals().ToArray();
    private static readonly GoalTile[] Level2GoalPool = GoalCatalog.GenerateAllLevel2Goals().ToArray();

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
    /// Sets up the board in a fixed pattern:
    /// <code>
    ///   P G O
    ///   G O P
    ///   O P G
    /// </code>
    /// - Player 1 agents at (0,0) and (2,2) — the non-orange corners (Purple, Green)
    /// - Player 2 agents at (0,2) and (2,0) — the orange corners
    /// - Each player board starts with 1 orange, 1 green, 1 purple tile
    /// - Player One gets 1 AP on this first turn; Player Two waits
    /// </summary>
    /// <returns>A ready-to-play GameState.</returns>
    public static GameState CreateInitialState(int? randomSeed = null)
    {
        var board = new Board();

        // Row 0: P G O
        board[0, 0] = PurpleTile();
        board[0, 1] = GreenTile();
        board[0, 2] = OrangeTile();

        // Row 1: G O P
        board[1, 0] = GreenTile();
        board[1, 1] = OrangeTile();
        board[1, 2] = PurpleTile();

        // Row 2: O P G
        board[2, 0] = OrangeTile();
        board[2, 1] = PurpleTile();
        board[2, 2] = GreenTile();

        // Players and agents
        // Player 1 on non-orange corners: (0,0)=Purple, (2,2)=Green
        var player1 = new Player(
            PlayerId.One,
            new Agent(PlayerId.One, new Position(0, 0)),
            new Agent(PlayerId.One, new Position(2, 2)));

        // Player 2 on orange corners: (0,2)=Orange, (2,0)=Orange
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
        return DrawRandomGoals(Level1GoalPool, count, random);
    }

    private static GoalTile[] DrawRandomLevel2Goals(int count, Random random)
    {
        return DrawRandomGoals(Level2GoalPool, count, random);
    }

    public static IReadOnlyList<GoalTile> GetAllLevel1Goals() => Level1GoalPool;

    public static IReadOnlyList<GoalTile> GetAllLevel2Goals() => Level2GoalPool;

    private static GoalTile[] DrawRandomGoals(GoalTile[] poolSource, int count, Random random)
    {
        if (count > poolSource.Length)
            throw new InvalidOperationException("Goal pool is too small for requested draw count.");

        var pool = poolSource.ToArray();

        for (var i = pool.Length - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        return pool.Take(count).ToArray();
    }
}
