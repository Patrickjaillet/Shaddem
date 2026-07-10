// Copyright (c) 2026 Patrick JAILLET
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;

namespace ShaderDemo.Core.Export;

public sealed class VideoRecorder
{
    private const int MaxQueuedFrames = 8;

    private Process? _process;
    private BlockingCollection<byte[]>? _writeQueue;
    private Thread? _writerThread;
    private Action<string>? _writeLog;
    private volatile bool _pipeBroken;

    public bool IsRecording => _process is { HasExited: false };
    public string? OutputFile { get; private set; }
    public string FfmpegPath { get; set; } = "ffmpeg";
    public string? HardwareEncoder { get; set; }

    public double EncodedSeconds { get; private set; }
    public long EncodedFrames { get; private set; }
    public int QueuedFrames => _writeQueue?.Count ?? 0;

    public bool Start(
        int width,
        int height,
        int fps,
        double? duration,
        string filename,
        string? musicFile,
        double audioOffset,
        bool includeAudio,
        bool isGif,
        Action<string>? log = null)
    {
        if (IsRecording) return false;

        EncodedSeconds = 0;
        EncodedFrames = 0;

        List<string> args = FfmpegExporter.BuildArguments(
            width, height, fps, duration, filename, musicFile, audioOffset, includeAudio, isGif,
            hardwareEncoder: isGif ? null : HardwareEncoder,
            reportProgress: true);

        var startInfo = new ProcessStartInfo
        {
            FileName = FfmpegPath,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (string arg in args) startInfo.ArgumentList.Add(arg);

        try
        {
            _process = Process.Start(startInfo);
            if (_process == null) return false;

            _process.OutputDataReceived += (_, e) => ParseProgressLine(e.Data);
            _process.BeginOutputReadLine();

            OutputFile = filename;
            _pipeBroken = false;
            _writeLog = log;
            _writeQueue = new BlockingCollection<byte[]>(MaxQueuedFrames);
            _writerThread = new Thread(RunWriterLoop) { IsBackground = true, Name = "VideoRecorderWriter" };
            _writerThread.Start();

            log?.Invoke($"Recording started: {filename}");
            return true;
        }
        catch (Exception ex)
        {
            log?.Invoke($"Failed to start ffmpeg: {ex.Message}");
            _process = null;
            return false;
        }
    }

    private void ParseProgressLine(string? line)
    {
        if (string.IsNullOrEmpty(line)) return;

        int separator = line.IndexOf('=');
        if (separator < 0) return;

        string key = line[..separator];
        string value = line[(separator + 1)..];

        if (key == "out_time_ms" && long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long microseconds))
        {
            EncodedSeconds = microseconds / 1_000_000.0;
        }
        else if (key == "frame" && long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long frame))
        {
            EncodedFrames = frame;
        }
    }

    public bool WriteFrame(byte[] rgbData, Action<string>? log = null)
    {
        if (_process == null || _writeQueue == null) return false;

        if (_pipeBroken)
        {
            Stop();
            return false;
        }

        try
        {
            _writeQueue.Add(rgbData);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private void RunWriterLoop()
    {
        if (_process == null || _writeQueue == null) return;

        try
        {
            foreach (byte[] frame in _writeQueue.GetConsumingEnumerable())
            {
                if (_pipeBroken) continue;

                try
                {
                    _process.StandardInput.BaseStream.Write(frame, 0, frame.Length);
                    _process.StandardInput.BaseStream.Flush();
                }
                catch (IOException ex)
                {
                    _pipeBroken = true;
                    _writeLog?.Invoke($"FFmpeg pipe broken, stopping recording: {ex.Message}");
                }
            }
        }
        catch (ObjectDisposedException)
        {
        }
    }

    public void Stop()
    {
        if (_process == null) return;

        _writeQueue?.CompleteAdding();
        _writerThread?.Join();
        _writeQueue?.Dispose();
        _writeQueue = null;
        _writerThread = null;

        try
        {
            _process.StandardInput.BaseStream.Close();
            _process.WaitForExit();
        }
        catch (IOException)
        {
        }
        finally
        {
            _process.Dispose();
            _process = null;
        }
    }
}
