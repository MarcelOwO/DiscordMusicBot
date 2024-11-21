using Microsoft.AspNetCore.Mvc;

namespace DiscordMusicBot.WebAPI.Endpoints;

public static class BotEndpoints
{
    public static void Map(WebApplication app, Worker.Worker worker)
    {
        app.MapPost("/api/bot/start", async ([FromBody] string token) => await worker.BotService.StartBot(token));
        app.MapPost("/api/bot/stop", async () => await worker.BotService.StopBot());

        app.MapPost("/api/bot/join",
            async ([FromBody] ulong channelId) => await worker.BotService.JoinChannel(channelId));
        app.MapPost("/api/bot/leave", () => worker.BotService.LeaveChannel());

        app.MapGet("/api/bot/servers", async () => await worker.BotService.ListConnectedServer());
        app.MapGet("/api/bot/channels/{serverId}",
            async (string serverId) => await worker.BotService.ListConnectedChannel(serverId));

        app.MapGet("/api/bot/status", () => worker.BotService.GetStatus());
    }
}