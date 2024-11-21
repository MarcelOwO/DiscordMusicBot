using System.Diagnostics;

namespace DiscordMusicBot.Worker.Services;

public class StreamProcessor : IDisposable
{
    private Process? _ffmpeg;

    private void StartFFmpeg()
    {
        _ffmpeg = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-re -i pipe:0 -f s16le -ar 48000 -ac 2 pipe:1",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        try
        {
            _ffmpeg.Start();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            _ffmpeg.Dispose();
            throw;
        }
    }

    public StreamProcessor()
    {
        StartFFmpeg();
    }

    public async Task<Stream> ProcessStreamAsync(Stream audioStream, CancellationToken token)
    {
        Dispose();
        StartFFmpeg();
        
        try
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await audioStream.CopyToAsync(_ffmpeg.StandardInput.BaseStream, token);
                }
                catch (Exception e) when (e is OperationCanceledException)
                {
                    Console.WriteLine("Stream processing canceled");
                }
                finally
                {
                    await _ffmpeg.StandardInput.BaseStream.FlushAsync(token);
                    
                    _ffmpeg.StandardInput.Close();
                }
            }, token);
        }
        catch (Exception e) when (e is OperationCanceledException)
        {
            Dispose();
            StartFFmpeg();
        }

        return _ffmpeg.StandardOutput.BaseStream;
    }

    public void Dispose()
    {
        if (_ffmpeg == null) return;

        if (!_ffmpeg.HasExited == false)
        {
            _ffmpeg.Kill();
        }

        _ffmpeg?.Dispose();
        _ffmpeg = null;
    }
}