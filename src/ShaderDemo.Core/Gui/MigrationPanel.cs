// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Migration;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.Core.Gui;

public static class MigrationPanel
{
    private static string _projectDirectory = "";
    private static readonly List<string> _log = new();

    public static void Draw(
        ShaderManager manager,
        AppSettings settings,
        TimelineEngine timeline,
        string settingsFilePath,
        string layersFilePath,
        string timelineFilePath)
    {
        ImGui.Text("Import a project from the Python version");
        ImGui.TextWrapped("Reads settings/settings.py, layers.json and timeline.db from the given folder and converts them into this project's JSON format.");
        ImGui.InputText("Legacy Project Folder", ref _projectDirectory, 512);

        if (ImGui.Button("Migrate"))
        {
            _log.Clear();
            MigrationResult result = ProjectMigrator.MigrateFromPythonProject(_projectDirectory, _log.Add);

            if (result.Settings != null)
            {
                settings.WindowWidth = result.Settings.WindowWidth;
                settings.WindowHeight = result.Settings.WindowHeight;
                settings.ShaderSwitchInterval = result.Settings.ShaderSwitchInterval;
                settings.MusicFile = result.Settings.MusicFile;
                settings.MusicVolume = result.Settings.MusicVolume;
                settings.AudioReactive = result.Settings.AudioReactive;
                settings.AutoSaveSettings = result.Settings.AutoSaveSettings;
                settings.Effects.CopyFrom(result.Settings.Effects);
                manager.Effects.CopyFrom(result.Settings.Effects);
                manager.Audio.Enabled = settings.AudioReactive;
            }

            if (result.Layers != null)
            {
                manager.Layers.Clear();
                manager.Layers.AddRange(result.Layers);
            }

            if (result.Timeline != null)
            {
                timeline.Clips.Clear();
                timeline.Clips.AddRange(result.Timeline.Clips);
                timeline.Markers.Clear();
                timeline.Markers.AddRange(result.Timeline.Markers);
            }

            ProjectMigrator.SaveMigrationResult(result, settingsFilePath, layersFilePath, timelineFilePath, _log.Add);
        }

        if (_log.Count > 0)
        {
            ImGui.BeginChild("MigrationLog", new System.Numerics.Vector2(0, 120), true);
            foreach (string line in _log)
            {
                ImGui.TextWrapped(line);
            }

            ImGui.EndChild();
        }
    }
}
