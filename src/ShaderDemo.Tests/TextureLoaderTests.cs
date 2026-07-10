// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Tests;

public class TextureLoaderTests
{
    private static byte[] MakePixels(int width, int height)
    {
        var pixels = new byte[width * height * 3];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = (y * width + x) * 3;
                pixels[idx] = (byte)(x % 256);
                pixels[idx + 1] = (byte)(y % 256);
                pixels[idx + 2] = 128;
            }
        }

        return pixels;
    }

    [Fact]
    public void DownscaleIfNeeded_LeavesImageUnchanged_WhenWithinLimit()
    {
        byte[] pixels = MakePixels(800, 600);

        (int width, int height, byte[] result) = TextureLoader.DownscaleIfNeeded(800, 600, pixels, maxDimension: 2048);

        Assert.Equal(800, width);
        Assert.Equal(600, height);
        Assert.Same(pixels, result);
    }

    [Fact]
    public void DownscaleIfNeeded_DoesNothing_WhenClampDisabled()
    {
        byte[] pixels = MakePixels(4096, 2304);

        (int width, int height, byte[] result) = TextureLoader.DownscaleIfNeeded(4096, 2304, pixels, maxDimension: 0);

        Assert.Equal(4096, width);
        Assert.Equal(2304, height);
        Assert.Same(pixels, result);
    }

    [Fact]
    public void DownscaleIfNeeded_ScalesDownToFitLongestEdge_PreservingAspectRatio()
    {
        byte[] pixels = MakePixels(4096, 2304);

        (int width, int height, byte[] result) = TextureLoader.DownscaleIfNeeded(4096, 2304, pixels, maxDimension: 1024);

        Assert.Equal(1024, width);
        Assert.Equal(576, height);
        Assert.Equal(width * height * 3, result.Length);

        double originalAspect = 4096.0 / 2304.0;
        double newAspect = (double)width / height;
        Assert.InRange(newAspect, originalAspect - 0.01, originalAspect + 0.01);
    }

    [Fact]
    public void DownscaleIfNeeded_ReducesPixelByteCountSubstantially_ForLargeSourceImages()
    {
        byte[] pixels = MakePixels(4096, 2304);
        long originalBytes = pixels.LongLength;

        (_, _, byte[] result) = TextureLoader.DownscaleIfNeeded(4096, 2304, pixels, maxDimension: 1024);

        Assert.True(result.LongLength < originalBytes / 10, $"expected >10x reduction, got {originalBytes} -> {result.LongLength}");
    }

    [Fact]
    public void DownscaleIfNeeded_HandlesPortraitImages_ClampingTallerEdge()
    {
        byte[] pixels = MakePixels(1000, 3000);

        (int width, int height, byte[] result) = TextureLoader.DownscaleIfNeeded(1000, 3000, pixels, maxDimension: 1500);

        Assert.Equal(500, width);
        Assert.Equal(1500, height);
        Assert.Equal(width * height * 3, result.Length);
    }
}
