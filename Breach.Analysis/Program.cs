using System.Text;
using System.Text.Json;
using Breach.Core;

const int maxActions = 2;

var initialState = GameSetup.CreateInitialState(randomSeed: 42);
var initialBoard = CloneBoard(initialState.Board);
var initialPlayerBoard = ClonePlayerBoard(initialState.Players[0].Board);

var p1Boards = ReachableBoards(initialBoard, Pos(0, 0), Pos(2, 2), initialPlayerBoard, maxActions);
var p2Boards = ReachableBoards(initialBoard, Pos(0, 2), Pos(2, 0), initialPlayerBoard, maxActions);

var allLevel1Goals = GoalCatalog.GenerateAllLevel1Goals();
var allLevel2Goals = GoalCatalog.GenerateAllLevel2Goals();
var hardLevel1Goals = allLevel1Goals
    .Where(goal => !CanPlayerAchieveGoal(p1Boards, goal, PlayerId.One) && !CanPlayerAchieveGoal(p2Boards, goal, PlayerId.Two))
    .OrderBy(goal => ShapeKey(goal.Requirements))
    .ThenBy(goal => ColorKey(goal.Requirements))
    .ToArray();

Console.WriteLine($"Player 1 reachable boards within {maxActions} actions: {p1Boards.Count}");
Console.WriteLine($"Player 2 reachable boards within {maxActions} actions: {p2Boards.Count}");
Console.WriteLine($"Valid Level-1 goals: {allLevel1Goals.Count}");
Console.WriteLine($"Hard Level-1 goals: {hardLevel1Goals.Length}");
Console.WriteLine($"Valid Level-2 goals: {allLevel2Goals.Count}");
Console.WriteLine();

ExportGoalJson(
    fileName: "hard-cards.json",
    title: "Hard Level-1 Goal Cards",
    maxActions,
    p1Boards.Count,
    p2Boards.Count,
    totalGoals: hardLevel1Goals.Length,
    goals: hardLevel1Goals);

ExportGoalVisual(
    fileName: "hard-cards-visual.txt",
    title: "Hard Level-1 Goal Cards",
    maxActions,
    hardLevel1Goals);

ExportGoalJson(
    fileName: "level2-cards.json",
    title: "All Level-2 Goal Cards",
    maxActions: null,
    player1ReachableBoards: null,
    player2ReachableBoards: null,
    totalGoals: allLevel2Goals.Count,
    goals: allLevel2Goals);

ExportGoalVisual(
    fileName: "level2-cards-visual.txt",
    title: "All Level-2 Goal Cards",
    maxActions: null,
    allLevel2Goals);

Console.WriteLine("Exported hard cards JSON to hard-cards.json");
Console.WriteLine("Exported hard cards visual text to hard-cards-visual.txt");
Console.WriteLine("Exported Level-2 cards JSON to level2-cards.json");
Console.WriteLine("Exported Level-2 cards visual text to level2-cards-visual.txt");

static int Pos(int row, int col) => row * 3 + col;

static TileColor?[] CloneBoard(Board board)
{
    var result = new TileColor?[9];
    for (var row = 0; row < 3; row++)
    for (var col = 0; col < 3; col++)
        result[Pos(row, col)] = board[row, col]?.Color;
    return result;
}

static TileColor?[] ClonePlayerBoard(PlayerBoard board)
{
    var result = new TileColor?[PlayerBoard.Size];
    for (var i = 0; i < PlayerBoard.Size; i++)
        result[i] = board[i]?.Color;
    return result;
}

static ulong EncodeState(TileColor?[] board, int a0, int a1, TileColor?[] pb)
{
    ulong s = 0;
    for (var i = 0; i < 9; i++) s |= (ulong)(EncodeColor(board[i]) & 3) << (i * 2);
    s |= (ulong)(a0 & 0xF) << 18;
    s |= (ulong)(a1 & 0xF) << 22;
    for (var i = 0; i < pb.Length; i++) s |= (ulong)(EncodeColor(pb[i]) & 3) << (26 + i * 2);
    return s;
}

static ulong BoardKey(TileColor?[] board)
{
    ulong s = 0;
    for (var i = 0; i < 9; i++) s |= (ulong)(EncodeColor(board[i]) & 3) << (i * 2);
    return s;
}

static TileColor?[] DecodeBoardKey(ulong key)
{
    var board = new TileColor?[9];
    for (var i = 0; i < 9; i++) board[i] = DecodeColor((int)((key >> (i * 2)) & 3));
    return board;
}

