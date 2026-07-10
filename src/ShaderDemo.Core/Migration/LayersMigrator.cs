// Copyright (c) 2026 Patrick JAILLET
using System.Text.Json;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Migration;

public static class LayersMigrator
{
    public static List<Layer> Migrate(string pythonLayersFilePath, Action<string>? log = null)
    {
        var layers = new List<Layer>();

        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(pythonLayersFilePath));
        foreach (JsonElement item in doc.RootElement.EnumerateArray())
        {
            string shaderName = item.GetProperty("shader_name").GetString() ?? "";
            var blendMode = (BlendMode)item.GetProperty("blend_mode").GetInt32();
            float opacity = item.GetProperty("opacity").GetSingle();
            bool enabled = item.GetProperty("enabled").GetBoolean();

            layers.Add(new Layer(shaderName, blendMode, opacity, enabled));
        }

        log?.Invoke($"Layers migration: {layers.Count} layer(s) imported");
        return layers;
    }
}
