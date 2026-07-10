// Copyright (c) 2026 Patrick JAILLET
using System.Text.Json;

namespace ShaderDemo.Core.Timeline;

public sealed class TimelineClip
{
    public double Start { get; set; }
    public double Duration { get; set; }
    public double End => Start + Duration;
    public ClipType Type { get; set; }
    public string Resource { get; set; }
    public Dictionary<string, object?> Params { get; set; }

    public TimelineClip(double start, double duration, ClipType type, string resource, Dictionary<string, object?>? clipParams = null)
    {
        Start = start;
        Duration = duration;
        Type = type;
        Resource = resource;
        Params = clipParams ?? new Dictionary<string, object?>();
    }

    public TimelineClip Clone()
    {
        return new TimelineClip(Start, Duration, Type, Resource, new Dictionary<string, object?>(Params));
    }

    public bool TryGetDouble(string key, out double value)
    {
        value = 0.0;
        if (!Params.TryGetValue(key, out object? raw) || raw is null) return false;

        switch (raw)
        {
            case double d: value = d; return true;
            case float f: value = f; return true;
            case int i: value = i; return true;
            case JsonElement { ValueKind: JsonValueKind.Number } el: value = el.GetDouble(); return true;
            default: return false;
        }
    }

    public bool TryGetDoubleArray(string key, out double[] value)
    {
        value = Array.Empty<double>();
        if (!Params.TryGetValue(key, out object? raw) || raw is null) return false;

        switch (raw)
        {
            case double[] arr: value = arr; return true;
            case JsonElement { ValueKind: JsonValueKind.Array } el:
                value = el.EnumerateArray().Select(x => x.GetDouble()).ToArray();
                return true;
            default: return false;
        }
    }

    public string? GetString(string key)
    {
        if (!Params.TryGetValue(key, out object? raw) || raw is null) return null;

        return raw switch
        {
            string s => s,
            JsonElement { ValueKind: JsonValueKind.String } el => el.GetString(),
            _ => null,
        };
    }
}
