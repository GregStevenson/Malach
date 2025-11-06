using Malach.Core;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ICommandExecutor, CommandExecutor>();

var app = builder.Build();

// Simple bearer token gate (dev default)
var token = builder.Configuration[""Malach:Token""] ?? ""dev-token"";
app.Use(async (ctx, next) =>
{
    if (!ctx.Request.Headers.TryGetValue(""Authorization"", out var auth) || auth.ToString() != $""Bearer {token}"")
    { ctx.Response.StatusCode = 401; return; }
    await next();
});

app.MapPost(""/v1/launch"", ([FromBody] LaunchReq req, ICommandExecutor exec) =>
{
    exec.Launch(req.Exe, req.Args);
    return Results.Ok(new { ok = true });
});

app.MapPost(""/v1/open"", ([FromBody] OpenReq req, ICommandExecutor exec) =>
{
    exec.Open(req.Path);
    return Results.Ok(new { ok = true });
});

app.Run(""http://127.0.0.1:5123"");

record LaunchReq(string Exe, string? Args);
record OpenReq(string Path);
