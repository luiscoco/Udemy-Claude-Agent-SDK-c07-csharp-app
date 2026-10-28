using LessonC07.App.Lab;

namespace LessonC07.Tests;

public class OutcomeClassifierTests
{
    [Theory]
    [InlineData("idle", "end_turn", false, "completed")]
    [InlineData("idle", "end_turn", true, "interrupted")]
    [InlineData("idle", "budget_reached", false, "paused-budget")]
    [InlineData("idle", "requires_action", false, "requires-action")]
    [InlineData("idle", "unknown", false, "error")]
    [InlineData("rescheduling", null, true, "incomplete")]
    [InlineData("terminated", null, false, "terminated")]
    public void IdleNeedsAReasonAndInterruptionContext(string status, string? reason, bool interrupted, string expected) =>
        Assert.Equal(expected, OutcomeClassifier.Classify(status, reason, interrupted));
}
