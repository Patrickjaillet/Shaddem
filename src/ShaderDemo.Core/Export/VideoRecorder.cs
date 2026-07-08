// Copyright (c) 2026 Patrick JAILLET
using System.Diagnostics;
using System.Globalization;

namespace ShaderDemo.Core.Export;

public sealed class VideoRecorder
{
    private Process? _process;

    public bool IsRecording => _process is { HasExited: false };
    public string? OutputFile { get; private set; }
    public string FfmpegPath { get; set; } = "ffmpeg";
    public string? HardwareEncoder { get; set; }

    public double EncodedSeconds { get; private set; }
    public long EncodedFrames { get; private set; }

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
        if (_process == null) return false;

        try
        {
            _process.StandardInput.BaseStream.Write(rgbData, 0, rgbData.Length);
            _process.StandardInput.BaseStream.Flush();
            return true;
        }
        catch (IOException ex)
        {
            log?.Invoke($"FFmpeg pipe broken, stopping recording: {ex.Message}");
            Stop();
            return false;
        }
    }

    public void Stop()
    {
        if (_process == null) return;

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
