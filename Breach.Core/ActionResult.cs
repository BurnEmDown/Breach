namespace Breach.Core;

public sealed class ActionResult
{
    public bool IsSuccess { get; }
    public string? FailureReason { get; }

    private ActionResult(bool isSuccess, string? failureReason)
    {
        IsSuccess     = isSuccess;
        FailureReason = failureReason;
    }

    public static ActionResult Success() => new(true, null);
    public static ActionResult Failure(string reason) => new(false, reason);

    public override string ToString() =>
        IsSuccess ? "OK" : $"FAIL: {FailureReason}";
}
