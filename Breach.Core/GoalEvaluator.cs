namespace Breach.Core;

/// <summary>
/// Evaluates goal tile completion against the main board.
/// </summary>
public static class GoalEvaluator
{
    public static bool IsGoalSatisfied(Board board, GoalTile goal)
    {
        for (var anchorRow = 0; anchorRow < 3; anchorRow++)
        {
            for (var anchorCol = 0; anchorCol < 3; anchorCol++)
            {
                if (MatchesAtAnchor(board, goal, anchorRow, anchorCol))
                    return true;
            }
        }

        return false;
    }

    public static IReadOnlyList<GoalTile> GetSatisfiedGoals(Player player, Board board)
    {
        return player.GoalTiles.Where(goal => IsGoalSatisfied(board, goal)).ToArray();
    }

    private static bool MatchesAtAnchor(Board board, GoalTile goal, int anchorRow, int anchorCol)
    {
        foreach (var requirement in goal.Requirements)
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
