// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;

namespace ShaderDemo.Core.Gui;

public static class AboutPanel
{
    private static readonly (string Name, string Role)[] TechStack =
    {
        (".NET 8 / C# 12", "Runtime and language"),
        ("Silk.NET", "Windowing, input, and OpenGL bindings"),
        ("ImGui.NET", "Immediate-mode GUI"),
        ("NAudio", "Audio decoding and FFT analysis"),
        ("StbImageSharp / StbImageWriteSharp", "Texture and screenshot I/O"),
        ("Microsoft.Data.Sqlite", "Legacy project migration only"),
        ("ffmpeg (external process)", "Video/GIF encoding"),
    };

    public static void Draw()
    {
        bool pushedTitle = Theme.PushFontIf(Theme.FontTitle);
        ImGui.TextColored(Theme.Accent, AppInfo.Name);
        Theme.PopFontIf(pushedTitle);

        ImGui.TextColored(Theme.TextMuted, $"Version {AppInfo.Version}");
        ImGui.TextWrapped("Real-time GLSL shader visualizer with audio reactivity, layers, a timeline, 3D model overlay, live coding, and MP4/GIF video export.");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        Theme.Heading("Tech Stack");
        foreach ((string name, string role) in TechStack)
        {
            ImGui.BulletText(name);
            ImGui.SameLine();
            ImGui.TextColored(Theme.TextMuted, $"— {role}");
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        Theme.Heading("License");
        ImGui.TextWrapped("Distributed under the MIT License. See LICENSE in the project root for the full text.");

        ImGui.Spacing();
        ImGui.TextColored(Theme.TextMuted, AppInfo.Copyright);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextWrapped("See README.md in the project root for the full feature list, screenshots, and usage guide.");
    }
}
