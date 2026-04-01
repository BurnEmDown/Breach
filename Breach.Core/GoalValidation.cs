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

        var colorCounts = requirements
            .GroupBy(r => r.Color)
            .ToDictionary(g => g.Key, g => g.Count());

        if (colorCounts.Values.Any(count => count >= 3))
            throw new ArgumentException("A goal cannot require 3 or more of the same color.", nameof(requirements));
    }
}
