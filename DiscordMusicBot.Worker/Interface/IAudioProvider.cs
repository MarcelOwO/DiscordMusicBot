using DiscordMusicBot.Core.Models;

namespace DiscordMusicBot.Worker.Interface;

public interface IAudioProvider
{
    public Task CancelDownload();
    public Task<QueueItem> GetStream(SearchResult query);
    
    public Task<List<SearchResult>> SearchVideos(string search, int searchCount = 5);
}