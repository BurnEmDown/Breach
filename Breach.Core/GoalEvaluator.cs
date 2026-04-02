namespace Breach.Core;

/// <summary>
/// Evaluates goal tile completion against the main board.
/// Player Two evaluates goals from the opposite side of the board,
/// which is equivalent to rotating the goal card 180 degrees.
/// </summary>
public static class GoalEvaluator
{
    public static bool IsGoalSatisfied(Board board, GoalTile goal) =>
        IsGoalSatisfied(board, goal, PlayerId.One);

    public static bool IsGoalSatisfied(Board board, GoalTile goal, PlayerId playerId)
    {
        var requirements = GetRequirementsForPlayer(goal, playerId);

        for (var anchorRow = 0; anchorRow < 3; anchorRow++)
        {
            for (var anchorCol = 0; anchorCol < 3; anchorCol++)
            {
                if (MatchesAtAnchor(board, requirements, anchorRow, anchorCol))
                    return true;
            }
        }

        return false;
    }

    public static IReadOnlyList<GoalTile> GetSatisfiedGoals(Player player, Board board)
    {
        return player.GoalTiles
            .Where(goal => IsGoalSatisfied(board, goal, player.Id))
            .ToArray();
    }

    private static IReadOnlyList<GoalRequirementCell> GetRequirementsForPlayer(GoalTile goal, PlayerId playerId)
    {
        if (playerId == PlayerId.One)
            return goal.Requirements;

        var minRow = goal.Requirements.Min(r => r.RowOffset);
        var maxRow = goal.Requirements.Max(r => r.RowOffset);
        var minCol = goal.Requirements.Min(r => r.ColOffset);
        var maxCol = goal.Requirements.Max(r => r.ColOffset);

        return goal.Requirements
            .Select(requirement => new GoalRequirementCell(
                maxRow + minRow - requirement.RowOffset,
                maxCol + minCol - requirement.ColOffset,
                requirement.Color))
            .ToArray();
    }

    private static bool MatchesAtAnchor(
        Board board,
        IReadOnlyList<GoalRequirementCell> requirements,
        int anchorRow,
        int anchorCol)
    {
        foreach (var requirement in requirements)
        {
            var row = anchorRow + requirement.RowOffset;
            var col = anchorCol + requirement.ColOffset;

            if (row < 0 || row > 2 || col < 0 || col > 2)
                return false;

            var tile = board[row, col];
            if (tile is null || tile.Color != requirement.Color)
                return false;
        }

        return true;
    }
}
