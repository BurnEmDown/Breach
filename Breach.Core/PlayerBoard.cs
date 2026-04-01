namespace Breach.Core;

/// <summary>
/// A player's personal board that holds up to 3 tiles. These are tiles the player
/// has set aside and can swap in via the Override action. Each player board slot
/// can be empty (null) or contain a tile.
/// </summary>
public sealed class PlayerBoard
{
    private readonly Tile?[] _slots = new Tile?[3];

    /// <summary>The number of tile slots on each player board (always 3).</summary>
    public const int Size = 3;

    /// <summary>Gets or sets the tile at the specified slot index (0-2).</summary>
    /// <param name="index">Slot index (0-2).</param>
    public Tile? this[int index]
    {
        get => _slots[index];
        set => _slots[index] = value;
    }

    /// <summary>Returns true if the specified player board slot is empty.</summary>
    /// <param name="index">Slot index (0-2).</param>
    public bool IsSlotEmpty(int index) => _slots[index] is null;

    /// <summary>
    /// Creates and returns a deep copy of this player board, including all tile slots.
    /// </summary>
    public PlayerBoard Clone()
    {
        var clone = new PlayerBoard();
        for (var i = 0; i < Size; i++)
            clone._slots[i] = _slots[i];
        return clone;
    }
}
