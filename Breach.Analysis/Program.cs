using System.Text;
using System.Text.Json;

const int O = 1, G = 2, P = 3;

int[] InitialBoard() => [3, 2, 1, 2, 1, 3, 1, 3, 2];
int[] InitialPBoard() => [O, G, P];

static int Pos(int r, int c) => r * 3 + c;
static (int r, int c) RC(int pos) => (pos / 3, pos % 3);
static bool InBounds(int r, int c) => r is >= 0 and <= 2 && c is >= 0 and <= 2;
static int[] CopyArray(int[] a) => (int[])a.Clone();

static ulong EncodeState(int[] board, int a0, int a1, int[] pb)
{
    ulong s = 0;
    for (var i = 0; i < 9; i++) s |= (ulong)(board[i] & 3) << (i * 2);
    s |= (ulong)(a0 & 0xF) << 18;
    s |= (ulong)(a1 & 0xF) << 22;
    for (var i = 0; i < 3; i++) s |= (ulong)(pb[i] & 3) << (26 + i * 2);
    return s;
}

static ulong BoardKey(int[] board)
{
    ulong s = 0;
    for (var i = 0; i < 9; i++) s |= (ulong)(board[i] & 3) << (i * 2);
    return s;
}

static int[] DecodeBoardKey(ulong key)
{
    var board = new int[9];
    for (var i = 0; i < 9; i++) board[i] = (int)((key >> (i * 2)) & 3);
    return board;
}

static IEnumerable<(int[] board, int a0, int a1, int[] pb)> NextStates(int[] board, int a0, int a1, int[] pb)
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

    for (var slot = 0; slot < 3; slot++)
    {
        if (pb[slot] == 0) continue;
        var nb = CopyArray(board);
        var npb = CopyArray(pb);
        (nb[a0], npb[slot]) = (npb[slot], nb[a0]);
        yield return (nb, a0, a1, npb);
    }

    for (var slot = 0; slot < 3; slot++)
    {
        if (pb[slot] == 0) continue;
        var nb = CopyArray(board);
        var npb = CopyArray(pb);
        (nb[a1], npb[slot]) = (npb[slot], nb[a1]);
        yield return (nb, a0, a1, npb);
    }
}

