// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.Core.Gui;

public static class GeneralPanel
{
    private static readonly (string Panel, string Title, string Hook)[] NextSteps =
    {
        ("Live Coding", "Live Coding", "Edit shader code directly and hot-compile it, with reusable snippets."),
        ("Timeline", "Timeline", "Sequence shaders, text, and effects to change over time instead of staying static."),
        ("3D Model", "3D Model", "Overlay an OBJ model on top of your shader background."),
    };

    private static readonly HashSet<string> DismissedNextSteps = new();

    public static void Draw(ShaderManager manager, AppSettings settings, TimelineEngine timeline)
    {
        DrawNextSteps();
        ImGui.Separator();
        bool timelineActive = timeline.Active;
        if (ImGui.Checkbox("Timeline Active", ref timelineActive)) timeline.Active = timelineActive;

        bool audioReactive = settings.AudioReactive;
        if (ImGui.Checkbox("Audio Reactivity", ref audioReactive))
        {
            settings.AudioReactive = audioReactive;
            manager.Audio.Enabled = audioReactive;
        }

        float volume = settings.MusicVolume;
        if (ImGui.SliderFloat("Volume", ref volume, 0.0f, 1.0f))
        {
            settings.MusicVolume = volume;
            manager.Player.SetVolume(volume);
        }

        if (settings.AudioReactive && (manager.Audio.BassEnvelope != null || manager.Audio.IsLiveInputActive))
        {
            manager.Audio.TryGetBassValue(manager.ElapsedTime, out float bassVal);
            manager.Audio.TryGetTrebleValue(manager.ElapsedTime, out float trebleVal);
            Theme.Heading("Audio Levels");

            bool pushedMono = Theme.PushFontIf(Theme.FontMono);
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, Theme.AudioBass);
            ImGui.ProgressBar(bassVal, new System.Numerics.Vector2(-1.0f, 0.0f), $"Bass   {bassVal:F2}");
            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, Theme.AudioTreble);
            ImGui.ProgressBar(trebleVal, new System.Numerics.Vector2(-1.0f, 0.0f), $"Treble {trebleVal:F2}");
            ImGui.PopStyleColor();
            Theme.PopFontIf(pushedMono);
        }
    }

    private static void DrawNextSteps()
    {
        var visible = NextSteps.Where(s => !DismissedNextSteps.Contains(s.Panel)).ToArray();
        if (visible.Length == 0) return;

        ImGui.TextColored(Theme.TextMuted, "Next steps you might not have found yet:");
        foreach (var (panel, title, hook) in visible)
        {
            ImGui.PushID(panel);
            Elevation.BeginCard();
            Theme.Heading(title, Theme.Accent);
            ImGui.TextWrapped(hook);
            if (ImGui.SmallButton("Try it"))
            {
                EffectsPanel.RequestOpenPanel(panel);
                DismissedNextSteps.Add(panel);
            }

            ImGui.SameLine();
            if (ImGui.SmallButton("Dismiss"))
            {
                DismissedNextSteps.Add(panel);
            }

            Elevation.EndCard();
            ImGui.PopID();
        }
    }
}
