using DiscordMusicBot.Core.Models;
using DiscordMusicBot.Worker.Services;
using DSharpPlus.Entities;

namespace DiscordMusicBot.Worker.Service;

public class BotService(Worker worker)
{
    public async Task StartBot(string token)
    {
        worker._bot ??= new DSharpPlusWrapper(token);

        await worker._bot.StartBot();
    }

    public async Task StopBot()
    {
        if (worker._bot == null) return;

        await worker._bot.StopBot();
    }

    public async Task JoinChannel(ulong channelId)
    {
        if (worker._bot == null) return;

        await worker._bot.JoinChannel(channelId);
    }

    public void LeaveChannel()
    {
        worker._bot?.LeaveChannel();
    }

    public async Task<List<ServerData>> ListConnectedServer()
    {
        if (worker._bot == null)
        {
            Console.WriteLine("Bot is not running");
            return new List<ServerData>();
        }

        var servers = await worker._bot.ListConnectedServer();
        var discordGuilds = servers as DiscordGuild[] ?? servers.ToArray();

        if (!discordGuilds.Any())
        {
            Console.WriteLine("No servers connected");
        }

        return discordGuilds.Select(x => new ServerData(x.Id.ToString(), x.Name)).ToList();
    }

    public async Task<List<ChannelData>> ListConnectedChannel(string serverId)
    {
        if (worker._bot == null) return new List<ChannelData>();
        var a = await worker._bot.ListConnectedChannel(ulong.Parse(serverId));

        return a.Select(x => new ChannelData(x.Id.ToString(), x.Name)).ToList();
    }

    public BotStatus GetStatus()
    {
        return worker._bot == null ? new BotStatus() : worker._bot.GetStatus();
    }
}