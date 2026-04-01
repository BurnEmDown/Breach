namespace Breach.Core;

public static class TileColorExtensions
{
    public static string ToAbbrev(this TileColor color) => color switch
    {
        TileColor.Orange => "O",
        TileColor.Green  => "G",
        TileColor.Purple => "P",
        _                => "?"
    };
}
