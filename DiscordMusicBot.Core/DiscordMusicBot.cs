using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.VoiceNext;

namespace DiscordMusicBot.Core;

public class DiscordMusicBot
{
    private DiscordClient _client;
    private DiscordChannel? channel;
    private VoiceNextConnection? connection;

    private BotStatus _status;

    public DiscordMusicBot(string token)
    {
        var builder = DiscordClientBuilder.CreateDefault(token, DiscordIntents.AllUnprivileged)
            .UseVoiceNext(new VoiceNextConfiguration());

        _client = builder.Build();

        _status = new BotStatus();
    }

    public async Task StartBot()
    {
        try
        {
            await _client.ConnectAsync();
        }
        catch (UnauthorizedAccessException e)
        {
            Console.WriteLine(e.Message);
            return;
        }
        catch (BadRequestException e)
        {
            Console.WriteLine(e.Message);
            return;
        }
        catch (ServerErrorException e)
        {
            Console.WriteLine(e.Message);
            return;
        }

        _status.IsRunning = true;
    }

    public async Task StopBot()
    {
        await _client.DisconnectAsync();
        _status.IsRunning = false;
    }

    public async Task JoinChannel(ulong channelId)
    {
        try
        {
            channel = await _client.GetChannelAsync(channelId);
        }
        catch (NotFoundException e)
        {
            Console.WriteLine(e.Message);
            return;
        }
        catch (BadRequestException e)
        {
            Console.WriteLine(e.Message);
            return;
        }
        catch (ServerErrorException e)
        {
            Console.WriteLine(e.Message);
            return;
        }

        connection = await channel.ConnectAsync();

        _status.IsConnectedToChannel = true;
        _status.ConnectedChannel = channel.Name;
    }

    public async Task LeaveChannel()
    {
        connection?.Disconnect();

        _status.IsConnectedToChannel = false;
        _status.ConnectedChannel = string.Empty;
    }

    public Task<IEnumerable<DiscordGuild>> ListConnectedServer()
    {
        return Task.FromResult(_client.Guilds.Values);
    }

    public async Task<IEnumerable<DiscordChannel>> ListConnectedChannel(ulong serverId)
    {
       Console.WriteLine(serverId);
        
        
        DiscordGuild? server = null;
        DiscordMember? member = null;

        try
        {
            server = await _client.GetGuildAsync(serverId);
            Console.WriteLine(server.Name);
        }
        catch (NotFoundException e)
        {
            Console.WriteLine(e.Message);
        }
        catch (BadRequestException e)
        {
            Console.WriteLine(e.Message);
        }
        catch (ServerErrorException e)
        {
            Console.WriteLine(e.Message);
        }
        
        if(server == null) return new List<DiscordChannel>();

        try
        {
            member ??= await server.GetMemberAsync(_client.CurrentUser.Id);
            
            Console.WriteLine(member.Nickname);
        }
        catch (ServerErrorException e)
        {
            Console.WriteLine(e);
        }
        if(member == null) return new List<DiscordChannel>();
        
        return from channel in server.Channels
            where channel.Value.Type == DiscordChannelType.Voice &&
                  channel.Value.PermissionsFor(member)
                      .HasPermission(DiscordPermissions.Speak | DiscordPermissions.UseVoice)
            select channel.Value;
    }

    public BotStatus GetStatus()
    {
        return _status;
    }
}