// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using System.Numerics;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;

namespace ShaderDemo.Core.Gui;

public static class WelcomePanel
{
    public static bool IsOpen { get; private set; }
    private static readonly FadeTracker _fade = new();

    public static void Show() => IsOpen = true;

    public static void Draw(ShaderManager manager, AppSettings settings, string settingsFilePath)
    {
        if (settings.FirstRun && !IsOpen)
        {
            IsOpen = true;
        }

        if (!IsOpen) return;

        ImGui.SetNextWindowSize(new Vector2(480, 360), ImGuiCond.Appearing);
        ImGui.SetNextWindowPos(ImGui.GetMainViewport().GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        bool open = true;
        if (ImGui.Begin("Welcome to ShaderDemo", ref open))
        {
            Elevation.DrawShadow(ImGui.GetWindowPos(), ImGui.GetWindowSize());

            float alpha = _fade.Value("shown");
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, alpha);

            bool pushedTitle = Theme.PushFontIf(Theme.FontTitle ?? Theme.FontSemibold);
            ImGui.TextColored(Theme.Accent, "Welcome to ShaderDemo");
            Theme.PopFontIf(pushedTitle);
            ImGui.TextWrapped("Real-time shader visuals with audio reactivity, layers, timeline, and video export. Pick how you'd like to start:");
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button("Start from a Template", new Vector2(-1, 40)))
            {
                Dismiss(settings, settingsFilePath);
                EffectsPanel.RequestOpenPanel("Templates");
                GuidedTour.Start();
            }

            ImGui.TextColored(Theme.TextMuted, "Browse curated shader + effect combos and start editing.");
            ImGui.Spacing();

            if (ImGui.Button("Start Blank", new Vector2(-1, 40)))
            {
                Dismiss(settings, settingsFilePath);
                GuidedTour.Start();
            }

            ImGui.TextColored(Theme.TextMuted, "Jump straight into the default shader with a clean slate.");
            ImGui.Spacing();

            if (ImGui.Button("Import a Project", new Vector2(-1, 40)))
            {
                Dismiss(settings, settingsFilePath);
                EffectsPanel.RequestOpenPanel("System");
            }

            ImGui.TextColored(Theme.TextMuted, "Load a previously saved project by name.");

            ImGui.PopStyleVar();
        }

        ImGui.End();

        if (!open)
        {
            Dismiss(settings, settingsFilePath);
        }
    }

    private static void Dismiss(AppSettings settings, string settingsFilePath)
    {
        IsOpen = false;
        settings.FirstRun = false;
        SettingsService.Save(settings, settingsFilePath);
    }
}
