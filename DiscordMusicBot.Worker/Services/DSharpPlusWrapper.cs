using DiscordMusicBot.Core.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.VoiceNext;

namespace DiscordMusicBot.Worker.Services;

public class DSharpPlusWrapper
{
    private readonly DiscordClient _client;
    private DiscordChannel? _channel;
    private VoiceNextConnection? _connection;
    private VoiceTransmitSink? _transmitSink;

    private readonly BotStatus _status = new();

    public DSharpPlusWrapper(string token)
    {
        var builder = DiscordClientBuilder.CreateDefault(token, DiscordIntents.All)
            .UseVoiceNext(new VoiceNextConfiguration());

        _client = builder.Build();
    }

    public async Task StartBot()
    {
        try
        {
            await _client.ConnectAsync();
        }
        catch (Exception e) when (e is UnauthorizedAccessException or BadRequestException or ServerErrorException)
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
            _channel = await _client.GetChannelAsync(channelId);
        }
        catch (Exception e) when (e is NotFoundException or BadRequestException or ServerErrorException)
        {
            Console.WriteLine(e.Message);
            return;
        }

        _connection = await _channel.ConnectAsync();

        _transmitSink = _connection.GetTransmitSink();

        _status.IsConnectedToChannel = true;
        _status.ConnectedChannel = _channel.Name;
    }

    public void LeaveChannel()
    {
        StopMusic();

        _connection?.Disconnect();

        _status.IsConnectedToChannel = false;
        _status.ConnectedChannel = string.Empty;
    }

    public Task<IEnumerable<DiscordGuild>> ListConnectedServer()
    {
        return Task.FromResult(_client.Guilds.Values);
    }

    public async Task<IEnumerable<DiscordChannel>> ListConnectedChannel(ulong serverId)
    {
        DiscordGuild? server = null;
        DiscordMember? member = null;

        try
        {
            server = await _client.GetGuildAsync(serverId);
        }
        catch (Exception e) when (e is NotFoundException or BadRequestException or ServerErrorException)
        {
            Console.WriteLine(e.Message);
        }


        if (server == null) return new List<DiscordChannel>();

        try
        {
            member ??= await server.GetMemberAsync(_client.CurrentUser.Id);
        }
        catch (ServerErrorException e)
        {
            Console.WriteLine(e);
        }

        if (member == null) return new List<DiscordChannel>();

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

    public async Task PlayMusicAsync(Stream stream, CancellationToken token, ManualResetEventSlim manualResetEventSlim)
    {
        if (_connection == null)
        {
            return;
        }

        if (_transmitSink == null)
        {
            return;
        }

        var buffer = new byte[81920];

        int bytesRead;

        try
        {
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
            {
                manualResetEventSlim.Wait(token);
                await _transmitSink.WriteAsync(buffer, 0, bytesRead, token);
            }
        }
        catch (Exception e) when (e is OperationCanceledException or ObjectDisposedException)
        {
            Console.WriteLine("Stream processing canceled in bot");
            await stream.DisposeAsync();
            
            return;
        }

        await stream.DisposeAsync();
    }

    public void PauseMusic()
    {
        if (_transmitSink == null) return;

        _transmitSink.Pause();
    }

    public async Task ResumeMusic()
    {
        if (_transmitSink == null) return;

        await _transmitSink.ResumeAsync();
    }

    public void StopMusic()
    {
        if (_transmitSink == null) return;

        _transmitSink.Pause();
        _transmitSink.Dispose();
    }


    public void AdjustVolume(int volume)
    {
        if (_transmitSink == null) return;

        _transmitSink.VolumeModifier = volume / 100f;
    }
}