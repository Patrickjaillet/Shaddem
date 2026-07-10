// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;

namespace ShaderDemo.Core.Gui;

public static class FileValidationHint
{
    public static void Draw(string path, string[]? validExtensions = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            ImGui.TextColored(Theme.TextMuted, "No file selected.");
            return;
        }

        if (!File.Exists(path))
        {
            ImGui.TextColored(Theme.Danger, $"{Theme.Icons.Close} File not found.");
            return;
        }

        if (validExtensions != null)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();
            if (!validExtensions.Contains(extension))
            {
                ImGui.TextColored(Theme.Warning, $"File found, but unsupported format ({extension}). Expected: {string.Join(", ", validExtensions)}");
                return;
            }
        }

        ImGui.TextColored(Theme.Success, $"{Theme.Icons.Bullet} File found.");
    }
}
