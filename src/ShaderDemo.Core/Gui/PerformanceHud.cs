// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using ImGuiNET;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;

namespace ShaderDemo.Core.Gui;

public static class PerformanceHud
{
    public static bool Visible { get; private set; }

    public static void Toggle(ShaderManager manager)
    {
        Visible = !Visible;
        manager.Profiler.Enabled = Visible;
    }

    public static void Draw(ShaderManager manager, AppSettings settings)
    {
        if (!Visible) return;

        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(new Vector2(viewport.WorkPos.X + Theme.SpaceMd, viewport.WorkPos.Y + Theme.SpaceMd), ImGuiCond.Always);
        ImGui.SetNextWindowBgAlpha(0.85f);

        ImGuiWindowFlags flags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings
            | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.AlwaysAutoResize;

        if (ImGui.Begin("##performance-hud", flags))
        {
            Elevation.DrawShadow(ImGui.GetWindowPos(), ImGui.GetWindowSize());

            float fps = ImGui.GetIO().Framerate;
            float frameTimeMs = fps > 0 ? 1000.0f / fps : 0.0f;

            Theme.Heading("Performance (F1 to hide)", Theme.Info);

            string gpuLabel = string.IsNullOrEmpty(settings.DetectedGpuName) ? "(not yet detected)" : settings.DetectedGpuName;
            ImGui.TextColored(Theme.TextMuted, $"Detected: {settings.QualityTier} ({gpuLabel})");

            bool pushedMono = Theme.PushFontIf(Theme.FontMono);
            ImGui.Text($"FPS: {fps:F1}   Frame: {frameTimeMs:F2} ms");

            double totalGpuMs = 0.0;
            foreach ((string name, double ms) in manager.Profiler.GetTimingsMilliseconds())
            {
                DrawMiniBar(name, ms, PerformanceBudget.FrameTimeBudgetMs);
                totalGpuMs += ms;
            }

            ImGui.Text($"GPU total: {totalGpuMs:F3} ms");
            ImGui.Text($"Draw calls: {GpuResourceTracker.DrawCallsLastFrame}");
            ImGui.Text($"VRAM (estimate): {GpuResourceTracker.EstimatedVramBytes / (1024.0 * 1024.0):F1} MB");
            Theme.PopFontIf(pushedMono);

            bool overBudget = frameTimeMs > PerformanceBudget.FrameTimeBudgetMs;
            ImGui.TextColored(overBudget ? Theme.Danger : Theme.Success, overBudget
                ? $"Over budget ({PerformanceBudget.FrameTimeBudgetMs:F1} ms target)"
                : $"Within budget ({PerformanceBudget.FrameTimeBudgetMs:F1} ms target)");

            ImGui.Separator();
            bool adaptiveEnabled = manager.AdaptiveResolution.Enabled;
            if (ImGui.Checkbox("Adaptive Resolution", ref adaptiveEnabled))
            {
                manager.AdaptiveResolution.Enabled = adaptiveEnabled;
            }

            if (manager.AdaptiveResolution.CurrentScale < 0.999f)
            {
                ImGui.TextColored(Theme.Warning, $"Internal render scale: {manager.AdaptiveResolution.CurrentScale * 100.0f:F0}%");
            }
        }

        ImGui.End();
    }

    private static void DrawMiniBar(string label, double ms, double maxMs)
    {
        float t = maxMs > 0 ? (float)Math.Clamp(ms / maxMs, 0.0, 1.0) : 0.0f;
        Vector4 color = t > 0.5f ? Theme.Danger : t > 0.25f ? Theme.Warning : Theme.Success;

        const float barWidth = 90.0f;
        const float barHeight = 10.0f;
        Vector2 cursor = ImGui.GetCursorScreenPos();
        ImDrawListPtr dl = ImGui.GetWindowDrawList();

        dl.AddRectFilled(cursor, cursor + new Vector2(barWidth, barHeight), ImGui.GetColorU32(Theme.SurfaceElevated), 2.0f);
        if (t > 0.0f)
        {
            dl.AddRectFilled(cursor, cursor + new Vector2(barWidth * t, barHeight), ImGui.GetColorU32(color), 2.0f);
        }

        ImGui.Dummy(new Vector2(barWidth, barHeight));
        ImGui.SameLine();
        ImGui.Text($"{label}: {ms:F3} ms");
    }
}
