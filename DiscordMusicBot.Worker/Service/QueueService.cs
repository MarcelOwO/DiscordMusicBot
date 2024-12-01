using DiscordMusicBot.Core.Enums;
using DiscordMusicBot.Core.Models;
using DiscordMusicBot.Worker.Interface;
using DiscordMusicBot.Worker.Services;

namespace DiscordMusicBot.Worker.Service;

public class QueueService(Worker worker)
{
    private IAudioProvider? _audioProvider = new YoutubeProvider();

    public void UpdateQueue(List<WebQueueItem> webQueue)
    {
        worker.Queue = webQueue.Select(x => worker.Queue.First(y => x.Id == y.Id)).ToList();
    }

    public async Task<List<SearchResult>> Search(string name)
    {
        if (_audioProvider == null) return [];

        var value = await _audioProvider.SearchVideos(name);

        return value;
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
        var exitingCopy = worker.Queue.FindAll(x => x.Id == searchResult.Id);

        if (exitingCopy.Count > 0)
        {
            var queueItem = exitingCopy.First();

            worker.Queue.Add(queueItem);
        }
        else
        {
            var queueItem = await _audioProvider.GetStream(searchResult);
            
            if(queueItem == null) return;

            worker.Queue.Add(queueItem);
        }
    }


    public List<WebQueueItem> GetQueue()
    {
        return (from x in worker.Queue select new WebQueueItem(x, false)).ToList();
    }

    public string GetCurrentSong()
    {
        return worker.CurrentQueueIndex.ToString();
    }
}