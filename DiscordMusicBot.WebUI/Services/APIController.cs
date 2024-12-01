using System.Net.Http.Json;
using DiscordMusicBot.Core.Enums;
using DiscordMusicBot.Core.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;

namespace DiscordMusicBot.WebUI.Services;

public class ApiController
{
    private HttpClient Client { get; }
    private LocalStorageService LocalStorageService { get; }

    public string Token = string.Empty;
    public string ChannelId = string.Empty;
    public string ServerId = string.Empty;

    public string Search = string.Empty;


    public List<ServerData> Servers = [];
    public List<ChannelData> Channels = [];

    public ApiStatus ApiStatusData = new();

    public event Action? OnChange;

    private const string AppIdentifier = "UWUDISCORDMUSICBOT";

    public int Volume { get; set; }

    private Timer? _timer;
    private Timer? _volumeDebounceTimer;

    public string currentID = string.Empty;

    public List<AudioProviders> AudioProviders { get; } =
    [
        Core.Enums.AudioProviders.Youtube, Core.Enums.AudioProviders.Tidal, Core.Enums.AudioProviders.Spotify
    ];

    public AudioProviders SelectedAudioProvider { get; set; } = Core.Enums.AudioProviders.Youtube;

    public List<SearchResult> LastSongSearchResult { get; set; } = new();


    public ApiController(HttpClient client, LocalStorageService storageService)
    {
        Client = client;
        LocalStorageService = storageService;
        try
        {
            _timer = new Timer(TimerCallback, null, 0, 5000);
        }
        catch (Exception e) when (e is ArgumentOutOfRangeException or ArgumentNullException)
        {
            _timer?.Dispose();

            Console.WriteLine(e);
        }
    }

    private async void TimerCallback(object? state)
    {
        if (ApiStatusData.IsRunning) return;

        await Task.Run(async () => await CheckApiStatus());

        OnChange?.Invoke();
    }

    private async Task CheckApiStatus()
    {
        HttpResponseMessage response;

        try
        {
            response = await Client.GetAsync($"/check");
        }
        catch (Exception e) when (e is InvalidOperationException or HttpRequestException or TaskCanceledException
                                      or UriFormatException)
        {
            return;
        }

        ApiStatusData.IsRunning = response.IsSuccessStatusCode;

        if (response.IsSuccessStatusCode)
        {
            await GetApiStatus();

            if (_timer != null)
            {
                await _timer.DisposeAsync();
                _timer = null;
            }
        }
    }

    private async Task GetApiStatus()
    {
        var response = await Client.GetFromJsonAsync<ApiStatus>("/api/bot/status");

        if (response != null)
        {
            ApiStatusData = response;
        }
    }


    public async Task StartBot()
    {
        var response = await Client.PostAsJsonAsync("/api/bot/start", Token);

        if (response.IsSuccessStatusCode)
        {
            ApiStatusData.BotIsRunning = true;
        }
    }

    public async Task StopBot()
    {
        var response = await Client.PostAsync("/api/bot/stop", null);

        if (response.IsSuccessStatusCode)
        {
            ApiStatusData.BotIsRunning = false;
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

    #region Search

    public async Task CheckIfEnterOnSearch(KeyboardEventArgs args)
    {
        if (args.Key == "Enter")
        {
            LastSongSearchResult = await SearchSongs();
        }
    }

    public async Task SearchSongsRequest()
    {
        LastSongSearchResult = await SearchSongs();
    }

    private async Task<List<SearchResult>> SearchSongs()
    {
        var query = Search;

        Console.WriteLine(query);

        if (Search == string.Empty)
        {
            return [];
        }

        var response = await Client.PostAsJsonAsync("/api/bot/search", query);

        Console.WriteLine(response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        var result = await response.Content.ReadFromJsonAsync<List<SearchResult>>();

        Console.WriteLine(result);

        return result ?? [];
    }

    #endregion

    #region Persistance

    public async Task SaveData()
    {
        await LocalStorageService.SetItem(AppIdentifier + "token", Token);
        await LocalStorageService.SetItem(AppIdentifier + "channelId", ChannelId);
        await LocalStorageService.SetItem(AppIdentifier + "serverId", ServerId);
    }

    public async Task GetSavedData()
    {
        Token = await LocalStorageService.GetItem(AppIdentifier + "token");
        ChannelId = await LocalStorageService.GetItem(AppIdentifier + "channelId");
        ServerId = await LocalStorageService.GetItem(AppIdentifier + "serverId");
    }

    #endregion

    #region Queue

    public async Task AddSongToQueue(SearchResult song)
    {
        var placeholderItem = new WebQueueItem(song.Id, song.Title, true);

        ApiStatusData.QueuedSongs.Add(placeholderItem);

        OnChange?.Invoke();

        var response = await Client.PostAsJsonAsync("/api/bot/add", song);
        if (response.IsSuccessStatusCode)
        {
            ApiStatusData.QueuedSongs = await GetQueue();
        }
        else
        {
            ApiStatusData.QueuedSongs.Remove(placeholderItem);
        }

        OnChange?.Invoke();
    }

    private async Task<List<WebQueueItem>> GetQueue()
    {
        var response = await Client.GetFromJsonAsync<List<WebQueueItem>>("/api/bot/queue");

        return response ?? [];
    }

    #endregion


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
        ApiStatusData.PlaybackState = PlaybackState.Playing;

        _ = await Client.PostAsync($"/api/bot/play", null);
        await GetCurrentSong();
    }

    public async Task Pause()
    {
        ApiStatusData.PlaybackState = PlaybackState.Paused;

        try
        {
            _ = await Client.PostAsync($"/api/bot/pause", null);
        }
        catch (Exception e) when (e is InvalidOperationException or HttpRequestException or TaskCanceledException
                                      or UriFormatException)
        {
            Console.WriteLine(e);
        }

        await GetCurrentSong();
    }


    public async Task NextSong()
    {
        try
        {
            _ = await Client.PostAsync($"/api/bot/next", null);
        }
        catch (Exception e) when (e is InvalidOperationException or HttpRequestException or TaskCanceledException
                                      or UriFormatException)
        {
            Console.WriteLine(e);
        }

        await GetCurrentSong();
    }

    public async Task PreviousSong()
    {
        try
        {
            _ = await Client.PostAsync($"/api/bot/previous", null);
        }
        catch (Exception e) when (e is InvalidOperationException or HttpRequestException or TaskCanceledException
                                      or UriFormatException)
        {
            Console.WriteLine(e);
        }

        await GetCurrentSong();
    }


    public async Task GetCurrentSong()
    {
        HttpResponseMessage? response;
        try
        {
            response = await Client.GetAsync($"/api/bot/current");
        }
        catch (Exception e) when (e is InvalidOperationException or HttpRequestException or TaskCanceledException
                                      or UriFormatException)
        {
            Console.WriteLine(e);
            return;
        }

        currentID = await response?.Content.ReadAsStringAsync();
    }

    #region Volume

    private async Task UpdateVolume(int volume)
    {
        var response = await Client.PostAsJsonAsync("/api/bot/volume", Volume.ToString());

        if (response.IsSuccessStatusCode)
        {
            Volume = volume;
        }
    }

    public async Task OnVolumeChanged(int volume)
    {
        _volumeDebounceTimer?.Dispose();

        _volumeDebounceTimer = new Timer(async _ => await UpdateVolume(volume), null, 300, Timeout.Infinite);


        await UpdateVolume(volume);
    }

    #endregion
}