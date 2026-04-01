namespace Breach.Core;

/// <summary>3×3 main board. Tiles are nullable — a slot can be empty.</summary>
public sealed class Board
{
    private readonly Tile?[,] _tiles = new Tile?[3, 3];

    public Tile? this[Position pos]
    {
        get => _tiles[pos.Row, pos.Col];
        set => _tiles[pos.Row, pos.Col] = value;
    }

    public Tile? this[int row, int col]
    {
        get => _tiles[row, col];
        set => _tiles[row, col] = value;
    }

    public void Swap(Position a, Position b) =>
        (_tiles[a.Row, a.Col], _tiles[b.Row, b.Col]) =
        (_tiles[b.Row, b.Col], _tiles[a.Row, a.Col]);

    /// <summary>Returns a deep copy of the board.</summary>
    public Board Clone()
    {
        var clone = new Board();
        for (var r = 0; r < 3; r++)
        for (var c = 0; c < 3; c++)
            clone._tiles[r, c] = _tiles[r, c];
        return clone;
    }
}
