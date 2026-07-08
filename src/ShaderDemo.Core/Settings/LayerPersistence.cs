// Copyright (c) 2026 Patrick JAILLET
using System.Text.Json;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Settings;

public static class LayerPersistence
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    private sealed class LayerData
    {
        public string ShaderName { get; set; } = "";
        public BlendMode BlendMode { get; set; }
        public float Opacity { get; set; } = 1.0f;
        public bool Enabled { get; set; } = true;
    }

    public static void Save(IEnumerable<Layer> layers, string filePath)
    {
        var data = layers.Select(l => new LayerData
        {
            ShaderName = l.ShaderName,
            BlendMode = l.BlendMode,
            Opacity = l.Opacity,
            Enabled = l.Enabled,
        }).ToList();

        File.WriteAllText(filePath, JsonSerializer.Serialize(data, JsonOptions));
    }

    public static List<Layer> Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return new List<Layer>();
        }

        string json = File.ReadAllText(filePath);
        List<LayerData>? data = JsonSerializer.Deserialize<List<LayerData>>(json, JsonOptions);
        if (data == null)
        {
            return new List<Layer>();
        }

        return data.Select(d => new Layer(d.ShaderName, d.BlendMode, d.Opacity, d.Enabled)).ToList();
    }
}
