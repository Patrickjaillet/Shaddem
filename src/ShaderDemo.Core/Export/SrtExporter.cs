// Copyright (c) 2026 Patrick JAILLET
using System.Text;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.Core.Export;

public static class SrtExporter
{
    public static int Export(TimelineEngine timeline, string outputPath)
    {
        List<TimelineClip> textClips = timeline.Clips
            .Where(c => c.Type == ClipType.Text)
            .OrderBy(c => c.Start)
            .ToList();

        if (textClips.Count == 0) return 0;

        var sb = new StringBuilder();
        for (int i = 0; i < textClips.Count; i++)
        {
            TimelineClip clip = textClips[i];
            sb.Append(i + 1).Append('\n');
            sb.Append(FormatTimecode(clip.Start)).Append(" --> ").Append(FormatTimecode(clip.Start + clip.Duration)).Append('\n');
            sb.Append(clip.Resource).Append('\n').Append('\n');
        }

        File.WriteAllText(outputPath, sb.ToString());
        return textClips.Count;
    }

    private static string FormatTimecode(double seconds)
    {
        if (seconds < 0) seconds = 0;
        var span = TimeSpan.FromSeconds(seconds);
        return $"{(int)span.TotalHours:D2}:{span.Minutes:D2}:{span.Seconds:D2},{span.Milliseconds:D3}";
    }
}
