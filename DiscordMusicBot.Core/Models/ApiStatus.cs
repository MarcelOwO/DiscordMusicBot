using DiscordMusicBot.Core.Enums;

namespace DiscordMusicBot.Core.Models;

public class ApiStatus
{
    public bool IsRunning { get; set; }
    public bool BotIsRunning { get; set; }
    public PlaybackState PlaybackState { get; set; }
    public bool IsConnectedToChannel { get; set; }
    public string ConnectedChannel { get; set; } = string.Empty;
    public List<WebQueueItem> QueuedSongs { get; set; } = [];
}