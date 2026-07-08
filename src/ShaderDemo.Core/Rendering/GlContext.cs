// Copyright (c) 2026 Patrick JAILLET
using Silk.NET.OpenGL;

namespace ShaderDemo.Core.Rendering;

public sealed class GlContext
{
    public GL Api { get; }

    public GlContext(GL api)
    {
        Api = api;
    }

    public void Viewport(int x, int y, int width, int height)
    {
        Api.Viewport(x, y, (uint)width, (uint)height);
    }

    public unsafe void Clear(float r, float g, float b, float a)
    {
        Api.ClearColor(r, g, b, a);
        Api.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    public void UseScreen(int width, int height)
    {
        Api.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        Viewport(0, 0, width, height);
    }

    public void EnableBlend()
    {
        Api.Enable(EnableCap.Blend);
        Api.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    }

    public void DisableBlend()
    {
        Api.Disable(EnableCap.Blend);
    }
}
