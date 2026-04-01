namespace Breach.Core;

/// <summary>A player's personal board with 3 tile slots.</summary>
public sealed class PlayerBoard
{
    private readonly Tile?[] _slots = new Tile?[3];

    public const int Size = 3;

    public Tile? this[int index]
    {
        get => _slots[index];
        set => _slots[index] = value;
    }

    public bool IsSlotEmpty(int index) => _slots[index] is null;

    public PlayerBoard Clone()
    {
        var clone = new PlayerBoard();
        for (var i = 0; i < Size; i++)
            clone._slots[i] = _slots[i];
        return clone;
    }
}
