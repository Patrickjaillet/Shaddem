// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Gui;

public static class DebugOverlayPanel
{
    public static void Draw(ShaderManager manager)
    {
        bool enabled = manager.Profiler.Enabled;
        if (ImGui.Checkbox("Enable GPU Timer Queries", ref enabled))
        {
            manager.Profiler.Enabled = enabled;
        }

        if (!enabled)
        {
            ImGui.TextColored(Theme.TextMuted, "Enable to measure GPU time per render pass (glBeginQuery/GL_TIME_ELAPSED, double-buffered).");
            return;
        }

        bool pushedMono = Theme.PushFontIf(Theme.FontMono);
        ImGui.Text($"CPU FPS: {ImGui.GetIO().Framerate:F1}");

        double total = 0.0;
        foreach ((string name, double ms) in manager.Profiler.GetTimingsMilliseconds())
        {
            ImGui.Text($"{name}: {ms:F3} ms");
            total += ms;
        }

        ImGui.Separator();
        ImGui.Text($"Total (sum of passes): {total:F3} ms");
        ImGui.Text($"Particle count: {manager.Particles.Count}");
        Theme.PopFontIf(pushedMono);
    }
}
