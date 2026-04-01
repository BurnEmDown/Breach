namespace Breach.Core;

/// <summary>
/// An immutable tile representing a game piece on the main board or player boards.
/// Each tile has exactly one color. There are 5 orange, 5 green, and 5 purple
/// tiles in the full game set.
/// </summary>
/// <param name="Color">The tile's color.</param>
public sealed record Tile(TileColor Color)
{
    /// <summary>
    /// Returns the tile color abbreviation.
    /// </summary>
    public override string ToString() => Color.ToAbbrev();
}
