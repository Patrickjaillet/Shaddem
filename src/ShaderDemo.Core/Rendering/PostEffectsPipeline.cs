// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using Silk.NET.OpenGL;

namespace ShaderDemo.Core.Rendering;

public sealed class PostEffectsPipeline : IDisposable
{
    private readonly GL _gl;
    private readonly FullscreenQuad _quad;

    private readonly ShaderProgram _bloomDownsample;
    private readonly ShaderProgram _bloomBlur;
    private readonly ShaderProgram _bloomCombine;
    private readonly ShaderProgram _transition;
    private readonly ShaderProgram _motionBlur;
    private readonly ShaderProgram _datamosh;
    private readonly ShaderProgram _feedback;
    private readonly ShaderProgram _fade;
    private readonly ShaderProgram _spectrum;
    private readonly ShaderProgram _waveform;
    private readonly ShaderProgram _overlay;
    private readonly ShaderProgram _texturePass;

    public Framebuffer BloomQuarter { get; private set; }
    public Framebuffer BloomBlurBuffer { get; private set; }

    public PostEffectsPipeline(GL gl, int width, int height)
    {
        _gl = gl;
        _quad = new FullscreenQuad(gl);

        _bloomDownsample = new ShaderProgram(gl, "BloomDownsample", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentBloomDownsample);
        _bloomBlur = new ShaderProgram(gl, "BloomBlur", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentBloomBlur);
        _bloomCombine = new ShaderProgram(gl, "BloomCombine", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentBloomCombine);
        _transition = new ShaderProgram(gl, "Transition", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentTransition);
        _motionBlur = new ShaderProgram(gl, "MotionBlur", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentMotionBlur);
        _datamosh = new ShaderProgram(gl, "Datamosh", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentDatamosh);
        _feedback = new ShaderProgram(gl, "Feedback", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentFeedback);
        _fade = new ShaderProgram(gl, "Fade", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentFade);
        _spectrum = new ShaderProgram(gl, "Spectrum", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentSpectrum);
        _waveform = new ShaderProgram(gl, "Waveform", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentWaveform);
        _overlay = new ShaderProgram(gl, "Overlay", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentOverlay);
        _texturePass = new ShaderProgram(gl, "TexturePass2", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentTexture);

        BloomQuarter = Framebuffer.CreateScaled(gl, width, height, 0.25f);
        BloomBlurBuffer = Framebuffer.CreateScaled(gl, width, height, 0.25f);
    }

    public void Resize(int width, int height)
    {
        BloomQuarter.Dispose();
        BloomBlurBuffer.Dispose();
        BloomQuarter = Framebuffer.CreateScaled(_gl, width, height, 0.25f);
        BloomBlurBuffer = Framebuffer.CreateScaled(_gl, width, height, 0.25f);
    }

    public void RenderBloom(Framebuffer input, Framebuffer output, float intensity)
    {
        BloomQuarter.Use();
        input.BindTexture(0);
        _bloomDownsample.Use();
        _bloomDownsample.SetUniform("tex", 0);
        _quad.Draw();

        BloomBlurBuffer.Use();
        BloomQuarter.BindTexture(0);
        _bloomBlur.Use();
        _bloomBlur.SetUniform("tex", 0);
        _bloomBlur.SetUniform("dir", new Vector2(1.0f / BloomQuarter.Width, 0.0f));
        _quad.Draw();

        BloomQuarter.Use();
        BloomBlurBuffer.BindTexture(0);
        _bloomBlur.Use();
        _bloomBlur.SetUniform("tex", 0);
        _bloomBlur.SetUniform("dir", new Vector2(0.0f, 1.0f / BloomQuarter.Height));
        _quad.Draw();

        output.Use();
        input.BindTexture(0);
        BloomQuarter.BindTexture(1);
        _bloomCombine.Use();
        _bloomCombine.SetUniform("tex_scene", 0);
        _bloomCombine.SetUniform("tex_bloom", 1);
        _bloomCombine.SetUniform("intensity", intensity);
        _quad.Draw();
    }

    public void RenderTransition(Framebuffer prev, Framebuffer next, float progress, int type, Framebuffer target)
    {
        target.Use();
        target.Clear(0.0f, 0.0f, 0.0f, 1.0f);

        prev.BindTexture(0);
        next.BindTexture(1);
        _transition.Use();
        _transition.SetUniform("tex_prev", 0);
        _transition.SetUniform("tex_next", 1);
        _transition.SetUniform("progress", progress);
        _transition.SetUniform("type", type);
        _quad.Draw();
    }

