// Copyright (c) 2026 Patrick JAILLET
using System.Diagnostics;

namespace ShaderDemo.Core.Export;

public static class AudioExporter
{
    public static bool ExportAudioOnly(string ffmpegPath, string sourceFile, string outputPath, double? duration, Action<string>? log = null)
    {
        if (!File.Exists(sourceFile))
        {
            log?.Invoke($"Audio source not found: {sourceFile}");
            return false;
        }

        var args = new List<string> { "-y", "-i", sourceFile };
        if (duration.HasValue)
        {
            args.Add("-t");
            args.Add(duration.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        args.Add(outputPath);

        var startInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
        };

        foreach (string arg in args) startInfo.ArgumentList.Add(arg);

        try
        {
            using Process? process = Process.Start(startInfo);
            if (process == null) return false;

            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                log?.Invoke($"Exported audio to {outputPath}");
                return true;
            }

            log?.Invoke($"ffmpeg exited with code {process.ExitCode}");
            return false;
        }
        catch (Exception ex)
        {
            log?.Invoke($"Failed to export audio: {ex.Message}");
            return false;
        }
    }
}
