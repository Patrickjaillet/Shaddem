// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Gui;

public static class ShaderErrorsPanel
{
    public static void Draw(ShaderManager manager)
    {
        if (ShaderErrorLog.Errors.Count == 0)
        {
            ImGui.TextColored(Theme.Success, $"{Theme.Icons.Bullet} No shader compile errors.");
            return;
        }

        foreach ((string name, string message) in ShaderErrorLog.Errors.ToList())
        {
            ImGui.PushID(name);
            ImGui.TextColored(Theme.Danger, $"{Theme.Icons.Close} {name}");
            ImGui.TextWrapped(SummarizeReason(message));

            if (ShaderErrorLog.TryGetLastGoodSource(name, out string lastGoodSource))
            {
                if (ImGui.Button("Revert to Last Working Version"))
                {
                    manager.RegisterShader(name, lastGoodSource);
                    ShaderErrorLog.ClearError(name);
                    ToastManager.Show($"Reverted {name} to last working version", ToastLevel.Success);
                }
            }
            else
            {
                ImGui.TextColored(Theme.TextMuted, "No previously working version to revert to.");
            }

            ImGui.Separator();
            ImGui.PopID();
        }
    }

    private static string SummarizeReason(string message)
    {
        int newlineIndex = message.IndexOf('\n');
        string firstLine = newlineIndex >= 0 ? message[..newlineIndex] : message;
        return firstLine.Length > 160 ? firstLine[..160] + "..." : firstLine;
    }
}
