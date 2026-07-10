// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;

namespace ShaderDemo.Core.Gui;

public static class Motion
{
    public static float EaseIn(float t) => t * t;
    public static float EaseOut(float t) => t * (2 - t);
    public static float Smooth(float t) => t * t * (3 - 2 * t);
    public static float Smoother(float t) => t * t * t * (t * (t * 6 - 15) + 10);
}

public sealed class FadeTracker
{
    private object? _lastKey;
    private double _changedAt = -1000.0;

    public float Value(object key, float durationSeconds = 0.18f)
    {
        double now = ImGui.GetTime();
        if (!Equals(key, _lastKey))
        {
            _lastKey = key;
            _changedAt = now;
        }

        float t = (float)System.Math.Clamp((now - _changedAt) / durationSeconds, 0.0, 1.0);
        return Motion.EaseOut(t);
    }
}
