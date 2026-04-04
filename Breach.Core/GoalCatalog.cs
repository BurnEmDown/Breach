namespace Breach.Core;

/// <summary>
/// Generates all valid goal cards for each goal level based on the game's
/// structural and color rules.
/// </summary>
public static class GoalCatalog
{
    private static readonly TileColor[] AvailableColors =
    [
        TileColor.Orange,
        TileColor.Green,
        TileColor.Purple
    ];

    /// <summary>Generates every valid goal card for the requested level.</summary>
    public static IReadOnlyList<GoalTile> GenerateAllGoals(GoalLevel level)
    {
        var cellCount = level switch
        {
            GoalLevel.Level1 => 3,
            GoalLevel.Level2 => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(level), "Unsupported goal level.")
        };

        var shapes = GenerateNormalizedShapes(cellCount, excludeFullDiagonals: level == GoalLevel.Level1);
        var goals = new List<GoalTile>();

        foreach (var shape in shapes)
        {
            foreach (var colors in GenerateColorAssignments(cellCount))
            {
                if (colors.Distinct().Count() < 2)
                    continue;

                var requirements = shape
                    .Select((cell, index) => new GoalRequirementCell(cell.Row, cell.Col, colors[index]))
                    .ToArray();

                goals.Add(new GoalTile(
                    BuildGoalId(level, shape, colors),
                    BuildGoalName(level, shape, colors),
                    level,
                    requirements));
            }
        }

        return goals;
    }

    /// <summary>Generates every valid Level-1 goal card.</summary>
    public static IReadOnlyList<GoalTile> GenerateAllLevel1Goals() =>
        GenerateAllGoals(GoalLevel.Level1);

    /// <summary>Generates every valid Level-2 goal card.</summary>
    public static IReadOnlyList<GoalTile> GenerateAllLevel2Goals() =>
        GenerateAllGoals(GoalLevel.Level2);

    private static IReadOnlyList<(int Row, int Col)[]> GenerateNormalizedShapes(int cellCount, bool excludeFullDiagonals)
    {
        var allCells = (
            from row in Enumerable.Range(0, 3)
            from col in Enumerable.Range(0, 3)
            select (Row: row, Col: col)
        ).ToArray();

        var uniqueShapes = new Dictionary<string, (int Row, int Col)[]>();

        foreach (var shape in GenerateCombinations(allCells, cellCount))
        {
            if (!EachCellHasNeighbor(shape))
                continue;

            var normalized = NormalizeShape(shape);
            if (excludeFullDiagonals && IsForbiddenLevel1DiagonalShape(normalized))
                continue;

            uniqueShapes.TryAdd(ShapeKey(normalized), normalized);
        }

        return uniqueShapes.Values
            .OrderBy(ShapeKey)
            .ToArray();
    }

    private static IEnumerable<T[]> GenerateCombinations<T>(IReadOnlyList<T> items, int count)
    {
        var buffer = new T[count];

        return Recurse(0, 0);

        IEnumerable<T[]> Recurse(int startIndex, int depth)
        {
            if (depth == count)
            {
                yield return buffer.ToArray();
                yield break;
            }

            for (var i = startIndex; i <= items.Count - (count - depth); i++)
            {
                buffer[depth] = items[i];
                foreach (var combination in Recurse(i + 1, depth + 1))
                    yield return combination;
            }
        }
    }

    private static IEnumerable<TileColor[]> GenerateColorAssignments(int count)
    {
        var buffer = new TileColor[count];
        return Recurse(0);

        IEnumerable<TileColor[]> Recurse(int depth)
        {
            if (depth == count)
            {
                yield return buffer.ToArray();
                yield break;
            }

            foreach (var color in AvailableColors)
            {
                buffer[depth] = color;
                foreach (var assignment in Recurse(depth + 1))
                    yield return assignment;
            }
        }
    }

    private static bool EachCellHasNeighbor((int Row, int Col)[] shape)
    {
        foreach (var cell in shape)
        {
            var hasNeighbor = shape.Any(other =>
                !(other.Row == cell.Row && other.Col == cell.Col)
                && Math.Abs(other.Row - cell.Row) <= 1
                && Math.Abs(other.Col - cell.Col) <= 1);

            if (!hasNeighbor)
                return false;
        }

        return true;
    }

    private static (int Row, int Col)[] NormalizeShape(IEnumerable<(int Row, int Col)> cells)
    {
        var arr = cells.OrderBy(x => x.Row).ThenBy(x => x.Col).ToArray();
        var minRow = arr.Min(x => x.Row);
        var minCol = arr.Min(x => x.Col);

        return arr
            .Select(x => (x.Row - minRow, x.Col - minCol))
            .OrderBy(x => x.Item1)
            .ThenBy(x => x.Item2)
            .Select(x => (Row: x.Item1, Col: x.Item2))
            .ToArray();
    }

    private static bool IsForbiddenLevel1DiagonalShape((int Row, int Col)[] shape) =>
        ShapeKey(shape) is "0,0;1,1;2,2" or "0,2;1,1;2,0";

    private static string ShapeKey((int Row, int Col)[] shape) =>
        string.Join(";", shape.Select(x => $"{x.Row},{x.Col}"));

    private static string BuildGoalId(GoalLevel level, (int Row, int Col)[] shape, IReadOnlyList<TileColor> colors)
    {
        var levelPrefix = level == GoalLevel.Level1 ? "L1" : "L2";
        var shapeId = string.Join("-", shape.Select(x => $"{x.Row}{x.Col}"));
        var colorId = string.Join(string.Empty, colors.Select(c => c.ToAbbrev()));
        return $"{levelPrefix}-{shapeId}-{colorId}";
    }

    private static string BuildGoalName(GoalLevel level, (int Row, int Col)[] shape, IReadOnlyList<TileColor> colors)
    {
        var colorText = string.Join("-", colors.Select(c => c.ToAbbrev()));
        var shapeText = string.Join(", ", shape.Select(x => $"({x.Row},{x.Col})"));
        return $"{level} {colorText} [{shapeText}]";
    }
}
