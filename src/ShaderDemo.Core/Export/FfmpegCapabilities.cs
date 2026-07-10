// Copyright (c) 2026 Patrick JAILLET
using System.Diagnostics;

namespace ShaderDemo.Core.Export;

public static class FfmpegCapabilities
{
    private static readonly string[] PreferredHardwareEncoders = { "h264_nvenc", "h264_qsv" };

    public static string? DetectHardwareEncoder(string ffmpegPath, Action<string>? log = null)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            startInfo.ArgumentList.Add("-hide_banner");
            startInfo.ArgumentList.Add("-encoders");

            using Process? process = Process.Start(startInfo);
            if (process == null) return null;

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);

            foreach (string encoder in PreferredHardwareEncoders)
            {
                if (output.Contains(encoder, StringComparison.Ordinal))
                {
                    return encoder;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            log?.Invoke($"Hardware encoder detection failed: {ex.Message}");
            return null;
        }
    }
}
