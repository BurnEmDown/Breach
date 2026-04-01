namespace Breach.Core;

/// <summary>Extension methods for <see cref="TileColor"/> enum.</summary>
public static class TileColorExtensions
{
    /// <summary>
    /// Returns a single-character abbreviation for the tile color, useful for
    /// displaying the board in ASCII format.
    /// </summary>
    /// <param name="color">The color to abbreviate.</param>
    /// <returns>"O" for Orange, "G" for Green, "P" for Purple, "?" for any unknown value.</returns>
    public static string ToAbbrev(this TileColor color) => color switch
    {
        TileColor.Orange => "O",
        TileColor.Green  => "G",
        TileColor.Purple => "P",
        _                => "?"
    };
}
