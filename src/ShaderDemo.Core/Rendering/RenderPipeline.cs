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
    private readonly ShaderProgram _imageLayerProgram;
    private readonly ShaderProgram _blendProgram;
    private readonly ShaderProgram _fallbackProgram;

    public int Width { get; private set; }
    public int Height { get; private set; }
    public float RenderScale { get; private set; } = 1.0f;

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
        _imageLayerProgram = new ShaderProgram(gl, "ImageLayer", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentImageLayer);
        _blendProgram = new ShaderProgram(gl, "LayerBlend", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentBlend);
        _fallbackProgram = new ShaderProgram(gl, "Fallback", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentFallback);

        SceneFbo = Framebuffer.Create(gl, width, height, linearFilter: true);
        LayerFbo = Framebuffer.Create(gl, width, height, linearFilter: true);
        ComposeA = Framebuffer.Create(gl, width, height, linearFilter: true);
        ComposeB = Framebuffer.Create(gl, width, height, linearFilter: true);
    }

    public void Resize(int width, int height)
    {
        Width = width;
        Height = height;
        RecreateInternalBuffers();
    }

    public void SetRenderScale(float scale)
    {
        scale = Math.Clamp(scale, 0.1f, 1.0f);
        if (MathF.Abs(scale - RenderScale) < 0.001f) return;

        RenderScale = scale;
        RecreateInternalBuffers();
    }

    private void RecreateInternalBuffers()
    {
        int internalWidth = Math.Max(1, (int)(Width * RenderScale));
        int internalHeight = Math.Max(1, (int)(Height * RenderScale));

        SceneFbo.Dispose();
        LayerFbo.Dispose();
        ComposeA.Dispose();
        ComposeB.Dispose();

        SceneFbo = Framebuffer.Create(_gl, internalWidth, internalHeight, linearFilter: true);
        LayerFbo = Framebuffer.Create(_gl, internalWidth, internalHeight, linearFilter: true);
        ComposeA = Framebuffer.Create(_gl, internalWidth, internalHeight, linearFilter: true);
        ComposeB = Framebuffer.Create(_gl, internalWidth, internalHeight, linearFilter: true);
    }

    public void RenderShaderPass(ShaderProgram? program, EffectParams effects, float time, Vector2 mouse, AudioUniforms audio, Texture channel0, Framebuffer? target, float qualityScale = 1.0f)
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
            _fallbackProgram.Use();
            _quad.Draw();
            return;
        }

        Vector2 targetResolution = target != null ? new Vector2(target.Width, target.Height) : new Vector2(Width, Height);

        program.Use();
        program.SetUniform("iResolution", targetResolution);
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
        program.SetUniform("customRotationSpeed", audio.RotationSpeed);
        program.SetUniform("customQualityScale", qualityScale);

        _quad.Draw();
    }

    public void RenderImageLayerPass(Texture image, ImageFitMode fitMode, float positionX, float positionY, float scale, float rotation, Framebuffer target)
    {
        target.Clear(0.0f, 0.0f, 0.0f, 0.0f);

        float targetWidth = target.Width;
        float targetHeight = target.Height;
        float imageWidth = image.Width;
        float imageHeight = image.Height;

        float fitScale = fitMode switch
        {
            ImageFitMode.Fit => Math.Min(targetWidth / imageWidth, targetHeight / imageHeight),
            ImageFitMode.Fill => Math.Max(targetWidth / imageWidth, targetHeight / imageHeight),
            ImageFitMode.Center => 1.0f,
            _ => -1.0f,
        };

        Vector2 uvScale;
        Vector2 uvOffset;
        if (fitScale < 0.0f)
        {
            uvScale = Vector2.One;
            uvOffset = Vector2.Zero;
        }
        else
        {
            float dispWidth = imageWidth * fitScale;
            float dispHeight = imageHeight * fitScale;
            uvScale = new Vector2(targetWidth / dispWidth, targetHeight / dispHeight);
            uvOffset = (Vector2.One - uvScale) * 0.5f;
        }

        image.Bind(0);
        _imageLayerProgram.Use();
        _imageLayerProgram.SetUniform("tex", 0);
        _imageLayerProgram.SetUniform("uv_scale", uvScale);
        _imageLayerProgram.SetUniform("uv_offset", uvOffset);
        _imageLayerProgram.SetUniform("position", new Vector2(positionX, positionY));
        _imageLayerProgram.SetUniform("rotation", rotation);
        _imageLayerProgram.SetUniform("user_scale", scale);

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

    public void ClearScreen()
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _gl.Viewport(0, 0, (uint)Width, (uint)Height);
        _gl.ClearColor(0.05f, 0.05f, 0.06f, 1.0f);
        _gl.Clear((uint)ClearBufferMask.ColorBufferBit);
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
        _imageLayerProgram.Dispose();
        _blendProgram.Dispose();
        _fallbackProgram.Dispose();
        SceneFbo.Dispose();
        LayerFbo.Dispose();
        ComposeA.Dispose();
        ComposeB.Dispose();
    }
}
