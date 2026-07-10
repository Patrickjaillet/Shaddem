// Copyright (c) 2026 Patrick JAILLET
namespace ShaderDemo.Core.Audio;

public sealed class AudioAnalysisResult
{
    public float[] Bass { get; init; } = Array.Empty<float>();
    public float[] Treble { get; init; } = Array.Empty<float>();
    public float[][] Spectrum { get; init; } = Array.Empty<float[]>();
    public float[][] Waveform { get; init; } = Array.Empty<float[]>();
    public double Duration { get; init; }
}