static HashSet<ulong> ReachableBoards(int[] startBoard, int agentA, int agentB, int[] startPBoard, int maxDepth)
{
    var reachableBoards = new HashSet<ulong>();
    var queue = new Queue<(int[] board, int a0, int a1, int[] pb, int depth)>();
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

static bool IsAdjacent((int r, int c) a, (int r, int c) b)
{
    if (a == b) return false;
    return Math.Abs(a.r - b.r) <= 1 && Math.Abs(a.c - b.c) <= 1;
}

static (int r, int c)[] NormalizeShape(IEnumerable<(int r, int c)> cells)
{
    var arr = cells.OrderBy(x => x.r).ThenBy(x => x.c).ToArray();
    var minR = arr.Min(x => x.r);
    var minC = arr.Min(x => x.c);
    return arr.Select(x => (x.r - minR, x.c - minC)).OrderBy(x => x.Item1).ThenBy(x => x.Item2).ToArray();
}

static ((int r, int c)[] shape, int[] colors) RotateCard180((int r, int c)[] shape, int[] colors)
{
    var minR = shape.Min(x => x.r);
    var maxR = shape.Max(x => x.r);
    var minC = shape.Min(x => x.c);
    var maxC = shape.Max(x => x.c);

    var rotated = shape
        .Select((cell, i) => new
        {
            Cell = (r: maxR + minR - cell.r, c: maxC + minC - cell.c),
            Color = colors[i]
        })
        .OrderBy(x => x.Cell.r)
        .ThenBy(x => x.Cell.c)
        .ToArray();

    return (rotated.Select(x => x.Cell).ToArray(), rotated.Select(x => x.Color).ToArray());
}

static string ShapeKey((int r, int c)[] shape) => string.Join(";", shape.Select(x => $"{x.r},{x.c}"));

static bool IsForbiddenLevel1DiagonalShape((int r, int c)[] shape)
{
    return ShapeKey(shape) is "0,0;1,1;2,2" or "0,2;1,1;2,0";
}

static bool EachCellHasNeighbor((int r, int c)[] shape)
{
    foreach (var cell in shape)
    {
        var hasNeighbor = shape.Any(other => IsAdjacent(cell, other));
        if (!hasNeighbor) return false;
    }

    return true;
}

static List<(int r, int c)[]> GenerateConnectedShapes3()
{
    var allCells = (
        from r in Enumerable.Range(0, 3)
        from c in Enumerable.Range(0, 3)
        select (r, c)
    ).ToArray();

    var unique = new Dictionary<string, (int r, int c)[]>();

    for (var i = 0; i < allCells.Length; i++)
    for (var j = i + 1; j < allCells.Length; j++)
    for (var k = j + 1; k < allCells.Length; k++)
    {
        var shape = new[] { allCells[i], allCells[j], allCells[k] };
        if (!EachCellHasNeighbor(shape)) continue;

        var normalized = NormalizeShape(shape);
        if (IsForbiddenLevel1DiagonalShape(normalized)) continue;

        var key = ShapeKey(normalized);
        unique.TryAdd(key, normalized);
    }

    return unique.Values.OrderBy(ShapeKey).ToList();
}

static bool GoalMatchesBoard(int[] board, (int r, int c)[] shape, int[] colors)
{
    var maxR = shape.Max(x => x.r);
    var maxC = shape.Max(x => x.c);

    for (var anchorR = 0; anchorR <= 2 - maxR; anchorR++)
    {
        for (var anchorC = 0; anchorC <= 2 - maxC; anchorC++)
        {
            var matches = true;
            for (var i = 0; i < shape.Length; i++)
            {
                var r = anchorR + shape[i].r;
                var c = anchorC + shape[i].c;
                if (board[Pos(r, c)] != colors[i])
                {
                    matches = false;
                    break;
                }
            }

            if (matches) return true;
        }
    }

    return false;
}

static bool GoalMatchesAnyBoard(HashSet<ulong> boardKeys, (int r, int c)[] shape, int[] colors)
{
    foreach (var key in boardKeys)
    {
        var board = DecodeBoardKey(key);
        if (GoalMatchesBoard(board, shape, colors))
            return true;
    }

    return false;
}

static string ColorName(int c) => c switch { 1 => "O", 2 => "G", 3 => "P", _ => "?" };

static string FormatGoal((int r, int c)[] shape, int[] colors)
{
    var sb = new StringBuilder();
    sb.Append("cells=");
    sb.Append(string.Join(",", shape.Select((cell, i) => $"({cell.r},{cell.c})={ColorName(colors[i])}")));
    return sb.ToString();
}

static string RenderCardAscii((int r, int c)[] shape, int[] colors)
{
    var height = shape.Max(cell => cell.r) + 1;
    var width = shape.Max(cell => cell.c) + 1;
    var grid = new string[height, width];

    for (var row = 0; row < height; row++)
    for (var col = 0; col < width; col++)
        grid[row, col] = "   ";

    for (var i = 0; i < shape.Length; i++)
        grid[shape[i].r, shape[i].c] = $" {ColorName(colors[i])} ";

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

static object ToCardJson((int r, int c)[] shape, int[] colors)
{
    return new
    {
        shape = shape.Select((cell, i) => new
        {
            rowOffset = cell.r,
            colOffset = cell.c,
            colorAbbrev = ColorName(colors[i]),
            color = colors[i] switch
            {
                O => "Orange",
                G => "Green",
                P => "Purple",
                _ => "Unknown"
            }
        }).ToArray()
    };
}

const int maxActions = 2;

var initialBoard = InitialBoard();
var initialPBoard = InitialPBoard();

var p1Boards = ReachableBoards(initialBoard, Pos(0, 0), Pos(2, 2), initialPBoard, maxActions);
var p2Boards = ReachableBoards(initialBoard, Pos(0, 2), Pos(2, 0), initialPBoard, maxActions);

var shapes = GenerateConnectedShapes3();
var colors = new[] { O, G, P };

var allCards = new List<((int r, int c)[] shape, int[] colors)>();

foreach (var shape in shapes)
{
    foreach (var c0 in colors)
    foreach (var c1 in colors)
    foreach (var c2 in colors)
    {
        var cardColors = new[] { c0, c1, c2 };
        if (cardColors.Distinct().Count() < 2) continue;
        allCards.Add((shape, cardColors));
    }
}

var hardCards = new List<((int r, int c)[] shape, int[] colors)>();

foreach (var card in allCards)
{
    var p1Can = GoalMatchesAnyBoard(p1Boards, card.shape, card.colors);
    var rotatedForPlayerTwo = RotateCard180(card.shape, card.colors);
    var p2Can = GoalMatchesAnyBoard(p2Boards, rotatedForPlayerTwo.shape, rotatedForPlayerTwo.colors);

    if (!p1Can && !p2Can)
        hardCards.Add(card);
}

Console.WriteLine($"Player 1 reachable boards within {maxActions} actions: {p1Boards.Count}");
Console.WriteLine($"Player 2 reachable boards within {maxActions} actions: {p2Boards.Count}");
Console.WriteLine($"Unique connected 3-cell shapes (normalized): {shapes.Count}");
Console.WriteLine($"Total valid level-1 cards (shape × colors with >=2 colors): {allCards.Count}");
Console.WriteLine($"Hard cards (neither player can achieve in <= {maxActions} actions): {hardCards.Count}");
Console.WriteLine();

foreach (var card in hardCards.OrderBy(x => ShapeKey(x.shape)).ThenBy(x => string.Join("", x.colors.Select(ColorName))))
{
    Console.WriteLine(FormatGoal(card.shape, card.colors));
}

var jsonOutput = new
{
    maxActions,
    player1ReachableBoards = p1Boards.Count,
    player2ReachableBoards = p2Boards.Count,
    uniqueConnectedShapes = shapes.Count,
    totalValidLevel1Cards = allCards.Count,
    hardCardsCount = hardCards.Count,
    hardCards = hardCards
        .OrderBy(x => ShapeKey(x.shape))
        .ThenBy(x => string.Join("", x.colors.Select(ColorName)))
        .Select(card => ToCardJson(card.shape, card.colors))
        .ToArray()
};

var json = JsonSerializer.Serialize(jsonOutput, new JsonSerializerOptions
{
    WriteIndented = true
});

File.WriteAllText("hard-cards.json", json);

var visualOutput = new StringBuilder();
var totalOrange = hardCards.Sum(card => card.colors.Count(color => color == O));
var totalGreen = hardCards.Sum(card => card.colors.Count(color => color == G));
var totalPurple = hardCards.Sum(card => card.colors.Count(color => color == P));

visualOutput.AppendLine($"Hard Level-1 Goal Cards ({hardCards.Count})");
visualOutput.AppendLine($"Generated with maxActions={maxActions}");
visualOutput.AppendLine($"Total color usage across all hard goals: O={totalOrange}, G={totalGreen}, P={totalPurple}");
visualOutput.AppendLine();

var orderedHardCards = hardCards
    .OrderBy(x => ShapeKey(x.shape))
    .ThenBy(x => string.Join("", x.colors.Select(ColorName)))
    .ToList();

for (var i = 0; i < orderedHardCards.Count; i++)
{
    var card = orderedHardCards[i];

    visualOutput.AppendLine($"Card {i + 1}");
    visualOutput.AppendLine(FormatGoal(card.shape, card.colors));
    visualOutput.AppendLine();
    visualOutput.AppendLine("View:");
    visualOutput.Append(RenderCardAscii(card.shape, card.colors));
    visualOutput.AppendLine(new string('=', 48));
    visualOutput.AppendLine();
}

File.WriteAllText("hard-cards-visual.txt", visualOutput.ToString());
Console.WriteLine();
Console.WriteLine("Exported hard cards JSON to hard-cards.json");
Console.WriteLine("Exported hard cards visual text to hard-cards-visual.txt");
