using System.Net.Http.Json;
using DiscordMusicBot.Core.Models;
using Microsoft.AspNetCore.Components.Web;

namespace DiscordMusicBot.WebUI.Services;

public class ApiController
{
    private HttpClient Client { get; set; }

    public bool BotIsOnline;

    public string Token = string.Empty;
    public string ChannelId = string.Empty;
    public string ServerId = string.Empty;
    public string Search = string.Empty;


    public List<ServerData> Servers = new();
    public List<ChannelData> Channels = new();

    public BotStatus Status = new();


    public event Action OnChange;

    public int Volume { get; set; }

    public bool IsPlaying { get; set; } = true;

    public async Task UpdateVolume(int volume)
    {
        var response = await Client.PostAsJsonAsync("/api/bot/volume", Volume.ToString());

        Volume = volume;
    }

    public List<SearchResult> LastSongSeachResult { get; set; } = new();
    public List<WebQueueItem> Queue { get; set; } = new();
    public bool APIIsOnline { get; set; }

    public ApiController(HttpClient client)
    {
        Client = client;

        var timer = new Timer(async _ =>
        {
            await CheckIfBotIsOnline();

            if (BotIsOnline && APIIsOnline)
            {
                await GetQueue();
            }

            OnChange?.Invoke();
        }, null, 0, 5000);

        Task.Run(async () =>
        {
            await GetBotStatus();
            OnChange?.Invoke();
        });
    }

    private async Task CheckIfBotIsOnline()
    {
        var response = await Client.GetAsync($"/check");
        APIIsOnline = response.IsSuccessStatusCode;
    }


    public async Task StartBot()
    {
        var response = await Client.PostAsJsonAsync("/api/bot/start", Token);

        if (response.IsSuccessStatusCode)
        {
            BotIsOnline = true;
        }
    }

    public async Task StopBot()
    {
        var response = await Client.PostAsync("/api/bot/stop", null);

        if (response.IsSuccessStatusCode)
        {
            BotIsOnline = false;
        }
    }


    public async Task JoinChannel()
    {
        _ = await Client.PostAsJsonAsync("/api/bot/join", ChannelId);
    }

    public async Task LeaveChannel()
    {
        _ = await Client.PostAsync("/api/bot/leave", null);
    }

    public async Task ListServers()
    {
        var response = await Client.GetFromJsonAsync<List<ServerData>>("/api/bot/servers");

        if (response != null)
        {
            Servers = response;
        }
    }

    public async Task ListChannels()
    {
        var response =
            await Client.GetFromJsonAsync<List<ChannelData>>($"/api/bot/channels/{ServerId}");
        if (response != null)
        {
            Channels = response;
        }
    }

    public void SetServerId(string id)
    {
        ServerId = id;
    }

    public void SetChannelId(string id)
    {
        ChannelId = id;
    }

    public async Task GetBotStatus()
    {
        var result = await Client.GetFromJsonAsync<BotStatus>($"/api/bot/status");

        if (result != null)
        {
            Status = result;
            BotIsOnline = true;
        }
    }

    private async Task<List<SearchResult>> SearchSongs()
    {
        var query = Search;

        Console.WriteLine(query);

        if (Search == string.Empty)
        {
            return new List<SearchResult>();
        }

        var response = await Client.PostAsJsonAsync("/api/bot/search", query);

        Console.WriteLine(response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            return new List<SearchResult>();
        }

        var result = await response.Content.ReadFromJsonAsync<List<SearchResult>>();

        Console.WriteLine(result);

        if (result == null)
        {
            return new List<SearchResult>();
        }

        return result;
    }


    public async Task AddSongToQueue(SearchResult song)
    {
        var placeholderItem = new WebQueueItem
        {
            Id = song.Id,
            Title = song.Title,
        };

        Queue.Add(placeholderItem);
        OnChange?.Invoke();

        var response = await Client.PostAsJsonAsync("/api/bot/add", song);
        if (response.IsSuccessStatusCode)
        {
            Queue = await GetQueue();
        }
        else
        {
            Queue.Remove(placeholderItem);
        }

        OnChange?.Invoke();
    }

    private async Task<List<WebQueueItem>> GetQueue()
    {
        var response = await Client.GetFromJsonAsync<List<WebQueueItem>>("/api/bot/queue");

        if (response == null)
        {
            return new List<WebQueueItem>();
        }

        return response;
    }


    public async Task SearchSongsRequest()
    {
        var task = SearchSongs();

        LastSongSeachResult = await task;
    }


    public async Task PlayId(WebQueueItem song)
    {
        _ = await Client.PostAsJsonAsync($"/api/bot/play", song);
    }

    public async Task AutoPlayToggle()
    {
        _ = await Client.PostAsync("/api/bot/autoPlay", null);
    }

    public async Task Play()
    {
        IsPlaying = true;
        _ = await Client.PostAsync($"/api/bot/play", null);
    }

    public async Task Pause()
    {
        IsPlaying = false;
        _ = await Client.PostAsync($"/api/bot/pause", null);
    }

    public async Task CheckIfEnterOnSearch(KeyboardEventArgs args)
    {
        if (args.Key == "Enter")
        {
            await SearchSongsRequest();
        }
    }

    public async Task NextSong()
    {
        _ = await Client.PostAsync($"/api/bot/next", null);
    }

    public async Task PreviousSong()
    {
        _ = await Client.PostAsync($"/api/bot/previous", null);
    }
}