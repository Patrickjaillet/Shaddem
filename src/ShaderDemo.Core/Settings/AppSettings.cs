// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Settings;

public sealed class AppSettings
{
    public int WindowWidth { get; set; } = 1920;
    public int WindowHeight { get; set; } = 1080;
    public int ShaderSwitchInterval { get; set; } = 30;

    public EffectParams Effects { get; set; } = new();

    public string MusicFile { get; set; } = "audio/music.wav";
    public float MusicVolume { get; set; } = 0.5f;
    public bool AudioReactive { get; set; } = true;
    public bool AutoSaveSettings { get; set; } = true;

    public bool FirstRun { get; set; } = true;
    public bool TourCompleted { get; set; } = false;
}
