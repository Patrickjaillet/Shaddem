// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using System.Reflection;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Timeline;

public sealed class TimelineEngine
{
    private const int MaxHistory = 50;

    private readonly List<List<TimelineClip>> _history = new();
    private readonly List<List<TimelineClip>> _redoStack = new();

    public List<TimelineClip> Clips { get; private set; } = new();
    public List<TimelineMarker> Markers { get; } = new();
    public bool Active { get; set; }

    private void SaveState()
    {
        _history.Add(Clips.Select(c => c.Clone()).ToList());
        _redoStack.Clear();
        if (_history.Count > MaxHistory) _history.RemoveAt(0);
    }

    public void Undo()
    {
        if (_history.Count == 0) return;
        _redoStack.Add(Clips.Select(c => c.Clone()).ToList());
        Clips = _history[^1];
        _history.RemoveAt(_history.Count - 1);
    }

    public void Redo()
    {
        if (_redoStack.Count == 0) return;
        _history.Add(Clips.Select(c => c.Clone()).ToList());
        Clips = _redoStack[^1];
        _redoStack.RemoveAt(_redoStack.Count - 1);
    }

    public TimelineClip Add(double start, double duration, ClipType type, string resource, Dictionary<string, object?>? clipParams = null)
    {
        SaveState();
        var clip = new TimelineClip(start, duration, type, resource, clipParams);
        Clips.Add(clip);
        Clips.Sort((a, b) => a.Start.CompareTo(b.Start));
        return clip;
    }

    public void Remove(TimelineClip clip)
    {
        if (!Clips.Contains(clip)) return;
        SaveState();
        Clips.Remove(clip);
    }

    public TimelineClip? Split(TimelineClip clip, double splitTime)
    {
        if (!(clip.Start + 0.01 < splitTime && splitTime < clip.End - 0.01)) return null;

        SaveState();

        double newStart = splitTime;
        double newDuration = clip.End - splitTime;
        var newClip = new TimelineClip(newStart, newDuration, clip.Type, clip.Resource, new Dictionary<string, object?>(clip.Params));

        clip.Duration = splitTime - clip.Start;

        Clips.Add(newClip);
        Clips.Sort((a, b) => a.Start.CompareTo(b.Start));
        return newClip;
    }

    public void AddMarker(double time, string label, Vector4 color)
    {
        Markers.Add(new TimelineMarker(time, label, color));
        Markers.Sort((a, b) => a.Time.CompareTo(b.Time));
    }

    public void RemoveMarker(double time)
    {
        Markers.RemoveAll(m => Math.Abs(m.Time - time) <= 0.001);
    }

    public IEnumerable<TimelineClip> GetActiveClips(double time)
    {
        return Clips.Where(c => c.Start <= time && time < c.End);
    }

    private static double ApplyEasing(double t, string? easing)
    {
        return easing switch
        {
            "ease_in" => t * t,
            "ease_out" => t * (2 - t),
            "smooth" => t * t * (3 - 2 * t),
            "smoother" => t * t * t * (t * (t * 6 - 15) + 10),
            _ => t,
        };
    }

    private static bool TryGetInterpolatedScalar(TimelineClip clip, double currentTime, out double value)
    {
        if (clip.TryGetDouble("value", out value)) return true;

        if (clip.TryGetDouble("start_value", out double start) && clip.TryGetDouble("end_value", out double end))
        {
            value = InterpolateScalar(clip, currentTime, start, end);
            return true;
        }

        value = 0.0;
        return false;
    }

    private static bool TryGetInterpolatedVector(TimelineClip clip, double currentTime, out double[] value)
    {
        if (clip.TryGetDoubleArray("value", out value)) return true;

        if (clip.TryGetDoubleArray("start_value", out double[] start) && clip.TryGetDoubleArray("end_value", out double[] end) && start.Length == end.Length)
        {
            double t = NormalizedProgress(clip, currentTime);
            value = new double[start.Length];
            for (int i = 0; i < start.Length; i++) value[i] = start[i] + (end[i] - start[i]) * t;
            return true;
        }

        value = Array.Empty<double>();
        return false;
    }

    private static double NormalizedProgress(TimelineClip clip, double currentTime)
    {
        if (clip.Duration <= 0.001) return 1.0;
        double t = (currentTime - clip.Start) / clip.Duration;
        t = Math.Clamp(t, 0.0, 1.0);
        return ApplyEasing(t, clip.GetString("easing"));
    }

    private static double InterpolateScalar(TimelineClip clip, double currentTime, double start, double end)
    {
        double t = NormalizedProgress(clip, currentTime);
        return start + (end - start) * t;
    }

    private static string ToPascalCase(string snakeCase)
    {
        string[] parts = snakeCase.Split('_', StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
    }

    public void ApplyEffects(EffectParams effects, double currentTime)
    {
        effects.Speed = 1.0f;
        effects.Intensity = 1.0f;
        effects.Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

        foreach (TimelineClip clip in GetActiveClips(currentTime))
        {
            if (clip.Type != ClipType.Effect) continue;

            FieldInfo? field = typeof(EffectParams).GetField(ToPascalCase(clip.Resource));
            if (field == null) continue;

            if (field.FieldType == typeof(float))
            {
                if (TryGetInterpolatedScalar(clip, currentTime, out double v)) field.SetValue(effects, (float)v);
            }
            else if (field.FieldType == typeof(int))
            {
                if (TryGetInterpolatedScalar(clip, currentTime, out double v)) field.SetValue(effects, (int)v);
            }
            else if (field.FieldType == typeof(bool))
            {
                if (TryGetInterpolatedScalar(clip, currentTime, out double v)) field.SetValue(effects, v != 0.0);
            }
            else if (field.FieldType == typeof(Vector3))
            {
                if (TryGetInterpolatedVector(clip, currentTime, out double[] v) && v.Length >= 3)
                {
                    field.SetValue(effects, new Vector3((float)v[0], (float)v[1], (float)v[2]));
                }
            }
            else if (field.FieldType == typeof(Vector4))
            {
                if (TryGetInterpolatedVector(clip, currentTime, out double[] v) && v.Length >= 4)
                {
                    field.SetValue(effects, new Vector4((float)v[0], (float)v[1], (float)v[2], (float)v[3]));
                }
            }
        }
    }

    public void ApplyShader(ShaderManager manager, double currentTime)
    {
        TimelineClip? shaderClip = GetActiveClips(currentTime).LastOrDefault(c => c.Type == ClipType.Shader);
        if (shaderClip == null) return;

        int index = manager.ShaderNames.ToList().IndexOf(shaderClip.Resource);
        if (index >= 0) manager.SelectShader(index);
    }
}
