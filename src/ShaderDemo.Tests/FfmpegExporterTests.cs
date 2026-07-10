// Copyright (c) 2026 Patrick JAILLET
using System.Globalization;
using ShaderDemo.Core.Export;

namespace ShaderDemo.Tests;

public class FfmpegExporterTests
{
    [Fact]
    public void BuildArguments_WithoutAudio_OmitsAudioMapping()
    {
        List<string> args = FfmpegExporter.BuildArguments(1920, 1080, 60, null, "out.mp4", null, 0.0, includeAudio: false, isGif: false);

        Assert.DoesNotContain("-map", args);
        Assert.Contains("1920x1080", args);
        Assert.Contains("out.mp4", args);
    }

    [Fact]
    public void BuildArguments_GifOutput_ForcesNoAudioAndAddsPaletteFilter()
    {
        List<string> args = FfmpegExporter.BuildArguments(640, 480, 15, null, "out.gif", "does_not_matter.wav", 0.0, includeAudio: true, isGif: true);

        Assert.DoesNotContain("-map", args);
        Assert.Contains(args, a => a.Contains("palettegen"));
        Assert.Contains("out.gif", args);
    }

    [Fact]
    public void BuildArguments_WithFixedDuration_AddsDurationFlag()
    {
        List<string> args = FfmpegExporter.BuildArguments(1280, 720, 30, 12.5, "out.mp4", null, 0.0, includeAudio: false, isGif: false);

        int index = args.IndexOf("-t");
        Assert.True(index >= 0);
        Assert.Equal("12.5", args[index + 1]);
    }

    [Fact]
    public void BuildArguments_HardwareEncoder_SelectsRequestedCodec()
    {
        List<string> args = FfmpegExporter.BuildArguments(1280, 720, 30, null, "out.mp4", null, 0.0, includeAudio: false, isGif: false, hardwareEncoder: "h264_nvenc");

        Assert.Contains("h264_nvenc", args);
        Assert.DoesNotContain("libx264", args);
    }

    [Fact]
    public void BuildArguments_NoHardwareEncoder_FallsBackToLibx264()
    {
        List<string> args = FfmpegExporter.BuildArguments(1280, 720, 30, null, "out.mp4", null, 0.0, includeAudio: false, isGif: false);

        Assert.Contains("libx264", args);
    }

    [Fact]
    public void BuildArguments_UsesInvariantCultureForDecimalArguments()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

            List<string> args = FfmpegExporter.BuildArguments(1280, 720, 30, 12.5, "out.mp4", null, 0.0, includeAudio: false, isGif: false);

            int index = args.IndexOf("-t");
            Assert.Equal("12.5", args[index + 1]);
            Assert.DoesNotContain(args, a => a.Contains(','));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }
}
