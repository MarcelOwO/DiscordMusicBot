using DiscordMusicBot.Core.Models;
using DiscordMusicBot.Worker.Enums;
using DiscordMusicBot.Worker.Service;
using DiscordMusicBot.Worker.Services;

namespace DiscordMusicBot.Worker;

public class Worker : BackgroundService
{
    public List<QueueItem> _queue = new();
    public DSharpPlusWrapper? _bot;

    public readonly BotService BotService;
    public readonly AudioService AudioService;
    public readonly QueueService QueueService;

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
}