static int EncodeColor(TileColor? color) => color switch
{
    TileColor.Orange => 1,
    TileColor.Green => 2,
    TileColor.Purple => 3,
    _ => 0
};

static TileColor? DecodeColor(int value) => value switch
{
    1 => TileColor.Orange,
    2 => TileColor.Green,
    3 => TileColor.Purple,
    _ => null
};

static (int row, int col) RC(int pos) => (pos / 3, pos % 3);

static bool InBounds(int row, int col) => row is >= 0 and <= 2 && col is >= 0 and <= 2;

static TileColor?[] CopyArray(TileColor?[] a) => (TileColor?[])a.Clone();

static IEnumerable<(TileColor?[] board, int a0, int a1, TileColor?[] pb)> NextStates(TileColor?[] board, int a0, int a1, TileColor?[] pb)
{
    int[] dR = [-1, 1, 0, 0];
    int[] dC = [0, 0, -1, 1];

    var (r0, c0) = RC(a0);
    for (var d = 0; d < 4; d++)
    {
        var nr = r0 + dR[d];
        var nc = c0 + dC[d];
        if (!InBounds(nr, nc)) continue;
        var npos = Pos(nr, nc);
        if (npos == a1) continue;
        yield return (CopyArray(board), npos, a1, CopyArray(pb));
    }

    var (r1, c1) = RC(a1);
    for (var d = 0; d < 4; d++)
    {
        var nr = r1 + dR[d];
        var nc = c1 + dC[d];
        if (!InBounds(nr, nc)) continue;
        var npos = Pos(nr, nc);
        if (npos == a0) continue;
        yield return (CopyArray(board), a0, npos, CopyArray(pb));
    }

    {
        var nb = CopyArray(board);
        (nb[a0], nb[a1]) = (nb[a1], nb[a0]);
        yield return (nb, a0, a1, CopyArray(pb));
    }

    for (var slot = 0; slot < pb.Length; slot++)
    {
        if (pb[slot] is null) continue;
        var nb = CopyArray(board);
        var npb = CopyArray(pb);
        (nb[a0], npb[slot]) = (npb[slot], nb[a0]);
        yield return (nb, a0, a1, npb);
    }

    for (var slot = 0; slot < pb.Length; slot++)
    {
        if (pb[slot] is null) continue;
        var nb = CopyArray(board);
        var npb = CopyArray(pb);
        (nb[a1], npb[slot]) = (npb[slot], nb[a1]);
        yield return (nb, a0, a1, npb);
    }
}

static HashSet<ulong> ReachableBoards(TileColor?[] startBoard, int agentA, int agentB, TileColor?[] startPBoard, int maxDepth)
{
    var reachableBoards = new HashSet<ulong>();
    var queue = new Queue<(TileColor?[] board, int a0, int a1, TileColor?[] pb, int depth)>();
    var visited = new HashSet<ulong>();

    var board0 = CopyArray(startBoard);
    var pb0 = CopyArray(startPBoard);
    visited.Add(EncodeState(board0, agentA, agentB, pb0));
    reachableBoards.Add(BoardKey(board0));
    queue.Enqueue((board0, agentA, agentB, pb0, 0));

    while (queue.Count > 0)
    {
        var (board, a0, a1, pb, depth) = queue.Dequeue();
        if (depth >= maxDepth) continue;

        foreach (var (nb, na0, na1, npb) in NextStates(board, a0, a1, pb))
        {
            var stateKey = EncodeState(nb, na0, na1, npb);
            if (!visited.Add(stateKey)) continue;
            reachableBoards.Add(BoardKey(nb));
            queue.Enqueue((nb, na0, na1, npb, depth + 1));
        }
    }

    return reachableBoards;
}

static bool CanPlayerAchieveGoal(HashSet<ulong> boardKeys, GoalTile goal, PlayerId playerId)
{
    foreach (var key in boardKeys)
    {
        var boardState = DecodeBoardKey(key);
        var board = new Board();
        for (var row = 0; row < 3; row++)
        for (var col = 0; col < 3; col++)
        {
            var color = boardState[Pos(row, col)];
            if (color is not null)
                board[row, col] = new Tile(color.Value);
        }

        if (GoalEvaluator.IsGoalSatisfied(board, goal, playerId))
            return true;
    }

    return false;
}

static string ShapeKey(IEnumerable<GoalRequirementCell> requirements) =>
    string.Join(";", requirements.OrderBy(r => r.RowOffset).ThenBy(r => r.ColOffset).Select(r => $"{r.RowOffset},{r.ColOffset}"));

