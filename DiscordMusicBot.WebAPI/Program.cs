using DiscordMusicBot.Worker;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<Worker>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

app.MapPost("/api/bot/start", async (Worker worker, [FromBody] string token) => await worker.StartBot(token));
app.MapPost("/api/bot/stop", async (Worker worker) => await worker.StopBot());

app.MapPost("/api/bot/join", async (Worker worker, [FromBody] ulong channelId) => await worker.JoinChannel(channelId));
app.MapPost("/api/bot/leave", async (Worker worker) => await worker.LeaveChannel());

app.MapGet("/api/bot/servers", async (Worker worker) => Results.Json(await worker.ListConnectedServer()));
app.MapGet("/api/bot/channels/{serverId}", async (Worker worker, string serverId) => await worker.ListConnectedChannel(serverId));

app.MapGet("/api/bot/status", (Worker worker) => worker.GetStatus());


app.Run();