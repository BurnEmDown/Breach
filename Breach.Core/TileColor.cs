namespace Breach.Core;

/// <summary>
/// Represents the three colors available in Breach. Each tile's primary color
/// determines which set it belongs to (5 tiles per color). The orange diagonal
/// forms the opening board layout.
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
