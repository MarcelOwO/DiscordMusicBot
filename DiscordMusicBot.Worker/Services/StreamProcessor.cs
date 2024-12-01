using System.Diagnostics;
using DSharpPlus.VoiceNext;

namespace DiscordMusicBot.Worker.Services;

public class StreamProcessor
{
    private Process? _ffmpeg;

    private CancellationTokenSource _cancellationTokenSource = new();
    private Task _processTask;
    private bool _isPaused;
    private readonly object _lock = new();

    public async Task ProcessStreamAsync(string filePath, VoiceTransmitSink? sink)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("File does not exist.", filePath);

        if (sink == null)
            throw new ArgumentNullException(nameof(sink));

        StopPlayback();

        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;
        _processTask = Task.Run(async () =>
        {
            try
            {
                StartFFmpeg(filePath);

                await using var standardOutput = _ffmpeg!.StandardOutput.BaseStream;

                Memory<byte> buffer = new(new byte[32768]);

                int bytesRead;

                while ((bytesRead = await standardOutput.ReadAsync(buffer, token)) > 0 &&
                       !token.IsCancellationRequested)
                {
                    lock (_lock)
                    {
                        if (_isPaused)
                        {
                            Monitor.Wait(_lock);
                        }
                    }

                    await sink.WriteAsync(buffer[..bytesRead], token);
                }

                await _ffmpeg.WaitForExitAsync(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during stream processing: {ex.Message}");
            }
        }, token);
    }

    private void StartFFmpeg(string filePath)
    {
        _ffmpeg = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{filePath}\" -filter:a \"atempo=1.0\" -f s16le -ar 48000 -ac 2 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        _ffmpeg.Start();
    }

    public void PausePlayback()
    {
        lock (_lock)
        {
            _isPaused = true;
        }
    }

    public void ResumePlayback()
    {
        lock (_lock)
        {
            if (_isPaused)
            {
                _isPaused = false;
                Monitor.Pulse(_lock);
            }
        }
    }

    public void StopPlayback()
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _processTask?.Wait();
            _processTask = null;
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }
}