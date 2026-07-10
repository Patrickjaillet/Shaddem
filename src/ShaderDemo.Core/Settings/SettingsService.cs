// Copyright (c) 2026 Patrick JAILLET
using System.Text.Json;

namespace ShaderDemo.Core.Settings;

public static class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true,
    };

    public static void Save(AppSettings settings, string filePath)
    {
        string json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(filePath, json);
    }

    public static AppSettings Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return new AppSettings();
        }

        string json = File.ReadAllText(filePath);
        AppSettings settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        Validate(settings);
        return settings;
    }

    private static void Validate(AppSettings settings)
    {
        settings.WindowWidth = Math.Clamp(settings.WindowWidth, 320, 7680);
        settings.WindowHeight = Math.Clamp(settings.WindowHeight, 240, 4320);
        settings.ShaderSwitchInterval = Math.Clamp(settings.ShaderSwitchInterval, 1, 3600);
        settings.MusicVolume = Math.Clamp(settings.MusicVolume, 0.0f, 1.0f);
        settings.Effects.ClampToValidRanges();
    }
}
