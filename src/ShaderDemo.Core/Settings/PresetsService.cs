// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Settings;

public static class PresetsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true,
    };

    public static Dictionary<string, EffectParams> Load(string filePath)
    {
        if (!File.Exists(filePath)) return new Dictionary<string, EffectParams>();

        string json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<Dictionary<string, EffectParams>>(json, JsonOptions) ?? new Dictionary<string, EffectParams>();
    }

    public static void Save(Dictionary<string, EffectParams> presets, string filePath)
    {
        File.WriteAllText(filePath, JsonSerializer.Serialize(presets, JsonOptions));
    }

    public static void CreatePreset(Dictionary<string, EffectParams> presets, string name, EffectParams current, string filePath)
    {
        var snapshot = new EffectParams();
        snapshot.CopyFrom(current);
        presets[name] = snapshot;
        Save(presets, filePath);
    }

    public static void ApplyPreset(Dictionary<string, EffectParams> presets, string name, EffectParams target)
    {
        if (presets.TryGetValue(name, out EffectParams? preset))
        {
            target.CopyFrom(preset);
        }
    }

    public static string GenerateRandomPreset(EffectParams s, Random random)
    {
        ResetToNeutral(s);

        string[] themes = { "retro", "art", "geo", "trippy", "dark" };
        string theme = themes[random.Next(themes.Length)];

        bool Chance(double p) => random.NextDouble() < p;
        float Uniform(float a, float b) => a + ((float)random.NextDouble() * (b - a));

        switch (theme)
        {
            case "retro":
                s.Scanlines = Uniform(0.3f, 0.8f);
                s.Vhs = Uniform(0.2f, 0.5f);
                s.RgbSplit = Uniform(0.01f, 0.03f);
                if (Chance(0.5)) s.Crt = Uniform(0.2f, 0.4f);
                if (Chance(0.3)) s.Pixelate = Uniform(2.0f, 4.0f);
                break;
            case "art":
                if (Chance(0.5)) s.EdgeDetect = Uniform(0.5f, 1.0f);
                else if (Chance(0.5)) s.SobelNeon = Uniform(0.5f, 1.0f);
                else if (Chance(0.5)) s.Halftone = Uniform(0.5f, 1.0f);
                else s.Crosshatch = Uniform(0.5f, 1.0f);
                break;
            case "geo":
                s.Mirror = random.Next(1, 4);
                if (Chance(0.5)) s.Kaleidoscope = Uniform(0.5f, 1.0f);
                if (Chance(0.3)) s.Polar = 1.0f;
                break;
            case "trippy":
                s.HueShift = Uniform(0.1f, 1.0f);
                s.Wave = Uniform(0.5f, 2.0f);
                s.Swirl = Uniform(-3.0f, 3.0f);
                if (Chance(0.3)) s.Invert = 1.0f;
                break;
            case "dark":
                s.Vignette = Uniform(0.8f, 1.5f);
                s.FilmGrain = Uniform(0.2f, 0.5f);
                s.Saturation = Uniform(0.0f, 0.5f);
                s.Contrast = Uniform(1.2f, 1.5f);
                if (Chance(0.5)) s.NightVision = Uniform(0.5f, 1.0f);
                break;
        }

        if (Chance(0.2)) s.Bloom = Uniform(0.2f, 0.8f);
        if (Chance(0.2)) s.ChromaticAberration = Uniform(0.1f, 0.4f);
        if (Chance(0.1)) s.Color = new Vector4((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble(), 1.0f);

        return theme;
    }

    private static void ResetToNeutral(EffectParams s)
    {
        foreach (FieldInfo field in typeof(EffectParams).GetFields())
        {
            if (field.FieldType == typeof(float))
            {
                field.SetValue(s, 0.0f);
            }
        }

        s.Brightness = 1.0f;
        s.Contrast = 1.0f;
        s.Saturation = 1.0f;
        s.Scale = 1.0f;
        s.Speed = 1.0f;
        s.Intensity = 1.0f;
        s.Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        s.TintColor = Vector3.One;
    }
}
