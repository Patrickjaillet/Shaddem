// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core.Export;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.Tests;

public class SrtExporterTests
{
    [Fact]
    public void Export_WritesOnlyTextClipsInTimeOrder()
    {
        var timeline = new TimelineEngine();
        timeline.Add(5.0, 2.0, ClipType.Text, "Second");
        timeline.Add(1.0, 3.0, ClipType.Text, "First");
        timeline.Add(0.0, 10.0, ClipType.Shader, "unrelated.glsl");

        string path = Path.Combine(Path.GetTempPath(), $"srt_test_{Guid.NewGuid():N}.srt");
        try
        {
            int count = SrtExporter.Export(timeline, path);
            string content = File.ReadAllText(path);

            Assert.Equal(2, count);
            Assert.Contains("00:00:01,000 --> 00:00:04,000", content);
            Assert.Contains("First", content);
            Assert.Contains("00:00:05,000 --> 00:00:07,000", content);
            Assert.Contains("Second", content);
            Assert.True(content.IndexOf("First", StringComparison.Ordinal) < content.IndexOf("Second", StringComparison.Ordinal));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Export_NoTextClips_ReturnsZeroAndWritesNoFile()
    {
        var timeline = new TimelineEngine();
        timeline.Add(0.0, 10.0, ClipType.Shader, "unrelated.glsl");

        string path = Path.Combine(Path.GetTempPath(), $"srt_empty_{Guid.NewGuid():N}.srt");
        int count = SrtExporter.Export(timeline, path);

        Assert.Equal(0, count);
        Assert.False(File.Exists(path));
    }
}
