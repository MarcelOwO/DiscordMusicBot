using DiscordMusicBot.Core;
using Newtonsoft.Json;

namespace DiscordMusicBot.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    private Core.DiscordMusicBot _bot;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Application");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    public async Task StartBot(string token)
    {
        if (_bot == null)
        {
            _bot = new Core.DiscordMusicBot(token);
        }

        await _bot.StartBot();
    }

    public async Task StopBot()
    {
        await _bot.StopBot();
    }

    public async Task JoinChannel(ulong channelId)
    {
        await _bot.JoinChannel(channelId);
    }

    public async Task LeaveChannel()
    {
        await _bot.LeaveChannel();
    }

    public async Task<List<ServerData>> ListConnectedServer()
    {
        var servers = await _bot.ListConnectedServer();
        return servers.Select(x => new ServerData(x.Name, x.Id)).ToList();
    }

    public async Task<List<ChannelData>> ListConnectedChannel(string serverId)
    {
        var a = await _bot.ListConnectedChannel(ulong.Parse(serverId));

        return a.Select(x => new ChannelData(x.Name, x.Id.ToString())).ToList();
    }

    public class ServerData(string name, ulong id)
    {
        [JsonProperty("name")] public string Name { get; set; } = name;
        [JsonProperty("id")] public string ID { get; set; } = id.ToString();
    }

    public class ChannelData(string name, string id)
    {
        [JsonProperty("name")] public string Name { get; set; } = name;
        [JsonProperty("id")] public string ID { get; set; } = id;
    }

    public BotStatus GetStatus()
    {
        if (_bot == null)
        {
            return new BotStatus();
        }

        return _bot.GetStatus();
    }
}