// Program.cs
using Malach.Server.Abstractions;
using Malach.Server.Common.Models;
using Malach.Server.Common;
using Malach.Server.Platforms.Windows;
using Malach.Server.Platforms.Mac;
using Malach.Server.Platforms.Linux;

var builder = WebApplication.CreateBuilder(args);

// Use exact property names from payload (key/type/modifiers/msg/password)
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = null;
});

// Platform-specific registrations
if (OperatingSystem.IsWindows())
{
    builder.Services.AddSingleton<IKeyboard, WindowsKeyboard>();
    builder.Services.AddSingleton<IMouse, WindowsMouse>();
    builder.Services.AddSingleton<IWindowManager, WindowsWindowManager>();
    builder.Services.AddSingleton<IFileActions, WindowsFileActions>();
}
else if (OperatingSystem.IsMacOS())
{
    builder.Services.AddSingleton<IKeyboard, MacKeyboard>();
    builder.Services.AddSingleton<IMouse, MacMouse>();
    builder.Services.AddSingleton<IWindowManager, MacWindowManager>();
    builder.Services.AddSingleton<IFileActions, MacFileActions>();
}
else
{
    builder.Services.AddSingleton<IKeyboard, LinuxKeyboard>();
    builder.Services.AddSingleton<IMouse, LinuxMouse>();
    builder.Services.AddSingleton<IWindowManager, LinuxWindowManager>();
    builder.Services.AddSingleton<IFileActions, LinuxFileActions>();
}

// Cross-platform utility
builder.Services.AddSingleton<IUtility, Utility>();

// Router
builder.Services.AddSingleton<Routing.RequestRouter>();

var app = builder.Build();

// Optional bearer token auth (set Server:BearerToken in appsettings.json)
var serverAuthToken = app.Configuration["Server:BearerToken"] ?? "";
app.Use(async (ctx, next) =>
{
    if (!string.IsNullOrWhiteSpace(serverAuthToken))
    {
        var auth = ctx.Request.Headers.Authorization.ToString();
        if (!auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ||
            auth["Bearer ".Length..].Trim() != serverAuthToken)
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await ctx.Response.WriteAsync("Unauthorized");
            return;
        }
    }
    await next();
});

// Single endpoint compatible with your Companion payloads
app.MapPost("/api/keys", (VicReoRequest req, Routing.RequestRouter router) =>
    router.Handle(req) ? Results.Ok(new { ok = true }) : Results.BadRequest(new { ok = false })
);

app.Run();
