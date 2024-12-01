using System.Security;
using DiscordMusicBot.Core.Models;
using DiscordMusicBot.Worker.Interface;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;
using YoutubeExplode;
using YoutubeExplode.Search;

namespace DiscordMusicBot.Worker.Services;

public class YoutubeProvider : IAudioProvider
{
    private readonly YoutubeDL _youtubeDl = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly IProgress<DownloadProgress> _downloadProgress = new Progress<DownloadProgress>();
    private readonly YoutubeClient _youtube = new();

    public YoutubeProvider()
    {
        try
        {
            _youtubeDl.OutputFolder = Path.Combine(Path.GetTempPath(), "DiscordMusicBot");
        }
        catch (Exception e) when (e is ArgumentException or ArgumentNullException or SecurityException)
        {
            Console.WriteLine("Failed creating temp folder");
            throw;
        }
    }

    public async Task CancelDownload()
    {
        try
        {
            await _cancellationTokenSource.CancelAsync();
        }
        catch (Exception e) when (e is ObjectDisposedException)
        {
            Console.WriteLine("Failed cancelling download");
        }
    }

    public async Task<QueueItem?> GetStream(SearchResult query)
    {
        try
        {
            var token = _cancellationTokenSource.Token;


            var result =
                await _youtubeDl.RunAudioDownload(query.Url, AudioConversionFormat.Opus, token, _downloadProgress);

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
        }
        catch (Exception e) when (e is ObjectDisposedException)
        {
            Console.WriteLine("Failed getting token for youtube stream download");
            return null;
        }

        return null;
    }


    private QueueItem? GetData(string path, SearchResult query)
    {


        if (!File.Exists(path))
        {
            Console.WriteLine("File does not exist");
            return null;
        }

        try
        {

            return new QueueItem()
            {
                Id = query.Id,
                Title = query.Title,
                Thumbnail = query.Thumbnail?[0].ToString(),
                filePath = path,
            };
        }
        catch (Exception e) when (e is UnauthorizedAccessException or DirectoryNotFoundException or IOException)
        {
            Console.WriteLine("Error getting file stream from youtube download");

            return null;
        }
    }


    public async Task<List<SearchResult>> SearchVideos(string search, int searchCount = 5)
    {
        List<VideoSearchResult> searchResults = [];

        var counter = 0;
        await foreach (var video in _youtube.Search.GetVideosAsync(search))
        {
            searchResults.Add(video);
            counter++;
            if (counter == searchCount)
            {
                break;
            }
        }

        return searchResults.Select(x => new SearchResult
            { Id = x.Id, Title = x.Title, Thumbnail = x.Thumbnails[0].Url, Url = x.Url }).ToList();
    }
}