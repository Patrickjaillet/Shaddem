// Copyright (c) 2026 Patrick JAILLET
using Silk.NET.OpenGL;
using StbImageSharp;

namespace ShaderDemo.Core.Rendering;

public static class TextureLoader
{
    public static Texture? Load(GL gl, string path, Action<string>? log = null)
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

            Texture texture = Texture.Create(gl, image.Width, image.Height, image.Data, hasAlpha: false, buildMipmaps: true);
            return texture;
        }
        catch (Exception ex)
        {
            log?.Invoke($"Failed to load texture {path}: {ex.Message}");
            return null;
        }
    }

    public static Texture CreateNoiseTexture(GL gl, int size = 256)
    {
        var random = new Random();
        byte[] data = new byte[size * size * 3];
        random.NextBytes(data);
        return Texture.Create(gl, size, size, data, hasAlpha: false, buildMipmaps: false);
    }
}
