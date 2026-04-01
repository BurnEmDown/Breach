namespace Breach.Core;

/// <summary>
/// One required colored cell in a goal pattern, expressed as an offset from
/// an anchor position during pattern matching.
/// </summary>
/// <param name="RowOffset">Row offset relative to the anchor.</param>
/// <param name="ColOffset">Column offset relative to the anchor.</param>
/// <param name="Color">Required tile color at the target cell.</param>
public readonly record struct GoalRequirementCell(int RowOffset, int ColOffset, TileColor Color);