static string ColorKey(IEnumerable<GoalRequirementCell> requirements) =>
    string.Join(string.Empty, requirements.OrderBy(r => r.RowOffset).ThenBy(r => r.ColOffset).Select(r => r.Color.ToAbbrev()));

static void ExportGoalJson(
    string fileName,
    string title,
    int? maxActions,
    int? player1ReachableBoards,
    int? player2ReachableBoards,
    int totalGoals,
    IReadOnlyList<GoalTile> goals)
{
    var jsonOutput = new
    {
        title,
        maxActions,
        player1ReachableBoards,
        player2ReachableBoards,
        totalGoals,
        goals = goals.Select(goal => new
        {
            goal.Id,
            goal.Name,
            level = goal.Level.ToString(),
            requirements = goal.Requirements.Select(r => new
            {
                r.RowOffset,
                r.ColOffset,
                colorAbbrev = r.Color.ToAbbrev(),
                color = r.Color.ToString()
            }).ToArray()
        }).ToArray()
    };

    File.WriteAllText(fileName, JsonSerializer.Serialize(jsonOutput, new JsonSerializerOptions
    {
        WriteIndented = true
    }));
}

static void ExportGoalVisual(string fileName, string title, int? maxActions, IReadOnlyList<GoalTile> goals)
{
    var orderedGoals = goals
        .OrderBy(goal => ShapeKey(goal.Requirements))
        .ThenBy(goal => ColorKey(goal.Requirements))
        .ToArray();

    var totalOrange = orderedGoals.Sum(goal => goal.Requirements.Count(r => r.Color == TileColor.Orange));
    var totalGreen = orderedGoals.Sum(goal => goal.Requirements.Count(r => r.Color == TileColor.Green));
    var totalPurple = orderedGoals.Sum(goal => goal.Requirements.Count(r => r.Color == TileColor.Purple));

    var output = new StringBuilder();
    output.AppendLine($"{title} ({orderedGoals.Length})");
    if (maxActions.HasValue)
        output.AppendLine($"Generated with maxActions={maxActions.Value}");
    output.AppendLine($"Total color usage across all goals: O={totalOrange}, G={totalGreen}, P={totalPurple}");
    output.AppendLine();

    for (var i = 0; i < orderedGoals.Length; i++)
    {
        var goal = orderedGoals[i];
        output.AppendLine($"Card {i + 1}");
        output.AppendLine(goal.Name);
        output.AppendLine($"id={goal.Id}");
        output.AppendLine($"cells={string.Join(",", goal.Requirements.OrderBy(r => r.RowOffset).ThenBy(r => r.ColOffset).Select(r => $"({r.RowOffset},{r.ColOffset})={r.Color.ToAbbrev()}"))}");
        output.AppendLine();
        output.AppendLine("View:");
        output.Append(RenderCardAscii(goal.Requirements));
        output.AppendLine(new string('=', 48));
        output.AppendLine();
    }

    File.WriteAllText(fileName, output.ToString());
}

static string RenderCardAscii(IReadOnlyList<GoalRequirementCell> requirements)
{
    var minRow = requirements.Min(r => r.RowOffset);
    var minCol = requirements.Min(r => r.ColOffset);
    var normalized = requirements
        .Select(r => new GoalRequirementCell(r.RowOffset - minRow, r.ColOffset - minCol, r.Color))
        .ToArray();

    var height = normalized.Max(cell => cell.RowOffset) + 1;
    var width = normalized.Max(cell => cell.ColOffset) + 1;
    var grid = new string[height, width];

    for (var row = 0; row < height; row++)
    for (var col = 0; col < width; col++)
        grid[row, col] = "   ";

    foreach (var requirement in normalized)
        grid[requirement.RowOffset, requirement.ColOffset] = $" {requirement.Color.ToAbbrev()} ";

    var sb = new StringBuilder();
    sb.Append("     ");
    for (var col = 0; col < width; col++)
        sb.Append($"{col,7}");
    sb.AppendLine();
    sb.AppendLine("  +" + string.Join("+", Enumerable.Repeat("-------", width)) + "+");

    for (var row = 0; row < height; row++)
    {
        sb.Append($"{row} |");
        for (var col = 0; col < width; col++)
            sb.Append($" {grid[row, col],5} |");

        sb.AppendLine();
        sb.AppendLine("  +" + string.Join("+", Enumerable.Repeat("-------", width)) + "+");
    }

    return sb.ToString();
}
