using DiscordMusicBot.Worker.Enums;
using DiscordMusicBot.Worker.Services;

namespace DiscordMusicBot.Worker.Service;

public class AudioService(Worker worker)
{
    private readonly StreamProcessor _streamProcessor = new();
    private CancellationTokenSource _cancellationTokenSource = new();
    private readonly ManualResetEventSlim _manualResetEventSlim = new(true);

    private int _currentQueueIndex;
    private int Volume = 50;

    private PlaybackState _playbackState = PlaybackState.Stopped;


    public async Task Play()
    {
        if (worker._bot == null)
        {
            Console.WriteLine("Bot is not running");
            return;
        }

        if (worker._queue.Count == 0)
        {
            Console.WriteLine("Queue is empty");
            return;
        }

        if (_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        switch (_playbackState)
        {
            case PlaybackState.Playing:
                Pause();
                break;
            case PlaybackState.Paused:
                await Resume();
                break;
            case PlaybackState.Stopped:
                await PlayInternalAsync();
                break;
        }
    }

    private async Task PlayInternalAsync()
    {
        if (worker._bot == null) return;

        _playbackState = PlaybackState.Playing;

        if (_manualResetEventSlim.IsSet) _manualResetEventSlim.Reset();

        var song = worker._queue[_currentQueueIndex];

        var stream = song.StreamTask.Value;
        var token = _cancellationTokenSource.Token;
        _currentQueueIndex = (_currentQueueIndex + 1) % worker._queue.Count;
        await using var processedStream = await _streamProcessor.ProcessStreamAsync(stream, token);
        await worker._bot.PlayMusicAsync(processedStream, token, _manualResetEventSlim);
    }

    public void Pause()
    {
        if (worker._bot == null) return;
        if (_playbackState != PlaybackState.Playing) return;

        lock (_manualResetEventSlim)
        {
            if (_manualResetEventSlim.IsSet)
            {
                _manualResetEventSlim.Reset();
            }
        }

        _playbackState = PlaybackState.Paused;

      //  worker._bot.PauseMusic();
    }

    private async Task Resume()
    {
        if (worker._bot == null) return;
        if (_playbackState != PlaybackState.Paused) return;

        lock (_manualResetEventSlim)
        {
            if (!_manualResetEventSlim.IsSet)
            {
                _manualResetEventSlim.Set();
            }
        }

        _playbackState = PlaybackState.Playing;


      //  await worker._bot.ResumeMusic();
    }


    private async Task Stop()
    {
        if (worker._bot == null) return;
        if (_playbackState == PlaybackState.Stopped) return;

        _playbackState = PlaybackState.Stopped;

        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            await Task.Delay(100);

            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        _manualResetEventSlim.Set();

        //worker._bot.StopMusic();
    }


    public Task SetVolume(string volume)
    {
        Volume = int.Parse(volume);

        worker._bot?.AdjustVolume(int.Parse(volume));

        return Task.CompletedTask;
    }

    public string GetVolume()
    {
        return Volume.ToString();
    }

    public async Task NextSong()
    {
        if (worker._bot == null) return;
        if (worker._queue.Count == 0) return;
        
        _currentQueueIndex = (_currentQueueIndex + 1) % worker._queue.Count;
        
        await Stop();
        
        
        await PlayInternalAsync();
    }

    public async Task PreviousSong()
    {
        if (worker._bot == null) return;
        if (worker._queue.Count == 0) return;

        if (_currentQueueIndex == 0)
        {
            _currentQueueIndex = 0;
        }
        else
        {
            _currentQueueIndex--;
        }

        await Stop();

        await PlayInternalAsync();
    }
}