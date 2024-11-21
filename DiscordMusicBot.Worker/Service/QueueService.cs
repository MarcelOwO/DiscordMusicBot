using DiscordMusicBot.Core.Models;
using DiscordMusicBot.Worker.Enums;
using DiscordMusicBot.Worker.Interface;
using DiscordMusicBot.Worker.Services;

namespace DiscordMusicBot.Worker.Service;

public class QueueService(Worker worker)
{
    private IAudioProvider? _audioProvider = new YoutubeProvider();

    public void UpdateQueue(List<WebQueueItem> webQueue)
    {
        worker._queue = webQueue.Select(x => worker._queue.First(y => x.Id == y.Id)).ToList();
    }

    public async Task<List<SearchResult>> Search(string name)
    {
        if (_audioProvider != null)
        {
            var value = await _audioProvider.SearchVideos(name);

            return value;
        }

        return new List<SearchResult>();
    }

    public void SetProvider(AudioProviders provider)
    {
        switch (provider)
        {
            case AudioProviders.Youtube:
                _audioProvider = new YoutubeProvider();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
        }
    }

    public async Task AddToQueue(SearchResult searchResult)
    {
        if (_audioProvider == null) return;

        var queue = await _audioProvider.GetStream(searchResult);

        worker._queue.Add(queue);
    }


    public List<WebQueueItem> GetQueue()
    {
        return (from x in worker._queue select new WebQueueItem() { Id = x.Id, Title = x.Title }).ToList();
    }
}