    public void RenderMotionBlur(Framebuffer scene, Framebuffer accumIn, Framebuffer accumOut, float amount)
    {
        accumOut.Use();
        scene.BindTexture(0);
        accumIn.BindTexture(1);
        _motionBlur.Use();
        _motionBlur.SetUniform("tex_new", 0);
        _motionBlur.SetUniform("tex_old", 1);
        _motionBlur.SetUniform("blur_amount", amount);
        _quad.Draw();
    }

    public void RenderDatamosh(Framebuffer scene, Framebuffer accumIn, Framebuffer accumOut, float amount, float time)
    {
        accumOut.Use();
        scene.BindTexture(0);
        accumIn.BindTexture(1);
        _datamosh.Use();
        _datamosh.SetUniform("tex_new", 0);
        _datamosh.SetUniform("tex_old", 1);
        _datamosh.SetUniform("amount", amount);
        _datamosh.SetUniform("time", time);
        _quad.Draw();
    }

    public void RenderFeedback(Framebuffer scene, Framebuffer accumIn, Framebuffer accumOut, float opacity, float zoom, float rotation)
    {
        accumOut.Use();
        scene.BindTexture(0);
        accumIn.BindTexture(1);
        _feedback.Use();
        _feedback.SetUniform("tex_new", 0);
        _feedback.SetUniform("tex_old", 1);
        _feedback.SetUniform("zoom", zoom);
        _feedback.SetUniform("rotation", rotation);
        _feedback.SetUniform("opacity", opacity);
        _quad.Draw();
    }

    public void RenderFade(Framebuffer source, Framebuffer target, float decay)
    {
        target.Use();
        source.BindTexture(0);
        _fade.Use();
        _fade.SetUniform("tex", 0);
        _fade.SetUniform("decay", decay);
        _quad.Draw();
    }

    public void RenderSpectrumBar(Texture spectrumTexture, Vector4 color, float opacity)
    {
        spectrumTexture.Bind(0);
        _spectrum.Use();
        _spectrum.SetUniform("tex_spectrum", 0);
        _spectrum.SetUniform("color", color);
        _spectrum.SetUniform("opacity", opacity);
        _quad.Draw();
    }

    public void RenderWaveformBar(Texture waveformTexture, Vector4 color, float opacity)
    {
        waveformTexture.Bind(0);
        _waveform.Use();
        _waveform.SetUniform("tex_waveform", 0);
        _waveform.SetUniform("color", color);
        _waveform.SetUniform("opacity", opacity);
        _quad.Draw();
    }

    public void RenderOverlay(Texture textTexture, float alpha, Vector2 uvOffset, float glitch, float time, float wave, float rainbow, float typewriterProgress)
    {
        textTexture.Bind(0);
        _overlay.Use();
        _overlay.SetUniform("text_texture", 0);
        _overlay.SetUniform("alpha", alpha);
        _overlay.SetUniform("uv_offset", uvOffset);
        _overlay.SetUniform("glitch_intensity", glitch);
        _overlay.SetUniform("time", time);
        _overlay.SetUniform("wave_intensity", wave);
        _overlay.SetUniform("rainbow_intensity", rainbow);
        _overlay.SetUniform("typewriter_progress", typewriterProgress);
        _quad.Draw();
    }

    public void CopyToTarget(Framebuffer source, Framebuffer target)
    {
        target.Use();
        target.Clear(0.0f, 0.0f, 0.0f, 1.0f);
        source.BindTexture(0);
        _texturePass.Use();
        _texturePass.SetUniform("tex", 0);
        _quad.Draw();
    }

    public void Dispose()
    {
        _quad.Dispose();
        _bloomDownsample.Dispose();
        _bloomBlur.Dispose();
        _bloomCombine.Dispose();
        _transition.Dispose();
        _motionBlur.Dispose();
        _datamosh.Dispose();
        _feedback.Dispose();
        _fade.Dispose();
        _spectrum.Dispose();
        _waveform.Dispose();
        _overlay.Dispose();
        _texturePass.Dispose();
        BloomQuarter.Dispose();
        BloomBlurBuffer.Dispose();
    }
}
