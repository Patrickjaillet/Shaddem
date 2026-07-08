// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core.Rendering;
using StbImageWriteSharp;

namespace ShaderDemo.Core.Export;

public static class ScreenshotService
{
    public static string Save(Framebuffer source, string directory)
    {
        Directory.CreateDirectory(directory);
        string filename = Path.Combine(directory, $"screenshot_{DateTime.Now:yyyyMMdd-HHmmss}.png");

        byte[] pixels = source.ReadPixelsRgb();
        byte[] flipped = FlipVertically(pixels, source.Width, source.Height, 3);

        using FileStream stream = File.OpenWrite(filename);
        var writer = new ImageWriter();
        writer.WritePng(flipped, source.Width, source.Height, ColorComponents.RedGreenBlue, stream);

        return filename;
    }

    private static byte[] FlipVertically(byte[] pixels, int width, int height, int components)
    {
        int stride = width * components;
        var flipped = new byte[pixels.Length];
        for (int y = 0; y < height; y++)
        {
            int srcOffset = y * stride;
            int dstOffset = (height - 1 - y) * stride;
            Array.Copy(pixels, srcOffset, flipped, dstOffset, stride);
        }

        return flipped;
    }
}
