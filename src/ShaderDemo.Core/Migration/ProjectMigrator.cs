// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.Core.Migration;

public sealed class MigrationResult
{
    public AppSettings? Settings { get; set; }
    public List<Layer>? Layers { get; set; }
    public TimelineEngine? Timeline { get; set; }
}

public static class ProjectMigrator
{
    public static MigrationResult MigrateFromPythonProject(string pythonProjectDirectory, Action<string>? log = null)
    {
        var result = new MigrationResult();

        string settingsPath = Path.Combine(pythonProjectDirectory, "settings", "settings.py");
        if (File.Exists(settingsPath))
        {
            try
            {
                result.Settings = SettingsMigrator.Migrate(settingsPath, log);
            }
            catch (Exception ex)
            {
                log?.Invoke($"Settings migration failed: {ex.Message}");
            }
        }
        else
        {
            log?.Invoke($"No settings.py found at {settingsPath}, skipped");
        }

        string layersPath = Path.Combine(pythonProjectDirectory, "layers.json");
        if (File.Exists(layersPath))
        {
            try
            {
                result.Layers = LayersMigrator.Migrate(layersPath, log);
            }
            catch (Exception ex)
            {
                log?.Invoke($"Layers migration failed: {ex.Message}");
            }
        }
        else
        {
            log?.Invoke($"No layers.json found at {layersPath}, skipped");
        }

        string timelineDbPath = Path.Combine(pythonProjectDirectory, "timeline.db");
        if (File.Exists(timelineDbPath))
        {
            try
            {
                result.Timeline = TimelineMigrator.Migrate(timelineDbPath, log);
            }
            catch (Exception ex)
            {
                log?.Invoke($"Timeline migration failed: {ex.Message}");
            }
        }
        else
        {
            log?.Invoke($"No timeline.db found at {timelineDbPath}, skipped");
        }

        return result;
    }

    public static void SaveMigrationResult(MigrationResult result, string settingsFilePath, string layersFilePath, string timelineFilePath, Action<string>? log = null)
    {
        if (result.Settings != null)
        {
            SettingsService.Save(result.Settings, settingsFilePath);
            log?.Invoke($"Saved migrated settings to {settingsFilePath}");
        }

        if (result.Layers != null)
        {
            LayerPersistence.Save(result.Layers, layersFilePath);
            log?.Invoke($"Saved migrated layers to {layersFilePath}");
        }

        if (result.Timeline != null)
        {
            TimelinePersistence.Save(result.Timeline, timelineFilePath);
            log?.Invoke($"Saved migrated timeline to {timelineFilePath}");
        }
    }
}
