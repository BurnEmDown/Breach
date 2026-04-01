namespace Breach.Core;

/// <summary>
/// Represents the outcome of executing a game action. If <see cref="IsSuccess"/>
/// is true, the action was valid and applied to the game state. Otherwise,
/// <see cref="FailureReason"/> explains why the action was rejected.
/// </summary>
public sealed class ActionResult
{
    /// <summary>True if the action was executed successfully; false if rejected.</summary>
    public bool IsSuccess { get; }
    
    /// <summary>
    /// If <see cref="IsSuccess"/> is false, contains a human-readable reason why
    /// the action failed (e.g., "Not enough action points", "Position out of bounds").
    /// Otherwise, null.
    /// </summary>
    public string? FailureReason { get; }

    private ActionResult(bool isSuccess, string? failureReason)
    {
        IsSuccess     = isSuccess;
        FailureReason = failureReason;
    }

    /// <summary>Creates a success result.</summary>
    public static ActionResult Success() => new(true, null);
    
    /// <summary>Creates a failure result with the given reason.</summary>
    /// <param name="reason">Explanation of why the action failed.</param>
    public static ActionResult Failure(string reason) => new(false, reason);

    /// <summary>Returns "OK" if successful, or "FAIL: {reason}" if failed.</summary>
    public override string ToString() =>
        IsSuccess ? "OK" : $"FAIL: {FailureReason}";
}
