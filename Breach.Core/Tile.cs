namespace Breach.Core;

/// <summary>
/// An immutable tile with three colors. <see cref="Primary"/> identifies which
/// set (Orange/Green/Purple) this tile belongs to; the two secondary colors
/// appear on its face for gameplay purposes (exact distribution TBD).
/// </summary>
public sealed record Tile(TileColor Primary, TileColor Secondary1, TileColor Secondary2)
{
    public override string ToString() =>
        $"{Primary.ToAbbrev()}/{Secondary1.ToAbbrev()}{Secondary2.ToAbbrev()}";
}
