// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Audio;

public sealed class AudioReactivity
{
    private readonly Random _random = new();

    public float[]? BassEnvelope { get; private set; }
    public float BassDuration { get; private set; }
    public float MusicStartTime { get; set; }
    public bool Enabled { get; set; } = true;

    public void Load(string filePath, float engineTimeNow)
    {
        (float[] envelope, double duration) = AudioAnalyzer.Analyze(filePath);
        BassEnvelope = envelope.Length > 0 ? envelope : null;
        BassDuration = (float)duration;
        MusicStartTime = engineTimeNow;
    }

    public void Clear()
    {
        BassEnvelope = null;
        BassDuration = 0f;
    }

    public bool TryGetBassValue(float currentTime, out float value)
    {
        value = 0f;
        if (BassEnvelope == null || !Enabled || BassDuration <= 0f) return false;

        float audioTime = currentTime - MusicStartTime;
        if (audioTime < 0f || audioTime >= BassDuration) return false;

        int idx = (int)((audioTime / BassDuration) * BassEnvelope.Length);
        if (idx < 0 || idx >= BassEnvelope.Length) return false;

        value = BassEnvelope[idx];
        return true;
    }

    public AudioUniforms Compute(EffectParams effects, float currentTime)
    {
        var result = new AudioUniforms
        {
            ShakeOffset = Vector2.Zero,
            Strobe = 0f,
            RgbSplit = effects.RgbSplit,
            Scale = effects.Scale,
            Kick = 0f,
        };

        if (effects.Shake > 0)
        {
            float intensity = effects.Shake;
            if (BassEnvelope != null && Enabled)
            {
                intensity = TryGetBassValue(currentTime, out float bass) ? intensity * bass * 30.0f : 0f;
            }
            else
            {
                intensity *= 5.0f;
            }

            if (intensity > 0)
            {
                result.ShakeOffset = new Vector2(
                    (float)((_random.NextDouble() * 2) - 1) * intensity,
                    (float)((_random.NextDouble() * 2) - 1) * intensity);
            }
        }

        if (effects.Strobe > 0)
        {
            float intensity = effects.Strobe;
            float strobe;
            if (BassEnvelope != null && Enabled)
            {
                strobe = TryGetBassValue(currentTime, out float bass) && bass > 0.5f
                    ? (bass - 0.5f) * 2.0f * intensity
                    : 0f;
            }
            else
            {
                strobe = ((MathF.Sin(currentTime * 30.0f) * 0.5f) + 0.5f) * intensity;
            }

            result.Strobe = Math.Clamp(strobe, 0f, 1f);
        }

        if (TryGetBassValue(currentTime, out float bassForSplit))
        {
            result.RgbSplit = effects.RgbSplit + (bassForSplit * 0.05f * effects.Intensity);
        }

        if (TryGetBassValue(currentTime, out float bassForScale))
        {
            result.Scale = effects.Scale + (bassForScale * 0.3f * effects.Intensity);
        }

        if (effects.KickIntensity > 0)
        {
            float kick = TryGetBassValue(currentTime, out float bassForKick) ? bassForKick * effects.KickIntensity : 0f;
            result.Kick = Math.Clamp(kick, 0f, 1f);
        }

        return result;
    }
}
