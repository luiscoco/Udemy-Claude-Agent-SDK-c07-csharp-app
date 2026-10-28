using Anthropic;
using Anthropic.Models.Beta;
using Anthropic.Models.Beta.Sessions;
using Anthropic.Models.Beta.Sessions.Events;

namespace LessonC07.App.Lab;

/// <summary>One piece of lifecycle evidence sent to the UI (same vocabulary as the Node app's NDJSON lines).</summary>
public sealed record LabUpdate(string Type, string Value);

/// <summary>Creates a session, observes its live events and classifies the stop reason.</summary>
public sealed class SessionRunner(LabSettings settings)
{
    public async Task RunAsync(string prompt, Func<LabUpdate, Task> send, CancellationToken ct)
    {
        try
        {
            AnthropicClient client = new() { ApiKey = settings.ApiKey };
            var session = await client.Beta.Sessions.Create(new SessionCreateParams
            {
                Agent = settings.AgentId,
                EnvironmentID = settings.EnvironmentId,
                Budget = new BetaManagedAgentsBudgetLimit
                {
                    MaxListCost = new BetaMonetaryAmount { Amount = "50", Currency = "USD" },
                },
            }, ct);
            await send(new("session", session.ID));

            // Stream first: start reading before sending so early status events are observed live.
            await using var live = client.Beta.Sessions.Events
                .StreamStreaming(session.ID, new EventStreamParams(), ct)
                .GetAsyncEnumerator(ct);
            var next = live.MoveNextAsync();

            await client.Beta.Sessions.Events.Send(session.ID, new EventSendParams
            {
                Events =
                [
                    new BetaManagedAgentsUserMessageEventParams
                    {
                        Content = [new BetaManagedAgentsTextBlock { Text = prompt.Trim() }],
                    },
                ],
            }, ct);

            var outcome = Outcome.Incomplete;
            while (await next)
            {
                var ev = live.Current;
                await send(new("event", ev.Json.GetProperty("type").GetString() ?? "unknown"));

                if (ev.TryPickAgentMessageEvent(out var message))
                {
                    foreach (var block in message.Content)
                        if (block.TryPickBetaManagedAgentsTextBlock(out var text)) await send(new("answer", text.Text));
                }
                if (ev.TryPickSessionStatusIdleEvent(out var idle))
                {
                    var reason = idle.StopReason.Json.GetProperty("type").GetString();
                    outcome = OutcomeClassifier.Classify("idle", reason, false);
                    await send(new("reason", reason ?? "unknown"));
                    break;
                }
                if (ev.TryPickSessionStatusTerminatedEvent(out _))
                {
                    outcome = OutcomeClassifier.Classify("terminated", null, false);
                    break;
                }
                if (ev.TryPickSessionErrorEvent(out _))
                {
                    outcome = Outcome.Error;
                    await send(new("error", "Session reported an error."));
                    break;
                }
                next = live.MoveNextAsync();
            }
            await send(new("outcome", outcome));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // The page closed. As in the Node app, this stops observing; it does not stop remote work.
        }
        catch (Exception error)
        {
            await send(new("error", error.Message));
        }
    }
}
