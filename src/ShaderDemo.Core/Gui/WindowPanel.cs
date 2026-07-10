// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Logging;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Gui;

public static class WindowPanel
{
    private static int _displayIndex;

    public static void Draw(SecondaryWindow preview, ShaderManager manager)
    {
        ImGui.Text("Secondary output window (mirrors the composed frame)");

        var displays = preview.ListDisplays();
        string[] displayNames = displays.Count > 0
            ? displays.Select(d => d.Name).ToArray()
            : new[] { "Display 0" };

        _displayIndex = Math.Clamp(_displayIndex, 0, displayNames.Length - 1);
        if (ImGui.Combo("Display", ref _displayIndex, displayNames, displayNames.Length))
        {
            preview.DisplayIndex = _displayIndex;
        }

        bool fullscreen = preview.Fullscreen;
        if (ImGui.Checkbox("Fullscreen Preview", ref fullscreen)) preview.Fullscreen = fullscreen;

        bool topMost = preview.TopMost;
        if (ImGui.Checkbox("Always On Top", ref topMost)) preview.TopMost = topMost;

        bool hideCursor = preview.HideCursor;
        if (ImGui.Checkbox("Hide Cursor", ref hideCursor)) preview.HideCursor = hideCursor;

        ImGui.TextWrapped("Display/fullscreen/always-on-top/hide-cursor take effect the next time the preview window is opened.");

        if (!preview.IsOpen)
        {
            if (ImGui.Button("Open Preview Window"))
            {
                preview.Open("ShaderDemo Preview", manager.Pipeline.Width, manager.Pipeline.Height, AppLog.Info);
            }
        }
        else
        {
            if (ImGui.Button("Close Preview Window"))
            {
                preview.Close();
            }

            ImGui.SameLine();
            if (ImGui.Button("Reset Position (Reopen)"))
            {
                preview.Close();
                preview.Open("ShaderDemo Preview", manager.Pipeline.Width, manager.Pipeline.Height, AppLog.Info);
            }
        }
    }
}
