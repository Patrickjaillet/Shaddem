// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;

namespace ShaderDemo.Core.Timeline;

public sealed class TimelineMarker
{
    public double Time { get; set; }
    public string Label { get; set; }
    public Vector4 Color { get; set; }

    public TimelineMarker(double time, string label, Vector4 color)
    {
        Time = time;
        Label = label;
        Color = color;
    }
}
