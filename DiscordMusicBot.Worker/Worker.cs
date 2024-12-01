using DiscordMusicBot.Core.Enums;
using DiscordMusicBot.Core.Models;
using DiscordMusicBot.Worker.Service;
using DiscordMusicBot.Worker.Services;

namespace DiscordMusicBot.Worker;

public class Worker : BackgroundService
{
    public List<QueueItem> Queue = new();
    public DSharpPlusWrapper? Bot;

    public readonly BotService BotService;
    public readonly AudioService AudioService;
    public readonly QueueService QueueService;

    public int CurrentQueueIndex => AudioService.CurrentQueueIndex;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }

    public Worker()
    {
        BotService = new BotService(this);
        AudioService = new AudioService(this);
        QueueService = new QueueService(this);

        QueueService.SetProvider(AudioProviders.Youtube);
    }

    public ApiStatus GetStatus()
    {
        ApiStatus status = new()
        {
            IsRunning = true,
            BotIsRunning = Bot is not null,
            QueuedSongs = Queue.Select(x => new WebQueueItem(x, false)).ToList(),
        };

        return status;
    }
}