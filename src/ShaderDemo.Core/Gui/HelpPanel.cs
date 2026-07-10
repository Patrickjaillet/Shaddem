// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using System.Numerics;

namespace ShaderDemo.Core.Gui;

public static class HelpPanel
{
    private static bool _isOpen;
    private static string _query = "";

    public static void Open() => _isOpen = true;

    public static void Draw()
    {
        if (!_isOpen) return;

        ImGui.SetNextWindowSize(new Vector2(420, 420), ImGuiCond.Appearing);
        bool open = true;
        if (ImGui.Begin("Help", ref open, ImGuiWindowFlags.NoCollapse))
        {
            Elevation.DrawShadow(ImGui.GetWindowPos(), ImGui.GetWindowSize());

            Icons.Inline(Icon.Search, ImGui.GetFontSize(), Theme.TextMuted);
            ImGui.SameLine();
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("##HelpSearch", "Search panels (e.g. \"audio\", \"export\", \"layers\")...", ref _query, 128);
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.BeginChild("HelpResults");
            foreach (var (panelName, description) in PanelDescriptions.All)
            {
                if (_query.Length > 0
                    && !panelName.Contains(_query, StringComparison.OrdinalIgnoreCase)
                    && !description.Contains(_query, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Theme.Heading(panelName, Theme.Accent);
                ImGui.TextWrapped(description);
                ImGui.SameLine();
                if (ImGui.SmallButton($"Open##{panelName}"))
                {
                    EffectsPanel.RequestOpenPanel(panelName);
                    _isOpen = false;
                }

                ImGui.Spacing();
            }

            ImGui.EndChild();
        }

        ImGui.End();
        if (!open) _isOpen = false;
    }
}
