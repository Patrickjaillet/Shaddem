// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using System.Numerics;
using ShaderDemo.Core.Logging;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.Core.Gui;

public static class QuickExportPanel
{
    private static bool _isOpen;

    public static void Open() => _isOpen = true;

    public static void Draw(ShaderManager manager, AppSettings settings, TimelineEngine timeline)
    {
        if (!_isOpen) return;

        ImGui.SetNextWindowSize(new Vector2(420, 0), ImGuiCond.Appearing);
        bool open = true;
        if (ImGui.Begin("Quick Export", ref open, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize))
        {
            Elevation.DrawShadow(ImGui.GetWindowPos(), ImGui.GetWindowSize());

            if (!manager.Recorder.IsRecording)
            {
                bool hasAudio = File.Exists(settings.MusicFile);
                float defaultDuration = hasAudio ? 30.0f : 15.0f;

                ImGui.Text($"Resolution: {manager.Pipeline.Width}x{manager.Pipeline.Height} (current window)");
                ImGui.Text($"Duration: {defaultDuration:F0}s{(hasAudio ? " (audio loaded)" : " (no audio loaded)")}");
                ImGui.Text("Format: MP4");
                ImGui.Spacing();

                if (Icons.IconLabelButton("ExportNow", Icon.Record, "Export Now", new Vector2(ImGui.GetContentRegionAvail().X, 36)))
                {
                    Directory.CreateDirectory("videos");
                    string filename = Path.Combine("videos", $"demo_{DateTime.Now:yyyyMMdd-HHmmss}.mp4");
                    manager.Recorder.Start(manager.Pipeline.Width, manager.Pipeline.Height, 60, defaultDuration, filename, settings.MusicFile, 0.0, includeAudio: hasAudio, isGif: false, AppLog.Info);
                    ToastManager.Show("Recording started", ToastLevel.Info);
                }
            }
            else
            {
                ImGui.TextColored(Theme.Danger, $"{Theme.Icons.Record} Recording: {manager.Recorder.OutputFile}");
                ImGui.Text($"Encoded: {manager.Recorder.EncodedSeconds:F1}s ({manager.Recorder.EncodedFrames} frames)");

                if (Icons.IconLabelButton("StopRecording", Icon.Stop, "Stop Recording", new Vector2(ImGui.GetContentRegionAvail().X, 36)))
                {
                    string finishedFile = manager.Recorder.OutputFile ?? "recording";
                    manager.Recorder.Stop();
                    ToastManager.Show($"Export finished: {finishedFile}", ToastLevel.Success);
                }
            }

            ImGui.Spacing();
            ImGui.Separator();

            if (ImGui.CollapsingHeader("Advanced Options (full Export panel)"))
            {
                ExportPanel.Draw(manager, settings, timeline);
            }
        }

        ImGui.End();
        if (!open) _isOpen = false;
    }
}
