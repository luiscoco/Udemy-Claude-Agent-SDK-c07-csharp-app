using Anthropic;
using Anthropic.Models.Beta.Agents;
using Anthropic.Models.Beta.Environments;

namespace LessonC07.App.Lab;

/// <summary>Optional one-time provisioning (<c>dotnet run -- setup</c>). Reuses IDs already present in .env.</summary>
public static class ResourceSetup
{
    public static async Task RunAsync(string envPath)
    {
        if (!LabSettings.IsSet(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")))
            throw new InvalidOperationException("Set your own ANTHROPIC_API_KEY in .env first.");

        AnthropicClient client = new() { MaxRetries = 0 };

        if (!LabSettings.IsSet(Environment.GetEnvironmentVariable("CLAUDE_MANAGED_ENVIRONMENT_ID")))
        {
            var environment = await client.Beta.Environments.Create(new EnvironmentCreateParams
            {
                Name = "C07 student environment",
                Config = new BetaCloudConfigParams { Networking = new BetaLimitedNetworkParams() },
            });
            // Save immediately so a later agent failure does not lose the environment ID.
            DotEnv.Save(envPath, "CLAUDE_MANAGED_ENVIRONMENT_ID", environment.ID);
        }

        if (!LabSettings.IsSet(Environment.GetEnvironmentVariable("CLAUDE_MANAGED_AGENT_ID")))
        {
            var model = Environment.GetEnvironmentVariable("CLAUDE_MODEL");
            var agent = await client.Beta.Agents.Create(new AgentCreateParams
            {
                Name = "C07 text-only student agent",
                Model = new BetaManagedAgentsModelConfigParams
                {
                    ID = string.IsNullOrEmpty(model) ? "claude-opus-5-5" : model,
                },
                System = "Answer the student's question clearly and briefly.",
                Tools = [],
                McpServers = [],
                Skills = [],
            });
            DotEnv.Save(envPath, "CLAUDE_MANAGED_AGENT_ID", agent.ID);
        }

        Console.WriteLine($"Prerequisite IDs are saved in {envPath}. Existing configured IDs were reused.");
    }
}
