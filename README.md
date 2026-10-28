# C07 · Session Outcomes and Stop Reasons (Blazor / .NET 10)

C# port of `../lesson-c07-app` (Node + `@anthropic-ai/sdk`). The app runs a real Managed Agents session and classifies the stop reason it observes. It shows pauses but does not implement approvals, resume, or remote interrupt cleanup.

Built with Blazor Web App (interactive server rendering) and the official [`Anthropic`](https://www.nuget.org/packages/Anthropic) NuGet package, version 13.3.0.

## Concepts students will learn

- Idle is not always completed
- Classify pauses, termination and incomplete streams
- Keep event evidence alongside an answer

## Layout

| Path | Purpose | Node equivalent |
|---|---|---|
| `src/LessonC07.App/Lab/OutcomeClassifier.cs` | Classifies the lifecycle status and stop reason together | `src/outcome.ts` |
| `src/LessonC07.App/Lab/SessionRunner.cs` | Creates the session, opens the stream, sends the prompt, classifies the outcome | `/api/run` in `src/server.ts` |
| `src/LessonC07.App/Lab/ResourceSetup.cs` | Optional one-time setup of the environment and agent (`dotnet run -- setup`) | `scripts/setup-resources.mts` |
| `src/LessonC07.App/Lab/DotEnv.cs`, `LabSettings.cs` | Load `.env` and keep credentials on the server | `process.loadEnvFile`, `configured` |
| `src/LessonC07.App/Components/Pages/Home.razor` | Lab UI (interactive server) | `public/index.html` + `public/app.js` |
| `Components/Pages/Guide.razor`, `Architecture.razor` | Student guide and walkthrough | `public/guide.html`, `public/architecture.html` |
| `tests/LessonC07.Tests` | xUnit tests for the classifier | `src/outcome.test.ts` |

The Node app streamed NDJSON from `POST /api/run` to a browser parser. Here the `Home` component runs on the server and updates the page over the Blazor SignalR circuit, so there is no public HTTP API. The browser only learns whether the app is configured. Kestrel listens on `127.0.0.1` only.

## Configure and run

You need the .NET 10 SDK. In PowerShell, from this folder:

```powershell
dotnet --version
dotnet restore
Copy-Item .env.example .env
notepad .env
```

```dotenv
ANTHROPIC_API_KEY=your_api_key
CLAUDE_MANAGED_AGENT_ID=your_agent_id
CLAUDE_MANAGED_ENVIRONMENT_ID=your_environment_id
PORT=3007
```

The app looks for `.env` in the current directory and its parents, up to the folder that contains `LessonC07.slnx`. Real environment variables take precedence over `.env`. Restart the server after editing the file.

If you don't have resource IDs yet, run the optional setup. It creates the environment before a text-only agent, saves each ID to `.env` as soon as it exists, and reuses IDs that are already configured. It uses `CLAUDE_MODEL`, or `claude-opus-5-5` if that's empty.

```powershell
dotnet run --project src/LessonC07.App -- setup
```

Start the app and open `http://127.0.0.1:3007` (or the port set in `PORT`):

```powershell
dotnet run --project src/LessonC07.App
```

Routes: `/` (lab, `#application-lab`), `/guide#learning`, `/guide#run-guide`, and `/architecture`.

## Validation

```powershell
dotnet test
```

## How the app was built

1. **Stream first.** `SessionRunner` creates the session, starts `client.Beta.Sessions.Events.StreamStreaming(...)`, and only then sends the `user.message`, so early status events are seen live.
2. **Classify on idle.** On `session.status_idle`, the stop reason's `type` (`end_turn`, `requires_action`, `budget_reached`, and so on) goes to `OutcomeClassifier.Classify`. `session.status_terminated` and `session.error` end the observation with their own outcomes.
3. **Closing the page stops observing.** When the component is disposed, the stream is cancelled. As in the Node app, the remote session is not interrupted.
