namespace Breach.Core;

/// <summary>
/// A zero-indexed (Row, Col) coordinate on the 3×3 main board.
/// Row 0 = top, Col 0 = left.
/// </summary>
public readonly record struct Position(int Row, int Col)
{
    public bool IsValid() => Row is >= 0 and < 3 && Col is >= 0 and < 3;

    /// <summary>Returns the four orthogonally adjacent positions that are within bounds.</summary>
    public IEnumerable<Position> OrthogonalNeighbors()
    {
        if (Row > 0) yield return new Position(Row - 1, Col);
        if (Row < 2) yield return new Position(Row + 1, Col);
        if (Col > 0) yield return new Position(Row, Col - 1);
        if (Col < 2) yield return new Position(Row, Col + 1);
    }

    public bool IsOrthogonallyAdjacentTo(Position other) =>
        (Math.Abs(Row - other.Row) == 1 && Col == other.Col) ||
        (Row == other.Row && Math.Abs(Col - other.Col) == 1);

    public override string ToString() => $"({Row},{Col})";
}
