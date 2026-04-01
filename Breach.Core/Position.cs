namespace Breach.Core;

/// <summary>
/// A zero-indexed (Row, Col) coordinate on the 3×3 main board where gameplay occurs.
/// Row 0 = top, Row 2 = bottom; Col 0 = left, Col 2 = right.
/// </summary>
/// <param name="Row">Row index (0-2); 0 is the top of the board.</param>
/// <param name="Col">Column index (0-2); 0 is the left of the board.</param>
public readonly record struct Position(int Row, int Col)
{
    /// <summary>Returns true if this position is within the 3×3 board bounds.</summary>
    public bool IsValid() => Row is >= 0 and < 3 && Col is >= 0 and < 3;

    /// <summary>
    /// Returns all orthogonally adjacent positions (up, down, left, right)
    /// that remain within board bounds.
    /// </summary>
    public IEnumerable<Position> OrthogonalNeighbors()
    {
        if (Row > 0) yield return new Position(Row - 1, Col);
        if (Row < 2) yield return new Position(Row + 1, Col);
        if (Col > 0) yield return new Position(Row, Col - 1);
        if (Col < 2) yield return new Position(Row, Col + 1);
    }

    /// <summary>
    /// Returns true if the other position is exactly one step away orthogonally
    /// (not diagonal). Used to validate Move action targets.
    /// </summary>
    public bool IsOrthogonallyAdjacentTo(Position other) =>
        (Math.Abs(Row - other.Row) == 1 && Col == other.Col) ||
        (Row == other.Row && Math.Abs(Col - other.Col) == 1);

    /// <summary>Returns a string representation in the format "(row,col)".</summary>
    public override string ToString() => $"({Row},{Col})";
}
