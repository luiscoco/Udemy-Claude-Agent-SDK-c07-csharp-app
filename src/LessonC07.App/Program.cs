using LessonC07.App.Components;
using LessonC07.App.Lab;

var envPath = DotEnv.Locate();
DotEnv.Load(envPath);

if (args.Length > 0 && args[0] == "setup")
{
    await ResourceSetup.RunAsync(envPath);
    return;
}

var settings = LabSettings.FromEnvironment();
var builder = WebApplication.CreateBuilder(args);
// Loopback only, like the Node app: credentials stay on this machine's server.
builder.WebHost.UseUrls($"http://127.0.0.1:{settings.Port}");

builder.Services.AddSingleton(settings);
builder.Services.AddTransient<SessionRunner>();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.Use(async (context, next) =>
{
    context.Response.Headers.CacheControl = "no-store";
    await next();
});

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Lifetime.ApplicationStarted.Register(() => Console.WriteLine($"Open http://127.0.0.1:{settings.Port}"));
app.Run();
