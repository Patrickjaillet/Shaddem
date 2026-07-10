// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Export;
using ShaderDemo.Core.Logging;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.Core.Gui;

public static class SystemPanel
{
    private static string _projectName = "default";
    private static string _projectStatus = "";

    public static void Draw(ShaderManager manager, AppSettings settings, TimelineEngine timeline, string settingsFilePath, string layersFilePath, string timelineFilePath, GlWindow window)
    {
        Theme.Mono($"FPS: {ImGui.GetIO().Framerate:F1}  |  Time: {manager.ElapsedTime:F1}s");

        if (ImGui.Button("Screenshot"))
        {
            string path = ScreenshotService.Save(manager.LastComposedFrame ?? manager.Pipeline.SceneFbo, "screenshots");
            AppLog.Info($"Screenshot: {path}");
            ToastManager.Show($"Screenshot saved: {path}", ToastLevel.Success);
        }

        ImGui.SameLine();
        if (ImGui.Button("Reset All Effects"))
        {
            manager.Effects.CopyFrom(new EffectParams());
        }

        ImGui.Separator();
        ImGui.Text("Project");

        if (ImGui.Button("New Project (Reset)"))
        {
            ImGui.OpenPopup("Confirm New Project");
        }

        if (ImGui.BeginPopupModal("Confirm New Project"))
        {
            ImGui.TextColored(Theme.Danger, "Reset all effects, layers, and timeline to defaults? Unsaved changes will be lost.");
            if (ImGui.Button("Reset"))
            {
                manager.Effects.CopyFrom(new EffectParams());
                manager.Layers.Clear();
                timeline.Clips.Clear();
                timeline.Markers.Clear();
                timeline.Active = false;
                _projectStatus = "Started a new project.";
                ToastManager.Show(_projectStatus, ToastLevel.Info);
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel")) ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
        }

        ImGui.InputText("Project Name", ref _projectName, 128);

        if (ImGui.Button("Save Project"))
        {
            if (Directory.Exists(ProjectDirectory(_projectName)))
            {
                ImGui.OpenPopup("Confirm Overwrite Project");
            }
            else
            {
                _projectStatus = SaveProject(manager, settings, timeline, _projectName);
                ToastManager.Show(_projectStatus, _projectStatus.StartsWith("Failed", StringComparison.Ordinal) ? ToastLevel.Danger : ToastLevel.Success);
            }
        }

        if (ImGui.BeginPopupModal("Confirm Overwrite Project"))
        {
            ImGui.TextColored(Theme.Warning, $"Project '{_projectName}' already exists. Overwrite it?");
            if (ImGui.Button("Overwrite"))
            {
                _projectStatus = SaveProject(manager, settings, timeline, _projectName);
                ToastManager.Show(_projectStatus, ToastLevel.Success);
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel")) ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
        }

        ImGui.SameLine();
        if (ImGui.Button("Load Project"))
        {
            _projectStatus = LoadProject(manager, settings, timeline, _projectName);
            ToastManager.Show(_projectStatus, _projectStatus.StartsWith("Failed", StringComparison.Ordinal) || _projectStatus.Contains("not found") ? ToastLevel.Danger : ToastLevel.Success);
        }

        if (_projectStatus.Length > 0)
        {
            ImGui.TextWrapped(_projectStatus);
        }

        ImGui.Separator();
        bool autoSave = settings.AutoSaveSettings;
        if (ImGui.Checkbox("Auto-Save Settings on Exit", ref autoSave))
        {
            settings.AutoSaveSettings = autoSave;
        }

        if (ImGui.Button("Save Settings Now"))
        {
            settings.Effects.CopyFrom(manager.Effects);
            SettingsService.Save(settings, settingsFilePath);
            ToastManager.Show("Settings saved", ToastLevel.Success);
        }

        ImGui.Separator();
        bool useShaderCache = ShaderManager.UseShaderBinaryCache;
        if (ImGui.Checkbox("Shader Binary Cache (experimental)", ref useShaderCache))
        {
            ShaderManager.UseShaderBinaryCache = useShaderCache;
        }

        ImGui.TextColored(Theme.TextMuted, "Caches compiled shader binaries to speed up repeat loads. GPU driver behavior on first use of this GL feature can vary by machine — off by default, opt in once you've confirmed it behaves well on your hardware.");

        ImGui.Separator();
        ImGui.Text("Performance Quality Tier");
        string[] tierNames = Enum.GetNames<QualityTier>();
        int tierIndex = (int)settings.QualityTier;
        if (ImGui.Combo("Quality Tier", ref tierIndex, tierNames, tierNames.Length))
        {
            settings.QualityTier = (QualityTier)tierIndex;
            SettingsService.Save(settings, settingsFilePath);
            QualityTierDetector.ApplyTierDefaults(manager, settings.QualityTier);
        }

        string detectedGpuLabel = string.IsNullOrEmpty(settings.DetectedGpuName) ? "not yet detected" : settings.DetectedGpuName;
        ImGui.TextColored(Theme.TextMuted, $"Detected automatically on first run from your GPU ({detectedGpuLabel}) and a short benchmark. Override here if detection got it wrong for your hardware.");

        if (ImGui.Button("Re-detect on Next Launch"))
        {
            settings.QualityTier = QualityTier.Unknown;
            SettingsService.Save(settings, settingsFilePath);
            ToastManager.Show("Quality tier will be re-detected next time you launch ShaderDemo", ToastLevel.Info);
        }

        ImGui.Separator();
        bool vsync = settings.VSync;
        if (ImGui.Checkbox("VSync", ref vsync))
        {
            settings.VSync = vsync;
            SettingsService.Save(settings, settingsFilePath);
            window.SetVSync(vsync);
        }

        ImGui.TextColored(Theme.TextMuted, vsync
            ? "Frame rate is capped to your display's refresh rate."
            : "Frame rate is uncapped by VSync, but still capped to your display's refresh rate to avoid needlessly burning GPU/battery beyond what you can see.");

        if (window.IsThrottled)
        {
            ImGui.TextColored(Theme.Warning, "Rendering throttled: window is unfocused or minimized.");
        }
    }

    private static string ProjectDirectory(string name) => Path.Combine("projects", name);

    private static string SaveProject(ShaderManager manager, AppSettings settings, TimelineEngine timeline, string name)
    {
        try
        {
            string dir = ProjectDirectory(name);
            Directory.CreateDirectory(dir);

            settings.Effects.CopyFrom(manager.Effects);
            SettingsService.Save(settings, Path.Combine(dir, "settings.json"));
            LayerPersistence.Save(manager.Layers, Path.Combine(dir, "layers.json"));
            TimelinePersistence.Save(timeline, Path.Combine(dir, "timeline.json"));

            return $"Saved project '{name}' to {dir}";
        }
        catch (Exception ex)
        {
            return $"Failed to save project: {ex.Message}";
        }
    }

    private static string LoadProject(ShaderManager manager, AppSettings settings, TimelineEngine timeline, string name)
    {
        try
        {
            string dir = ProjectDirectory(name);
            if (!Directory.Exists(dir))
            {
                return $"Project folder not found: {dir}";
            }

            AppSettings loaded = SettingsService.Load(Path.Combine(dir, "settings.json"));
            settings.MusicFile = loaded.MusicFile;
            settings.MusicVolume = loaded.MusicVolume;
            settings.AudioReactive = loaded.AudioReactive;
            settings.AutoSaveSettings = loaded.AutoSaveSettings;
            settings.Effects.CopyFrom(loaded.Effects);
            manager.Effects.CopyFrom(loaded.Effects);
            manager.Audio.Enabled = settings.AudioReactive;

            manager.Layers.Clear();
            manager.Layers.AddRange(LayerPersistence.Load(Path.Combine(dir, "layers.json")));

            TimelinePersistence.Load(timeline, Path.Combine(dir, "timeline.json"));

            return $"Loaded project '{name}' from {dir}";
        }
        catch (Exception ex)
        {
            return $"Failed to load project: {ex.Message}";
        }
    }
}
