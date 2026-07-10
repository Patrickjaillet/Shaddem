// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;

namespace ShaderDemo.Core.Gui;

public static class TemplatesPanel
{
    private static readonly Random Random = new();

    public static void Draw(ShaderManager manager, AppSettings settings)
    {
        if (ImGui.Button("Randomize Demo"))
        {
            string theme = TemplatesService.Randomize(manager, settings, Random);
            ToastManager.Show($"Randomized demo ({theme})", ToastLevel.Success);
        }

        ImGui.SameLine();
        ImGui.TextColored(Theme.TextMuted, "Random shader + themed preset, in one click.");

        ImGui.Separator();
        ImGui.TextColored(Theme.TextMuted, "Curated combos of shader + effects. Pick one as a starting point, then tweak from there.");
        ImGui.Spacing();

        DrawGrid(manager, settings);
    }

    public static void DrawGrid(ShaderManager manager, AppSettings settings)
    {
        foreach (DemoTemplate template in TemplatesService.BuiltIn)
        {
            ImGui.PushID(template.Name);
            Elevation.BeginCard();
            Theme.Heading(template.Name, Theme.Accent);
            ImGui.TextWrapped(template.Description);

            if (ImGui.Button("Use This Template"))
            {
                bool applied = TemplatesService.Apply(template, manager);
                ToastManager.Show(
                    applied ? $"Template applied: {template.Name}" : $"Template shader not found: {template.ShaderName}",
                    applied ? ToastLevel.Success : ToastLevel.Danger);
            }

            Elevation.EndCard();
            ImGui.PopID();
        }
    }
}
