namespace Breach.Core;

/// <summary>
/// An immutable tile representing a game piece on the main board or player boards.
/// Each tile has one primary color (which determines its set: 5 orange, 5 green, 5 purple)
/// and two secondary colors that appear on its face for thematic purposes.
/// Tiles move between board positions and player boards via game actions (Move, Switch, Override).
/// </summary>
/// <param name="Primary">The primary color identifying this tile's set.</param>
/// <param name="Secondary1">First secondary color on the tile face.</param>
/// <param name="Secondary2">Second secondary color on the tile face.</param>
public sealed record Tile(TileColor Primary, TileColor Secondary1, TileColor Secondary2)
{
    /// <summary>
    /// Returns a diagnostic string showing all three colors in abbreviated form.
    /// Example: "O/GP" (Orange primary, Green and Purple secondary).
    /// </summary>
    public override string ToString() =>
        $"{Primary.ToAbbrev()}/{Secondary1.ToAbbrev()}{Secondary2.ToAbbrev()}";
}
