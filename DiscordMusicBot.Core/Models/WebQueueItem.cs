namespace DiscordMusicBot.Core.Models;

public class WebQueueItem
{
    public string? Title { get; set; }
    public string? Id { get; set; }
    public bool IsTemp { get; set; }

    public WebQueueItem(QueueItem item, bool temp)
    {
        Title = item.Title;
        Id = item.Id;
        IsTemp = temp;
    }

    public WebQueueItem(string? id, string? title, bool temp)
    {
        Title = title;
        Id = id;
        IsTemp = temp;
    }

    public WebQueueItem()
    {
        IsTemp = false;
    }
}