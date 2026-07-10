// Copyright (c) 2026 Patrick JAILLET
using System.Collections.Concurrent;
using StbImageSharp;

namespace ShaderDemo.Core.Rendering;

public sealed class AsyncTextureLoader
{
    private sealed record DecodedImage(string Path, int Width, int Height, byte[] Pixels);

    private readonly ConcurrentQueue<DecodedImage> _decoded = new();
    private readonly HashSet<string> _pending = new();
    private readonly object _pendingLock = new();

    public int PendingCount
    {
        get { lock (_pendingLock) { return _pending.Count; } }
    }

    public void RequestLoad(string path)
    {
        lock (_pendingLock)
        {
            if (!_pending.Add(path)) return;
        }

        Task.Run(() => DecodeInBackground(path));
    }

    private void DecodeInBackground(string path)
    {
        try
        {
            StbImage.stbi_set_flip_vertically_on_load(1);
            byte[] fileData = File.ReadAllBytes(path);
            ImageResult image = ImageResult.FromMemory(fileData, ColorComponents.RedGreenBlue);
            _decoded.Enqueue(new DecodedImage(path, image.Width, image.Height, image.Data));
        }
        catch (Exception)
        {
        }
        finally
        {
            lock (_pendingLock)
            {
                _pending.Remove(path);
            }
        }
    }

    public void UploadReady(Silk.NET.OpenGL.GL gl, Action<string, Texture> onUploaded, int maxUploadsPerCall = 2)
    {
        int uploaded = 0;
        while (uploaded < maxUploadsPerCall && _decoded.TryDequeue(out DecodedImage? image))
        {
            Texture texture = Texture.Create(gl, image.Width, image.Height, image.Pixels, hasAlpha: false, buildMipmaps: true);
            onUploaded(image.Path, texture);
            uploaded++;
        }
    }
}
