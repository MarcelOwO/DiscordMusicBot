using DiscordMusicBot.Core.Enums;
using DiscordMusicBot.Worker.Services;

namespace DiscordMusicBot.Worker.Service;

public class AudioService(Worker worker)
{
    private readonly StreamProcessor _streamProcessor = new();

    private int _volume;

    private PlaybackState _playbackState = PlaybackState.Stopped;

    public int CurrentQueueIndex { get; private set; }

    public async Task Play()
    {
        if (worker.Bot == null)
        {
            Console.WriteLine("Bot is not running");
            return;
        }

        if (worker.Queue.Count == 0)
        {
            Console.WriteLine("Queue is empty");
            return;
        }

        switch (_playbackState)
        {
            case PlaybackState.Playing:
                Console.WriteLine("Pause");
                Pause();
                break;
            case PlaybackState.Paused:
                Console.WriteLine("Resume");
                await Resume();
                break;
            case PlaybackState.Stopped:
                Console.WriteLine("Play");
                await PlayInternalAsync();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task PlayInternalAsync()
    {
        if (worker.Bot == null)
        {
            Console.WriteLine("Escaping internalmusiclogic because bot is null");
            return;
        }

        _playbackState = PlaybackState.Playing;


        var song = worker.Queue[CurrentQueueIndex];


        try
        {
            await _streamProcessor.ProcessStreamAsync(song.filePath, worker.Bot.GetConnection());
        }
        catch (Exception e) when (e is ObjectDisposedException)
        {
            Console.WriteLine("Error while playing music because of cancellation token missing");
        }
    }

    public void Pause()
    {
        if (worker.Bot == null)
        {
            Console.WriteLine("Escaping pausing music because bot is null");
            return;
        }

        if (_playbackState != PlaybackState.Playing)
        {
            Console.WriteLine("Escaping pausing music because playback state is not playing");
            return;
        }


        _playbackState = PlaybackState.Paused;
        _streamProcessor.PausePlayback();
        //worker.Bot.PauseMusic();
    }

    private async Task Resume()
    {
        if (worker.Bot == null)
        {
            Console.WriteLine("Escaping resuming music because bot is null");
            return;
        }

        if (_playbackState != PlaybackState.Paused)
        {
            Console.WriteLine("Escaping resuming music because playback state is not paused");
            return;
        }

        _streamProcessor.ResumePlayback();
        _playbackState = PlaybackState.Playing;

        //await worker.Bot.ResumeMusic();
    }


    private async Task Stop()
    {
        if (worker.Bot == null)
        {
            Console.WriteLine("Escaping stopping music because bot is null");
            return;
        }

        if (_playbackState == PlaybackState.Stopped)
        {
            Console.WriteLine("Escaping stopping music because playback state is stopped");
            return;
        }

        _streamProcessor.StopPlayback();

        _playbackState = PlaybackState.Stopped;
    }


    public Task SetVolume(string volume)
    {
        _volume = int.Parse(volume);

        worker.Bot?.AdjustVolume(int.Parse(volume));

        return Task.CompletedTask;
        
    }
    

    public string GetVolume()
    {
        return _volume.ToString();
    }

    public async Task NextSong()
    {
        if (worker.Bot == null) return;
        if (worker.Queue.Count == 0) return;

        CurrentQueueIndex = (CurrentQueueIndex + 1) % worker.Queue.Count;

        await Stop();


        await PlayInternalAsync();
    }

    public async Task PreviousSong()
    {
        if (worker.Bot == null) return;
        if (worker.Queue.Count == 0) return;

        if (CurrentQueueIndex == 0)
        {
            CurrentQueueIndex = 0;
        }
        else
        {
            CurrentQueueIndex--;
        }

        await Stop();

        await PlayInternalAsync();
    }
}