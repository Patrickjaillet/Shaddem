// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using System.Runtime.Versioning;
using Silk.NET.OpenGL;
using ShaderDemo.Core.Audio;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.Core.Rendering;

[SupportedOSPlatform("windows")]
public sealed class TextOverlay : IDisposable
{
    private readonly GL _gl;
    private Texture? _texture;
    private TimelineClip? _currentClip;

    public TextOverlay(GL gl)
    {
        _gl = gl;
    }

    public void Update(TimelineClip? clip, int width, int height, Action<string>? log = null)
    {
        if (clip == null)
        {
            _currentClip = null;
            return;
        }

        if (ReferenceEquals(clip, _currentClip)) return;

        _currentClip = clip;

        int size = clip.TryGetDouble("size", out double s) ? (int)s : 60;
        float[] colorArr = clip.TryGetDoubleArray("color", out double[] cArr)
            ? new[] { (float)cArr[0], (float)cArr[1], (float)cArr[2] }
            : new[] { 255f, 255f, 255f };
        string position = clip.GetString("position") ?? "center";

        _texture?.Dispose();
        _texture = TextTextureGenerator.Create(_gl, width, height, clip.Resource, size, new Vector3(colorArr[0], colorArr[1], colorArr[2]), position, log);
    }

    public void Render(PostEffectsPipeline pipeline, Framebuffer target, TimelineClip clip, float currentTime, AudioReactivity? audio)
    {
        if (_texture == null) return;

        float alpha = 1.0f;
        float fadeIn = clip.TryGetDouble("fade_in", out double fi) ? (float)fi : 0.0f;
        float fadeOut = clip.TryGetDouble("fade_out", out double fo) ? (float)fo : 0.0f;

        if (fadeIn > 0)
        {
            float elapsed = currentTime - (float)clip.Start;
            alpha = Math.Min(alpha, elapsed / fadeIn);
        }

        if (fadeOut > 0)
        {
            float remaining = (float)clip.End - currentTime;
            alpha = Math.Min(alpha, remaining / fadeOut);
        }

        alpha = Math.Clamp(alpha, 0.0f, 1.0f);

        float scrollX = clip.TryGetDouble("scroll_x", out double sx) ? (float)sx : 0.0f;
        float scrollY = clip.TryGetDouble("scroll_y", out double sy) ? (float)sy : 0.0f;
        float glitch = clip.TryGetDouble("glitch", out double g) ? (float)g : 0.0f;

        bool audioReactive = clip.GetString("audio_reactive") == "true" || (clip.TryGetDouble("audio_reactive", out double ar) && ar != 0.0);
        if (audioReactive && audio != null && audio.TryGetBassValue(currentTime, out float bass))
        {
            glitch *= bass * 2.0f;
        }

        Vector2 uvOffset = Vector2.Zero;
        if (scrollX != 0 || scrollY != 0)
        {
            float elapsed = currentTime - (float)clip.Start;
            uvOffset = new Vector2(scrollX * elapsed, scrollY * elapsed);
        }

        target.Use();
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        pipeline.RenderOverlay(_texture, alpha, uvOffset, glitch, currentTime, 0.0f, 0.0f, 1.0f);
        _gl.Disable(EnableCap.Blend);
    }

    public void Dispose()
    {
        _texture?.Dispose();
    }
}
