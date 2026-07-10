// Copyright (c) 2026 Patrick JAILLET
using System.Text.Json;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Settings;

public static class ModelPresetsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true,
    };

    public static Dictionary<string, Model3DState> Load(string filePath)
    {
        if (!File.Exists(filePath)) return new Dictionary<string, Model3DState>();

        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<Dictionary<string, Model3DState>>(json, JsonOptions) ?? new Dictionary<string, Model3DState>();
    }

    public static void Save(Dictionary<string, Model3DState> presets, string filePath)
    {
        File.WriteAllText(filePath, JsonSerializer.Serialize(presets, JsonOptions));
    }

    public static void CreatePreset(Dictionary<string, Model3DState> presets, string name, Model3DState current, string filePath)
    {
        var snapshot = new Model3DState
        {
            Position = current.Position,
            Rotation = current.Rotation,
            Scale = current.Scale,
            AutoRotateSpeed = current.AutoRotateSpeed,
            LightDir = current.LightDir,
            Wireframe = current.Wireframe,
            WireframeColor = current.WireframeColor,
            ShowModel = current.ShowModel,
            CurrentModelFilename = current.CurrentModelFilename,
            CurrentTextureFilename = current.CurrentTextureFilename,
        };

        presets[name] = snapshot;
        Save(presets, filePath);
    }

    public static void ApplyPreset(Dictionary<string, Model3DState> presets, string name, Model3DState target)
    {
        if (!presets.TryGetValue(name, out Model3DState? preset)) return;

        target.Position = preset.Position;
        target.Rotation = preset.Rotation;
        target.Scale = preset.Scale;
        target.AutoRotateSpeed = preset.AutoRotateSpeed;
        target.LightDir = preset.LightDir;
        target.Wireframe = preset.Wireframe;
        target.WireframeColor = preset.WireframeColor;
        target.ShowModel = preset.ShowModel;
    }
}
