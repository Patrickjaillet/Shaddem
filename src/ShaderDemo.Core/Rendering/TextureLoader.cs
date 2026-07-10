// Copyright (c) 2026 Patrick JAILLET
using Silk.NET.OpenGL;
using StbImageSharp;

namespace ShaderDemo.Core.Rendering;

public static class TextureLoader
{
    public static Texture? Load(GL gl, string path, Action<string>? log = null, int maxDimension = 0)
    {
        if (!File.Exists(path))
        {
            log?.Invoke($"Texture not found: {path}");
            return null;
        }

        try
        {
            StbImage.stbi_set_flip_vertically_on_load(1);
            byte[] fileData = File.ReadAllBytes(path);
            ImageResult image = ImageResult.FromMemory(fileData, ColorComponents.RedGreenBlue);

            (int width, int height, byte[] pixels) = DownscaleIfNeeded(image.Width, image.Height, image.Data, maxDimension);
            if (width != image.Width || height != image.Height)
            {
                log?.Invoke($"Downscaled texture {path}: {image.Width}x{image.Height} -> {width}x{height} (max dimension {maxDimension})");
            }

            Texture texture = Texture.Create(gl, width, height, pixels, hasAlpha: false, buildMipmaps: true);
            return texture;
        }
        catch (Exception ex)
        {
            log?.Invoke($"Failed to load texture {path}: {ex.Message}");
            return null;
        }
    }

    public static (int Width, int Height, byte[] Pixels) DownscaleIfNeeded(int width, int height, byte[] pixels, int maxDimension)
    {
        if (maxDimension <= 0 || (width <= maxDimension && height <= maxDimension))
        {
            return (width, height, pixels);
        }

        float scale = maxDimension / (float)Math.Max(width, height);
        int newWidth = Math.Max(1, (int)(width * scale));
        int newHeight = Math.Max(1, (int)(height * scale));

        byte[] resized = new byte[newWidth * newHeight * 3];
        for (int y = 0; y < newHeight; y++)
        {
            int srcY = Math.Min(height - 1, (int)(y / scale));
            for (int x = 0; x < newWidth; x++)
            {
                int srcX = Math.Min(width - 1, (int)(x / scale));
                int srcIdx = (srcY * width + srcX) * 3;
                int dstIdx = (y * newWidth + x) * 3;
                resized[dstIdx] = pixels[srcIdx];
                resized[dstIdx + 1] = pixels[srcIdx + 1];
                resized[dstIdx + 2] = pixels[srcIdx + 2];
            }
        }

        return (newWidth, newHeight, resized);
    }

    public static Texture CreateNoiseTexture(GL gl, int size = 256)
    {
        var random = new Random();
        byte[] data = new byte[size * size * 3];
        random.NextBytes(data);
        return Texture.Create(gl, size, size, data, hasAlpha: false, buildMipmaps: false);
    }
}
