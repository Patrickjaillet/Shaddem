// Copyright (c) 2026 Patrick JAILLET
using System.Globalization;

namespace ShaderDemo.Core.Export;

public static class FfmpegExporter
{
    public static List<string> BuildArguments(
        int width,
        int height,
        int fps,
        double? duration,
        string filename,
        string? musicFile,
        double audioOffset,
        bool includeAudio,
        bool isGif,
        string? hardwareEncoder = null,
        bool reportProgress = false)
    {
        var args = new List<string>
        {
            "-y",
            "-f", "rawvideo",
            "-vcodec", "rawvideo",
            "-s", $"{width}x{height}",
            "-pix_fmt", "rgb24",
            "-framerate", fps.ToString(),
            "-i", "-",
        };

        if (isGif) includeAudio = false;

        if (includeAudio && musicFile != null && File.Exists(musicFile))
        {
            if (audioOffset > 0)
            {
                args.Add("-ss");
                args.Add(audioOffset.ToString("F3", CultureInfo.InvariantCulture));
            }

            args.Add("-i");
            args.Add(musicFile);
            args.AddRange(new[] { "-map", "0:v:0", "-map", "1:a:0" });
            args.AddRange(new[] { "-c:a", "aac", "-b:a", "192k" });
        }

        if (duration.HasValue)
        {
            args.Add("-t");
            args.Add(duration.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (reportProgress)
        {
            args.Add("-progress");
            args.Add("pipe:1");
        }

        if (isGif)
        {
            args.AddRange(new[]
            {
                "-vf", "fps=15,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse",
                "-loop", "0",
                filename,
            });
        }
        else
        {
            args.AddRange(BuildVideoCodecArguments(hardwareEncoder));
            args.Add(filename);
        }

        return args;
    }

    private static IEnumerable<string> BuildVideoCodecArguments(string? hardwareEncoder)
    {
        return hardwareEncoder switch
        {
            "h264_nvenc" => new[] { "-c:v", "h264_nvenc", "-pix_fmt", "yuv420p", "-preset", "medium", "-rc:v", "vbr", "-cq", "18" },
            "h264_qsv" => new[] { "-c:v", "h264_qsv", "-pix_fmt", "yuv420p", "-preset", "medium", "-global_quality", "18" },
            _ => new[] { "-c:v", "libx264", "-pix_fmt", "yuv420p", "-preset", "medium", "-crf", "18" },
        };
    }
}
