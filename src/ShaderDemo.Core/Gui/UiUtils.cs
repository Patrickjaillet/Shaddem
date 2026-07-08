// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;

namespace ShaderDemo.Core.Gui;

public static class UiUtils
{
    public static bool SliderWithReset(string label, ref float val, float min, float max, float defaultValue)
    {
        bool changed = ImGui.SliderFloat(label, ref val, min, max);
        ImGui.SameLine();
        if (ImGui.Button($"R##{label}"))
        {
            val = defaultValue;
            changed = true;
        }

        return changed;
    }
}
