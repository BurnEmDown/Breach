namespace Breach.Core;

/// <summary>
/// Represents the 3×3 main game board where tiles are placed and agents move.
/// Board positions can be empty (null) or contain a tile. Indexing follows
/// (Row, Col) coordinates where Row 0 = top and Col 0 = left.
/// </summary>
public sealed class Board
{
    private readonly Tile?[,] _tiles = new Tile?[3, 3];

    /// <summary>Gets or sets the tile at the specified position.</summary>
    /// <param name="pos">The board position to access.</param>
    public Tile? this[Position pos]
    {
        get => _tiles[pos.Row, pos.Col];
        set => _tiles[pos.Row, pos.Col] = value;
    }

    /// <summary>Gets or sets the tile at the specified row and column.</summary>
    /// <param name="row">Row index (0-2).</param>
    /// <param name="col">Column index (0-2).</param>
    public Tile? this[int row, int col]
    {
        get => _tiles[row, col];
        set => _tiles[row, col] = value;
    }

    /// <summary>
    /// Swaps two tiles on the board (used by the Switch action).
    /// This atomically exchanges the tiles at the two positions.
    /// </summary>
    /// <param name="a">First position.</param>
    /// <param name="b">Second position.</param>
    public void Swap(Position a, Position b) =>
        (_tiles[a.Row, a.Col], _tiles[b.Row, b.Col]) =
        (_tiles[b.Row, b.Col], _tiles[a.Row, a.Col]);

    /// <summary>
    /// Creates and returns a deep copy of this board, including all tiles.
    /// Useful for simulating actions without modifying the current state.
    /// </summary>
    public Board Clone()
    {
        var clone = new Board();
        for (var r = 0; r < 3; r++)
        for (var c = 0; c < 3; c++)
            clone._tiles[r, c] = _tiles[r, c];
        return clone;
    }
}
