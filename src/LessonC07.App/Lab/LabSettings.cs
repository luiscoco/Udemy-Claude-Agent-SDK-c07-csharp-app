namespace LessonC07.App.Lab;

/// <summary>Server-side configuration. Values never reach the browser; only <see cref="Configured"/> does.</summary>
public sealed record LabSettings(string ApiKey, string AgentId, string EnvironmentId, int Port)
{
    public static LabSettings FromEnvironment() => new(
        Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? "",
        Environment.GetEnvironmentVariable("CLAUDE_MANAGED_AGENT_ID") ?? "",
        Environment.GetEnvironmentVariable("CLAUDE_MANAGED_ENVIRONMENT_ID") ?? "",
        int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var port) ? port : 3007);

    public static bool IsSet(string? value) => !string.IsNullOrEmpty(value) && !value.StartsWith("your_");

    public bool Configured => IsSet(ApiKey) && IsSet(AgentId) && IsSet(EnvironmentId);
}
