namespace DiscordMusicBot.Core.Models;

public class QueueItem
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Thumbnail { get; set; }
    public Lazy<Stream> StreamTask { get; set; }
}