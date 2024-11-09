using System.Threading.Channels;

namespace DiscordMusicBot.Core;

public class ChannelManager
{
    public static ChannelManager Instance { get; } = new ChannelManager();

    private readonly Channel<string> _statusChannel;
    private readonly Channel<string> _controlChannel;

    public ChannelManager()
    {
        _statusChannel = Channel.CreateUnbounded<string>();
        _controlChannel = Channel.CreateUnbounded<string>();
    }



    public async Task StatusData(string data)
    {
        await _statusChannel.Writer.WriteAsync(data);
    }
    
    
}




