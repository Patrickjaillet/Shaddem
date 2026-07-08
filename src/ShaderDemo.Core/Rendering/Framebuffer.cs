// Copyright (c) 2026 Patrick JAILLET
using Silk.NET.OpenGL;

namespace ShaderDemo.Core.Rendering;

public sealed class Framebuffer : IDisposable
{
    private readonly GL _gl;

    public uint Handle { get; }
    public uint ColorTexture { get; }
    public int Width { get; }
    public int Height { get; }

    private Framebuffer(GL gl, uint handle, uint colorTexture, int width, int height)
    {
        _gl = gl;
        Handle = handle;
        ColorTexture = colorTexture;
        Width = width;
        Height = height;
    }

    public static unsafe Framebuffer Create(GL gl, int width, int height, bool linearFilter = false)
    {
        uint texture = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, texture);
        gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)width, (uint)height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);

        var filter = linearFilter ? (int)GLEnum.Linear : (int)GLEnum.Nearest;
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, ref filter);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, ref filter);
        var clamp = (int)GLEnum.ClampToEdge;
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, ref clamp);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, ref clamp);

        uint fbo = gl.GenFramebuffer();
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
        gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texture, 0);

        var status = gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != GLEnum.FramebufferComplete)
        {
            throw new InvalidOperationException($"Framebuffer incomplete: {status}");
        }

        gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        return new Framebuffer(gl, fbo, texture, width, height);
    }

    public static Framebuffer CreateScaled(GL gl, int baseWidth, int baseHeight, float scale, bool linearFilter = true)
    {
        int width = Math.Max(1, (int)(baseWidth * scale));
        int height = Math.Max(1, (int)(baseHeight * scale));
        return Create(gl, width, height, linearFilter);
    }

    public void Use()
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);
        _gl.Viewport(0, 0, (uint)Width, (uint)Height);
    }

    public void Clear(float r = 0f, float g = 0f, float b = 0f, float a = 1f)
    {
        Use();
        _gl.ClearColor(r, g, b, a);
        _gl.Clear((uint)ClearBufferMask.ColorBufferBit);
    }

    public void BindTexture(int unit)
    {
        _gl.ActiveTexture(TextureUnit.Texture0 + unit);
        _gl.BindTexture(TextureTarget.Texture2D, ColorTexture);
    }

    public unsafe byte[] ReadPixelsRgb()
    {
        Use();
        _gl.PixelStore(PixelStoreParameter.PackAlignment, 1);
        byte[] data = new byte[Width * Height * 3];
        fixed (byte* ptr = data)
        {
            _gl.ReadPixels(0, 0, (uint)Width, (uint)Height, PixelFormat.Rgb, PixelType.UnsignedByte, ptr);
        }

        return data;
    }

    public void Dispose()
    {
        _gl.DeleteFramebuffer(Handle);
        _gl.DeleteTexture(ColorTexture);
    }
}
