using DSharpPlus;
using DSharpPlus.Entities;

namespace DiscordMusicBot.Core;

public class DiscordMusicBot
{
    private DiscordClient _client;

    public DiscordMusicBot(string token)
    {
        DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(token, DiscordIntents.AllUnprivileged);
        _client = builder.Build();
    }

    public async Task StartBot()
    {
        if (_client == null) return;

        await _client.ConnectAsync();
    }

    public async Task StopBot()
    {
        await _client.DisconnectAsync();
    }
    
}