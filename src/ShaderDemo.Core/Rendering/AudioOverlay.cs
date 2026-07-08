// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using Silk.NET.OpenGL;
using ShaderDemo.Core.Audio;

namespace ShaderDemo.Core.Rendering;

public enum AudioVizMode
{
    Spectrum,
    Waveform,
}

public sealed class AudioOverlay : IDisposable
{
    private readonly GL _gl;

    private Framebuffer? _pingBuffer;
    private Framebuffer? _pongBuffer;
    private Texture? _spectrumTexture;
    private Texture? _waveformTexture;
    private int _waveformWidth = -1;

    public bool Enabled;
    public AudioVizMode Mode = AudioVizMode.Spectrum;
    public float Height = 0.2f;
    public float Opacity = 0.8f;
    public Vector4 Color = new(0.0f, 1.0f, 1.0f, 1.0f);
    public float TrailDecay = 0.0f;

    public AudioAnalysisResult? Analysis { get; private set; }
    public double MusicStartTime { get; set; }

    public AudioOverlay(GL gl)
    {
        _gl = gl;
    }

    public void SetAnalysis(AudioAnalysisResult analysis, double musicStartTime)
    {
        Analysis = analysis;
        MusicStartTime = musicStartTime;
        _spectrumTexture?.Dispose();
        _spectrumTexture = Texture.Create(_gl, AudioAnalyzer.SpectrumBandsCount, 1, new byte[AudioAnalyzer.SpectrumBandsCount * 4 * 3], hasAlpha: false, buildMipmaps: false);
    }

    public unsafe void Render(PostEffectsPipeline pipeline, Framebuffer target, float currentTime, int screenWidth, int screenHeight)
    {
        if (!Enabled || Analysis == null) return;

        double audioTime = currentTime - MusicStartTime;
        if (audioTime < 0 || audioTime >= Analysis.Duration) return;

        int frameCount = Mode == AudioVizMode.Spectrum ? Analysis.Spectrum.Length : Analysis.Waveform.Length;
        if (frameCount == 0) return;

        int idx = (int)((audioTime / Analysis.Duration) * frameCount);
        if (idx < 0 || idx >= frameCount) return;

        Texture barTexture;
        if (Mode == AudioVizMode.Spectrum)
        {
            float[] bands = Analysis.Spectrum[idx];
            UploadFloatTextureR(ref _spectrumTexture, bands.Length, bands);
            barTexture = _spectrumTexture!;
        }
        else
        {
            float[] wave = Analysis.Waveform[idx];
            if (_waveformTexture == null || _waveformWidth != wave.Length)
            {
                _waveformTexture?.Dispose();
                _waveformTexture = Texture.Create(_gl, wave.Length, 1, new byte[wave.Length * 4], hasAlpha: false, buildMipmaps: false);
                _waveformWidth = wave.Length;
            }

            UploadFloatTextureR(ref _waveformTexture, wave.Length, wave);
            barTexture = _waveformTexture!;
        }

        EnsureTrailBuffers(screenWidth, screenHeight);

        if (TrailDecay > 0.0f && _pingBuffer != null && _pongBuffer != null)
        {
            _gl.Disable(EnableCap.Blend);
            pipeline.RenderFade(_pingBuffer, _pongBuffer, TrailDecay);

            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            _pongBuffer.Use();
            _gl.Viewport(0, 0, (uint)screenWidth, (uint)(screenHeight * Height));
            DrawBar(pipeline, barTexture);
            _gl.Viewport(0, 0, (uint)screenWidth, (uint)screenHeight);

            (_pingBuffer, _pongBuffer) = (_pongBuffer, _pingBuffer);

            pipeline.CopyToTarget(_pingBuffer, target);
        }
        else
        {
            target.Use();
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            _gl.Viewport(0, 0, (uint)screenWidth, (uint)(screenHeight * Height));
            DrawBar(pipeline, barTexture);
            _gl.Viewport(0, 0, (uint)screenWidth, (uint)screenHeight);
            _gl.Disable(EnableCap.Blend);
        }
    }

    private void DrawBar(PostEffectsPipeline pipeline, Texture texture)
    {
        if (Mode == AudioVizMode.Spectrum) pipeline.RenderSpectrumBar(texture, Color, Opacity);
        else pipeline.RenderWaveformBar(texture, Color, Opacity);
    }

    private void EnsureTrailBuffers(int width, int height)
    {
        if (_pingBuffer != null && _pingBuffer.Width == width && _pingBuffer.Height == height) return;

        _pingBuffer?.Dispose();
        _pongBuffer?.Dispose();
        _pingBuffer = Framebuffer.Create(_gl, width, height);
        _pongBuffer = Framebuffer.Create(_gl, width, height);
    }

    private unsafe void UploadFloatTextureR(ref Texture? texture, int width, float[] data)
    {
        byte[] bytes = new byte[width * 3];
        for (int i = 0; i < width; i++)
        {
            byte v = (byte)Math.Clamp(data[i] * 255f, 0f, 255f);
            bytes[(i * 3) + 0] = v;
            bytes[(i * 3) + 1] = v;
            bytes[(i * 3) + 2] = v;
        }

        texture?.Dispose();
        texture = Texture.Create(_gl, width, 1, bytes, hasAlpha: false, buildMipmaps: false);
    }

    public void Dispose()
    {
        _pingBuffer?.Dispose();
        _pongBuffer?.Dispose();
        _spectrumTexture?.Dispose();
        _waveformTexture?.Dispose();
    }
}
