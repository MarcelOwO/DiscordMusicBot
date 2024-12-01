using DiscordMusicBot.Core.Models;
using DiscordMusicBot.Worker.Services;
using DSharpPlus.Entities;

namespace DiscordMusicBot.Worker.Service;

public class BotService(Worker worker)
{
    public async Task StartBot(string token)
    {
        worker.Bot ??= new DSharpPlusWrapper(token);

        await worker.Bot.StartBot();
    }

    public async Task StopBot()
    {
        if (worker.Bot == null) return;

        await worker.Bot.StopBot();
    }

    public async Task JoinChannel(ulong channelId)
    {
        if (worker.Bot == null) return;

        await worker.Bot.JoinChannel(channelId);
    }

    public void LeaveChannel()
    {
        worker.Bot?.LeaveChannel();
    }

    public async Task<List<ServerData>> ListConnectedServer()
    {
        if (worker.Bot == null)
        {
            Console.WriteLine("Bot is not running");
            return new List<ServerData>();
        }

        var servers = await worker.Bot.ListConnectedServer();
        var discordGuilds = servers as DiscordGuild[] ?? servers.ToArray();

        if (!discordGuilds.Any())
        {
            Console.WriteLine("No servers connected");
        }

        return discordGuilds.Select(x => new ServerData(x.Id.ToString(), x.Name)).ToList();
    }

    public async Task<List<ChannelData>> ListConnectedChannel(string serverId)
    {
        if (worker.Bot == null) return [];
        
        var a = await worker.Bot.ListConnectedChannel(ulong.Parse(serverId));

        return a.Select(x => new ChannelData(x.Id.ToString(), x.Name)).ToList();
    }

    public ApiStatus GetStatus()
    {
        return worker.Bot == null ? new ApiStatus() : worker.Bot.GetStatus();
    }
}