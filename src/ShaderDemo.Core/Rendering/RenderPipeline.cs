// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using Silk.NET.OpenGL;
using ShaderDemo.Core.Audio;

namespace ShaderDemo.Core.Rendering;

public sealed class RenderPipeline : IDisposable
{
    private readonly GL _gl;
    private readonly FullscreenQuad _quad;
    private readonly ShaderProgram _texturePassProgram;
    private readonly ShaderProgram _blendProgram;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public Framebuffer SceneFbo { get; private set; }
    public Framebuffer LayerFbo { get; private set; }
    public Framebuffer ComposeA { get; private set; }
    public Framebuffer ComposeB { get; private set; }

    public RenderPipeline(GL gl, int width, int height)
    {
        _gl = gl;
        Width = width;
        Height = height;

        _quad = new FullscreenQuad(gl);
        _texturePassProgram = new ShaderProgram(gl, "TexturePass", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentTexture);
        _blendProgram = new ShaderProgram(gl, "LayerBlend", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentBlend);

        SceneFbo = Framebuffer.Create(gl, width, height);
        LayerFbo = Framebuffer.Create(gl, width, height);
        ComposeA = Framebuffer.Create(gl, width, height);
        ComposeB = Framebuffer.Create(gl, width, height);
    }

    public void Resize(int width, int height)
    {
        Width = width;
        Height = height;

        SceneFbo.Dispose();
        LayerFbo.Dispose();
        ComposeA.Dispose();
        ComposeB.Dispose();

        SceneFbo = Framebuffer.Create(_gl, width, height);
        LayerFbo = Framebuffer.Create(_gl, width, height);
        ComposeA = Framebuffer.Create(_gl, width, height);
        ComposeB = Framebuffer.Create(_gl, width, height);
    }

    public void RenderShaderPass(ShaderProgram? program, EffectParams effects, float time, Vector2 mouse, AudioUniforms audio, Texture channel0, Framebuffer? target)
    {
        if (target != null)
        {
            target.Use();
            target.Clear(0.0f, 0.0f, 0.0f, 1.0f);
        }
        else
        {
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            _gl.Viewport(0, 0, (uint)Width, (uint)Height);
            _gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            _gl.Clear((uint)ClearBufferMask.ColorBufferBit);
        }

        if (program == null)
        {
            using var fallback = new ShaderProgram(_gl, "Fallback", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentFallback);
            fallback.Use();
            _quad.Draw();
            return;
        }

        program.Use();
        program.SetUniform("iResolution", new Vector2(Width, Height));
        program.SetUniform("iTime", time * effects.Speed);
        program.SetUniform("iOffset", audio.ShakeOffset);
        program.SetUniform("iMouse", new Vector4(mouse.X, mouse.Y, 0.0f, 0.0f));
        channel0.Bind(0);
        program.SetUniform("iChannel0", 0);
        effects.Apply(program);
        program.SetUniform("customStrobe", audio.Strobe);
        program.SetUniform("customRgbSplit", audio.RgbSplit);
        program.SetUniform("customScale", audio.Scale);
        program.SetUniform("customAudioKick", audio.Kick);

        _quad.Draw();
    }

    public void BlendLayer(Framebuffer baseFbo, Framebuffer layerFbo, BlendMode mode, float opacity, Framebuffer? target)
    {
        if (target != null)
        {
            target.Use();
        }
        else
        {
            _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            _gl.Viewport(0, 0, (uint)Width, (uint)Height);
        }

        baseFbo.BindTexture(0);
        layerFbo.BindTexture(1);

        _blendProgram.Use();
        _blendProgram.SetUniform("tex_base", 0);
        _blendProgram.SetUniform("tex_layer", 1);
        _blendProgram.SetUniform("mode", (int)mode);
        _blendProgram.SetUniform("opacity", opacity);

        _quad.Draw();
    }

    public void PresentToScreen(Framebuffer source)
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _gl.Viewport(0, 0, (uint)Width, (uint)Height);
        _gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        _gl.Clear((uint)ClearBufferMask.ColorBufferBit);

        source.BindTexture(0);
        _texturePassProgram.Use();
        _texturePassProgram.SetUniform("tex", 0);

        _quad.Draw();
    }

    public void Dispose()
    {
        _quad.Dispose();
        _texturePassProgram.Dispose();
        _blendProgram.Dispose();
        SceneFbo.Dispose();
        LayerFbo.Dispose();
        ComposeA.Dispose();
        ComposeB.Dispose();
    }
}
