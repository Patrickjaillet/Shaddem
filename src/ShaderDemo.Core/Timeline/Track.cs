// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;

namespace ShaderDemo.Core.Timeline;

public sealed class Track
{
    public string Name { get; set; }
    public List<ClipType> Types { get; set; }
    public float Height { get; set; }
    public Vector4 Background { get; set; }
    public bool Visible { get; set; } = true;
    public bool Mute { get; set; }
    public bool Solo { get; set; }

    public Track(string name, List<ClipType> types, float height, Vector4 background)
    {
        Name = name;
        Types = types;
        Height = height;
        Background = background;
    }

    public static List<Track> CreateDefaults()
    {
        return new List<Track>
        {
            new("Video", new List<ClipType> { ClipType.Shader, ClipType.Image, ClipType.Model3D }, 40, new Vector4(0.2f, 0.2f, 0.22f, 1.0f)),
            new("Text", new List<ClipType> { ClipType.Text }, 30, new Vector4(0.18f, 0.2f, 0.18f, 1.0f)),
            new("Effects", new List<ClipType> { ClipType.Effect, ClipType.LayerAutomation }, 30, new Vector4(0.2f, 0.18f, 0.2f, 1.0f)),
            new("Audio", new List<ClipType> { ClipType.Music }, 40, new Vector4(0.2f, 0.2f, 0.18f, 1.0f)),
        };
    }
}
