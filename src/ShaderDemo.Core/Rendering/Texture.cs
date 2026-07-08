// Copyright (c) 2026 Patrick JAILLET
using Silk.NET.OpenGL;

namespace ShaderDemo.Core.Rendering;

public sealed class Texture : IDisposable
{
    private readonly GL _gl;

    public uint Handle { get; }
    public int Width { get; }
    public int Height { get; }

    private Texture(GL gl, uint handle, int width, int height)
    {
        _gl = gl;
        Handle = handle;
        Width = width;
        Height = height;
    }

    public static unsafe Texture Create(GL gl, int width, int height, ReadOnlySpan<byte> pixels, bool hasAlpha, bool buildMipmaps)
    {
        uint texture = gl.GenTexture();
        gl.BindTexture(TextureTarget.Texture2D, texture);

        PixelFormat format = hasAlpha ? PixelFormat.Rgba : PixelFormat.Rgb;
        InternalFormat internalFormat = hasAlpha ? InternalFormat.Rgba8 : InternalFormat.Rgb8;

        fixed (byte* ptr = pixels)
        {
            gl.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, (uint)width, (uint)height, 0, format, PixelType.UnsignedByte, ptr);
        }

        var wrap = (int)GLEnum.Repeat;
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, ref wrap);
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, ref wrap);

        if (buildMipmaps)
        {
            gl.GenerateMipmap(TextureTarget.Texture2D);
            var minFilter = (int)GLEnum.LinearMipmapLinear;
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, ref minFilter);
        }
        else
        {
            var minFilter = (int)GLEnum.Linear;
            gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, ref minFilter);
        }

        var magFilter = (int)GLEnum.Linear;
        gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, ref magFilter);

        gl.BindTexture(TextureTarget.Texture2D, 0);
        return new Texture(gl, texture, width, height);
    }

    public void Bind(int unit)
    {
        _gl.ActiveTexture(TextureUnit.Texture0 + unit);
        _gl.BindTexture(TextureTarget.Texture2D, Handle);
    }

    public void Dispose()
    {
        _gl.DeleteTexture(Handle);
    }
}
