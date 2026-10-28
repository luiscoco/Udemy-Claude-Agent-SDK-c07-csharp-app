namespace LessonC07.App.Lab;

/// <summary>Lifecycle outcomes shown to the student. Values match the original Node lesson.</summary>
public static class Outcome
{
    public const string Completed = "completed";
    public const string Interrupted = "interrupted";
    public const string PausedBudget = "paused-budget";
    public const string RequiresAction = "requires-action";
    public const string Error = "error";
    public const string Terminated = "terminated";
    public const string Incomplete = "incomplete";
}

public static class OutcomeClassifier
{
    /// <summary>Classify lifecycle state and stop reason together; idle alone does not mean completed.</summary>
    public static string Classify(string status, string? reason, bool interrupted)
    {
        if (status == "terminated") return Outcome.Terminated;
        if (status != "idle") return Outcome.Incomplete;
        if (reason == "requires_action") return Outcome.RequiresAction;
        if (reason == "budget_reached") return Outcome.PausedBudget;
        if (reason == "end_turn") return interrupted ? Outcome.Interrupted : Outcome.Completed;
        return Outcome.Error;
    }
}
