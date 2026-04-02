namespace Breach.Core;

/// <summary>
/// Validation rules for goal tiles and their pattern requirements.
/// </summary>
public static class GoalValidation
{
    public static void Validate(GoalLevel level, IReadOnlyList<GoalRequirementCell> requirements)
    {
        if (requirements.Count == 0)
            throw new ArgumentException("A goal tile must contain at least one requirement.", nameof(requirements));

        var expectedCount = level switch
        {
            GoalLevel.Level1 => 3,
            GoalLevel.Level2 => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(level), "Unsupported goal level.")
        };

        if (requirements.Count != expectedCount)
            throw new ArgumentException(
                $"{level} goals must contain exactly {expectedCount} requirements.",
                nameof(requirements));

        var seenOffsets = new HashSet<(int RowOffset, int ColOffset)>();
        foreach (var requirement in requirements)
        {
            if (Math.Abs(requirement.RowOffset) > 2 || Math.Abs(requirement.ColOffset) > 2)
                throw new ArgumentException(
                    "Requirement offsets must be within -2..2 for the 3x3 board.",
                    nameof(requirements));

            if (!seenOffsets.Add((requirement.RowOffset, requirement.ColOffset)))
                throw new ArgumentException("Goal requirements cannot reuse the same relative cell.", nameof(requirements));
        }

        foreach (var requirement in requirements)
        {
            var hasAdjacentNeighbor = requirements.Any(other =>
                !(other.RowOffset == requirement.RowOffset && other.ColOffset == requirement.ColOffset)
                && Math.Abs(other.RowOffset - requirement.RowOffset) <= 1
                && Math.Abs(other.ColOffset - requirement.ColOffset) <= 1);

            if (!hasAdjacentNeighbor)
                throw new ArgumentException(
                    "Each required tile must be adjacent (orthogonally or diagonally) to at least one other required tile.",
                    nameof(requirements));
        }

        if (level == GoalLevel.Level1 && IsForbiddenLevel1Diagonal(requirements))
            throw new ArgumentException(
                "A Level1 goal cannot use all three tiles of either full board diagonal.",
                nameof(requirements));

        var colorCounts = requirements
            .GroupBy(r => r.Color)
            .ToDictionary(g => g.Key, g => g.Count());

        if (colorCounts.Values.Any(count => count >= 3))
            throw new ArgumentException("A goal cannot require 3 or more of the same color.", nameof(requirements));
    }

    private static bool IsForbiddenLevel1Diagonal(IReadOnlyList<GoalRequirementCell> requirements)
    {
        var rawOffsets = requirements.Select(r => (r.RowOffset, r.ColOffset)).ToArray();
        var minRow = rawOffsets.Min(offset => offset.RowOffset);
        var minCol = rawOffsets.Min(offset => offset.ColOffset);

        var offsets = rawOffsets
            .Select(offset => (offset.RowOffset - minRow, offset.ColOffset - minCol))
            .OrderBy(offset => offset.Item1)
            .ThenBy(offset => offset.Item2)
            .ToArray();

        return offsets.SequenceEqual([(0, 0), (1, 1), (2, 2)])
            || offsets.SequenceEqual([(0, 2), (1, 1), (2, 0)]);
    }
}
