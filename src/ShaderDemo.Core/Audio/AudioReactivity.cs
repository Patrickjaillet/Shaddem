// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Audio;

public sealed class AudioReactivity : IDisposable
{
    private readonly Random _random = new();

    public float[]? BassEnvelope { get; private set; }
    public float[]? MidEnvelope { get; private set; }
    public float[]? TrebleEnvelope { get; private set; }
    public float BassDuration { get; private set; }
    public float MusicStartTime { get; set; }
    public bool Enabled { get; set; } = true;

    public LiveAudioAnalyzer? LiveInput { get; private set; }
    public bool IsLiveInputActive => LiveInput is { IsCapturing: true };

    public void Load(string filePath, float engineTimeNow)
    {
        StopLiveInput();

        (float[] bass, float[] mid, float[] treble, double duration) = AudioAnalyzer.Analyze(filePath);
        BassEnvelope = bass.Length > 0 ? bass : null;
        MidEnvelope = mid.Length > 0 ? mid : null;
        TrebleEnvelope = treble.Length > 0 ? treble : null;
        BassDuration = (float)duration;
        MusicStartTime = engineTimeNow;
    }

    public void Clear()
    {
        BassEnvelope = null;
        MidEnvelope = null;
        TrebleEnvelope = null;
        BassDuration = 0f;
    }

    public void StartLiveInput(int deviceNumber = 0)
    {
        Clear();
        LiveInput?.Dispose();
        LiveInput = new LiveAudioAnalyzer();
        LiveInput.Start(deviceNumber);
    }

    public void StopLiveInput()
    {
        LiveInput?.Dispose();
        LiveInput = null;
    }

    public bool TryGetBassValue(float currentTime, out float value) => TryGetBandValue(BassEnvelope, currentTime, live => live.CurrentBass, out value);

    public bool TryGetMidValue(float currentTime, out float value) => TryGetBandValue(MidEnvelope, currentTime, live => live.CurrentMid, out value);

    public bool TryGetTrebleValue(float currentTime, out float value) => TryGetBandValue(TrebleEnvelope, currentTime, live => live.CurrentTreble, out value);

    private bool TryGetBandValue(float[]? envelope, float currentTime, Func<LiveAudioAnalyzer, float> liveSelector, out float value)
    {
        value = 0f;
        if (!Enabled) return false;

        if (IsLiveInputActive)
        {
            value = liveSelector(LiveInput!);
            return true;
        }

        if (envelope == null || BassDuration <= 0f) return false;

        float audioTime = currentTime - MusicStartTime;
        if (audioTime < 0f || audioTime >= BassDuration) return false;

        int idx = (int)((audioTime / BassDuration) * envelope.Length);
        if (idx < 0 || idx >= envelope.Length) return false;

        value = envelope[idx];
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
            RotationSpeed = effects.RotationSpeed,
        };

        bool hasBass = BassEnvelope != null || IsLiveInputActive;

        if (effects.Shake > 0)
        {
            float intensity = effects.Shake;
            if (hasBass && Enabled)
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
            if (hasBass && Enabled)
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

        if (effects.RotationSpeed != 0f && TryGetTrebleValue(currentTime, out float trebleForRotation))
        {
            result.RotationSpeed = effects.RotationSpeed * (1.0f + (trebleForRotation * effects.Intensity));
        }

        return result;
    }

    public void Dispose()
    {
        LiveInput?.Dispose();
        LiveInput = null;
    }
}
