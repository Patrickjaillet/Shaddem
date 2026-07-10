// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using ImGuiNET;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Gui;

public static class RecordingIndicator
{
    public static void Draw(ShaderManager manager)
    {
        if (!manager.Recorder.IsRecording) return;

        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        Vector2 pos = new(viewport.WorkPos.X + viewport.WorkSize.X * 0.5f, viewport.WorkPos.Y + Theme.SpaceSm);
        ImGui.SetNextWindowPos(pos, ImGuiCond.Always, new Vector2(0.5f, 0.0f));
        ImGui.SetNextWindowBgAlpha(0.95f);

        ImGuiWindowFlags flags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings
            | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.AlwaysAutoResize;

        float pulse = 0.5f + 0.5f * MathF.Sin((float)ImGui.GetTime() * 4.0f);
        Vector4 dotColor = Vector4.Lerp(Theme.Danger, new Vector4(1.0f, 1.0f, 1.0f, 1.0f), pulse * 0.35f);

        ImGui.PushStyleColor(ImGuiCol.WindowBg, Theme.SurfaceOverlay);
        ImGui.PushStyleColor(ImGuiCol.Border, Theme.Danger);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.5f);

        if (ImGui.Begin("##recording-indicator", flags))
        {
            Elevation.DrawShadow(ImGui.GetWindowPos(), ImGui.GetWindowSize());

            Icons.Inline(Icon.Record, ImGui.GetFontSize(), dotColor);
            ImGui.SameLine();
            Theme.Mono($"REC {manager.Recorder.EncodedSeconds:F1}s", Theme.Danger);
        }

        ImGui.End();
        ImGui.PopStyleVar();
        ImGui.PopStyleColor(2);
    }
}
