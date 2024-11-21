using DiscordMusicBot.Core.Models;
using DiscordMusicBot.Worker.Interface;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using YoutubeExplode;
using YoutubeExplode.Search;

namespace DiscordMusicBot.Worker.Services;

public class YoutubeProvider : IAudioProvider
{
    private YoutubeDL _YoutubeDl = new();

    private CancellationTokenSource _CancellationTokenSource = new();

    private IProgress<DownloadProgress> _downloadProgress = new Progress<DownloadProgress>();

    private YoutubeClient _youtube = new();

    private readonly Dictionary<string, VideoSearchResult> _lastSearchResults = new();

    public YoutubeProvider()
    {
        _YoutubeDl.OutputFolder = Path.Combine(Path.GetTempPath(), "DiscordMusicBot");
    }

    public async Task CancelDownload()
    {
        await _CancellationTokenSource.CancelAsync();
    }

    public async Task<QueueItem> GetStream(SearchResult query)
    {
        var token = _CancellationTokenSource.Token;
        Console.WriteLine($"querry url: {query.Url}");

        var result = await _YoutubeDl.RunAudioDownload(query.Url, AudioConversionFormat.Opus, token, _downloadProgress);

        if (result.Success)
        {
            var path = result.Data as string;
            return GetData(path, query);
        }
        else
        {
            var errors = result.ErrorOutput;
            errors.ToList().ForEach(Console.WriteLine);
        }

        Console.WriteLine("Error downloading audio");
        return null;
    }


    private QueueItem GetData(string path, SearchResult query)
    {
        FileInfo fileInfo = new(path);

        if (!fileInfo.Exists)
        {
            Console.WriteLine("File does not exist");
            return null;
        }

        try
        {
            var stream = new Lazy<Stream>(fileInfo.OpenRead);
            return new QueueItem()
            {
                Id = query.Id,
                Title = query.Title,
                Thumbnail = query.Thumbnail?[0].ToString(),
                StreamTask = stream,
            };
        }
        catch (Exception e) when (e is UnauthorizedAccessException or DirectoryNotFoundException or IOException)
        {
            Console.WriteLine("Error opening file");
            Console.WriteLine(e.Message);

            return null;
        }
    }


    public async Task<List<SearchResult>> SearchVideos(string search, int searchCount = 5)
    {
        List<VideoSearchResult> searchResults = new List<VideoSearchResult>();

        int counter = 0;
        await foreach (var video in _youtube.Search.GetVideosAsync(search))
        {
            searchResults.Add(video);
            counter++;
            if (counter == searchCount)
            {
                break;
            }
        }

        _lastSearchResults.Clear();

        foreach (var videoSearchResult in searchResults)
        {
            _lastSearchResults.Add(videoSearchResult.Id, videoSearchResult);
        }

        return searchResults.Select(x => new SearchResult
            { Id = x.Id, Title = x.Title, Thumbnail = x.Thumbnails[0].Url, Url = x.Url }).ToList();
    }
}