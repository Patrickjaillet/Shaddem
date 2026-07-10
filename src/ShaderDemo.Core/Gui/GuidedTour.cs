// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using System.Numerics;
using ShaderDemo.Core.Settings;

namespace ShaderDemo.Core.Gui;

public static class GuidedTour
{
    private readonly record struct Step(string Title, string Text, string? PanelToOpen);

    private static readonly Step[] Steps =
    {
        new("1/4 — Shader Picker", "Use Previous/Next or the shader list at the top to switch what's rendering. Every bundled shader is a starting point.", null),
        new("2/4 — Effect Intensity", "In Simple mode, the Intensity and Color Mood sliders at the bottom scale a whole group of effects together. Advanced mode exposes every individual slider.", null),
        new("3/4 — Load Audio", "Open the Audio panel to load a music file (or a live mic input) and drive effects off the beat.", "Audio"),
        new("4/4 — Record", "Open the Export panel to record a video/GIF, or press E anytime to start/stop recording with defaults.", "Export"),
    };

    private static int _stepIndex = -1;
    private static readonly FadeTracker _stepFade = new();

    public static bool IsActive => _stepIndex >= 0 && _stepIndex < Steps.Length;

    public static void Start()
    {
        _stepIndex = 0;
        if (Steps[0].PanelToOpen != null) EffectsPanel.RequestOpenPanel(Steps[0].PanelToOpen!);
    }

    public static void Draw(AppSettings settings, string settingsFilePath)
    {
        if (settings.TourCompleted || !IsActive) return;

        Step step = Steps[_stepIndex];

        ImGui.SetNextWindowSize(new Vector2(340, 0), ImGuiCond.Always);
        ImGui.SetNextWindowPos(new Vector2(ImGui.GetIO().DisplaySize.X - 360, 60), ImGuiCond.Always);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, Theme.SurfaceOverlay);

        if (ImGui.Begin("Guided Tour", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse))
        {
            Elevation.DrawShadow(ImGui.GetWindowPos(), ImGui.GetWindowSize());

            float alpha = _stepFade.Value(_stepIndex);
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, alpha);

            bool pushedTitle = Theme.PushFontIf(Theme.FontSemibold);
            ImGui.TextColored(Theme.Accent, step.Title);
            Theme.PopFontIf(pushedTitle);
            ImGui.TextWrapped(step.Text);
            ImGui.Spacing();
            ImGui.Separator();

            if (ImGui.Button("Skip Tour"))
            {
                Finish(settings, settingsFilePath);
            }

            ImGui.SameLine();
            bool isLast = _stepIndex == Steps.Length - 1;
            if (ImGui.Button(isLast ? "Done" : "Next"))
            {
                if (isLast)
                {
                    Finish(settings, settingsFilePath);
                }
                else
                {
                    _stepIndex++;
                    string? panel = Steps[_stepIndex].PanelToOpen;
                    if (panel != null) EffectsPanel.RequestOpenPanel(panel);
                }
            }

            ImGui.PopStyleVar();
        }

        ImGui.End();
        ImGui.PopStyleColor();
    }

    private static void Finish(AppSettings settings, string settingsFilePath)
    {
        _stepIndex = -1;
        settings.TourCompleted = true;
        SettingsService.Save(settings, settingsFilePath);
    }
}
