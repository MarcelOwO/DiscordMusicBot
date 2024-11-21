using DiscordMusicBot.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DiscordMusicBot.WebAPI.Endpoints;

public static class MusicEndpoints
{
    public static void Map(WebApplication app, Worker.Worker worker)
    {
        app.MapPost("/api/bot/play", async () => await worker.AudioService.Play());
        app.MapPost("/api/bot/pause",  () =>  worker.AudioService.Pause());

        app.MapPost("/api/bot/volume", async ([FromBody] string volume) => await worker.AudioService.SetVolume(volume));
        app.MapGet("/api/bot/volume", () => worker.AudioService.GetVolume());

        app.MapPost("/api/bot/search", async ([FromBody] string name) => await worker.QueueService.Search(name));

        app.MapPost("/api/bot/add",
            async ([FromBody] SearchResult searchResult) => await worker.QueueService.AddToQueue(searchResult));

        app.MapGet("/api/bot/queue", () => worker.QueueService.GetQueue());
        app.MapPost("/api/bot/queue", ([FromBody] List<WebQueueItem> queue) => worker.QueueService.UpdateQueue(queue));


        app.MapPost("/api/bot/next", async () => await worker.AudioService.NextSong());
        app.MapPost("/api/bot/previous", async () => await worker.AudioService.PreviousSong());
    }
}