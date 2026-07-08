// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Gui;

public static class ShaderErrorsPanel
{
    public static void Draw()
    {
        if (ShaderErrorLog.Errors.Count == 0)
        {
            ImGui.TextColored(new System.Numerics.Vector4(0.4f, 1.0f, 0.4f, 1.0f), "No shader compile errors.");
            return;
        }

        foreach (var (name, message) in ShaderErrorLog.Errors)
        {
            ImGui.TextColored(new System.Numerics.Vector4(1.0f, 0.3f, 0.3f, 1.0f), name);
            ImGui.TextWrapped(message);
            ImGui.Separator();
        }
    }
}
