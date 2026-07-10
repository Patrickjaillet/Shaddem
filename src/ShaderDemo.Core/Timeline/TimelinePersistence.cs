// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShaderDemo.Core.Timeline;

public static class TimelinePersistence
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private sealed class ClipData
    {
        public double Start { get; set; }
        public double Duration { get; set; }
        public ClipType Type { get; set; }
        public string Resource { get; set; } = "";
        public Dictionary<string, object?> Params { get; set; } = new();
    }

    private sealed class MarkerData
    {
        public double Time { get; set; }
        public string Label { get; set; } = "";
        public float[] Color { get; set; } = { 1.0f, 1.0f, 0.0f, 1.0f };
    }

    private sealed class TimelineData
    {
        public List<ClipData> Clips { get; set; } = new();
        public List<MarkerData> Markers { get; set; } = new();
    }

    public static void Save(TimelineEngine engine, string filePath)
    {
        var data = new TimelineData
        {
            Clips = engine.Clips.Select(c => new ClipData
            {
                Start = c.Start,
                Duration = c.Duration,
                Type = c.Type,
                Resource = c.Resource,
                Params = c.Params,
            }).ToList(),
            Markers = engine.Markers.Select(m => new MarkerData
            {
                Time = m.Time,
                Label = m.Label,
                Color = new[] { m.Color.X, m.Color.Y, m.Color.Z, m.Color.W },
            }).ToList(),
        };

        File.WriteAllText(filePath, JsonSerializer.Serialize(data, JsonOptions));
    }

    public static void Load(TimelineEngine engine, string filePath)
    {
        if (!File.Exists(filePath)) return;

        string json = File.ReadAllText(filePath);
        TimelineData? data = JsonSerializer.Deserialize<TimelineData>(json, JsonOptions);
        if (data == null) return;

        engine.Clips.Clear();
        foreach (ClipData c in data.Clips)
        {
            engine.Clips.Add(new TimelineClip(c.Start, c.Duration, c.Type, c.Resource, c.Params));
        }

        engine.Markers.Clear();
        foreach (MarkerData m in data.Markers)
        {
            Vector4 color = m.Color.Length >= 4
                ? new Vector4(m.Color[0], m.Color[1], m.Color[2], m.Color[3])
                : new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
            engine.Markers.Add(new TimelineMarker(m.Time, m.Label, color));
        }
    }
}
