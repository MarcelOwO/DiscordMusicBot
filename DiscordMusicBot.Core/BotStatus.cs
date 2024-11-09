namespace DiscordMusicBot.Core;

public class BotStatus
{
    public bool IsRunning { get; set; } = false;
    public bool IsConnectedToChannel { get; set; } = false;
    public string ConnectedChannel { get; set; } = string.Empty;

    public List<string> QueuedSongs { get; set; } = new List<string>();
}