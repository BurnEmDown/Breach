namespace Breach.Core;

/// <summary>
/// Represents the three colors available in Breach. Each tile has exactly one
/// of these colors, with 5 tiles per color in the full game set.
/// </summary>
public enum TileColor
{
    /// <summary>Orange tile color — forms the diagonal line on the opening board.</summary>
    Orange,
    
    /// <summary>Green tile color — distributed 2 one side, 1 on the other of the diagonal.</summary>
    Green,
    
    /// <summary>Purple tile color — distributed 1 one side, 2 on the other of the diagonal.</summary>
    Purple
}
