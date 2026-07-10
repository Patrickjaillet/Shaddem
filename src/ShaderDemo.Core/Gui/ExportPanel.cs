// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Export;
using ShaderDemo.Core.Logging;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.Core.Gui;

public static class ExportPanel
{
    private static bool _useFixedDuration;
    private static float _durationSeconds = 10.0f;
    private static bool _recordAsGif;
    private static bool _includeAudio = true;
    private static bool _useHardwareEncoding;
    private static string? _detectedHardwareEncoder;
    private static bool _hardwareDetectionAttempted;
    private static string _exportStatus = "";

    public static void Draw(ShaderManager manager, AppSettings settings, TimelineEngine timeline)
    {
        if (Icons.IconLabelButton("Screenshot", Icon.Screenshot, "Screenshot"))
        {
            string path = ScreenshotService.Save(manager.LastComposedFrame ?? manager.Pipeline.SceneFbo, "screenshots");
            AppLog.Info($"Screenshot: {path}");
        }

        ImGui.Separator();
        ImGui.Text("Recording");

        ImGui.Checkbox("Use Fixed Duration", ref _useFixedDuration);
        if (_useFixedDuration)
        {
            ImGui.SliderFloat("Duration (s)", ref _durationSeconds, 1.0f, 300.0f);
        }

        ImGui.Checkbox("Record as GIF", ref _recordAsGif);
        ImGui.Checkbox("Include Audio", ref _includeAudio);

        if (ImGui.Checkbox("Use Hardware Encoding (auto-detect)", ref _useHardwareEncoding))
        {
            if (_useHardwareEncoding && !_hardwareDetectionAttempted)
            {
                _detectedHardwareEncoder = FfmpegCapabilities.DetectHardwareEncoder(manager.Recorder.FfmpegPath, AppLog.Info);
                _hardwareDetectionAttempted = true;
            }
        }

        if (_useHardwareEncoding)
        {
            ImGui.TextColored(
                Theme.Info,
                _detectedHardwareEncoder != null ? $"Detected: {_detectedHardwareEncoder}" : "No hardware encoder found, falling back to libx264");
        }

        if (!manager.Recorder.IsRecording)
        {
            if (Icons.IconLabelButton("StartRecording", Icon.Record, "Start Recording"))
            {
                Directory.CreateDirectory("videos");
                string extension = _recordAsGif ? "gif" : "mp4";
                string filename = Path.Combine("videos", $"recording_{DateTime.Now:yyyyMMdd-HHmmss}.{extension}");
                double? duration = _useFixedDuration ? _durationSeconds : null;

                manager.Recorder.HardwareEncoder = _useHardwareEncoding ? _detectedHardwareEncoder : null;
                manager.Recorder.Start(
                    manager.Pipeline.Width,
                    manager.Pipeline.Height,
                    60,
                    duration,
                    filename,
                    settings.MusicFile,
                    0.0,
                    _includeAudio,
                    _recordAsGif,
                    AppLog.Info);
            }
        }
        else
        {
            ImGui.TextColored(Theme.Danger, $"{Theme.Icons.Record} Recording: {manager.Recorder.OutputFile}");

            if (_useFixedDuration && _durationSeconds > 0)
            {
                float fraction = Math.Clamp((float)(manager.Recorder.EncodedSeconds / _durationSeconds), 0.0f, 1.0f);
                ImGui.ProgressBar(fraction, new System.Numerics.Vector2(-1.0f, 0.0f), $"{manager.Recorder.EncodedSeconds:F1}s / {_durationSeconds:F1}s");
            }
            else
            {
                Theme.Mono($"Encoded: {manager.Recorder.EncodedSeconds:F1}s ({manager.Recorder.EncodedFrames} frames)");
            }

            if (Icons.IconLabelButton("StopRecording", Icon.Stop, "Stop Recording"))
            {
                string finishedFile = manager.Recorder.OutputFile ?? "recording";
                manager.Recorder.Stop();
                ToastManager.Show($"Export finished: {finishedFile}", ToastLevel.Success);
            }
        }

        ImGui.Separator();
        ImGui.Text("Standalone Exports");

        if (ImGui.Button("Export Audio Only (.wav)"))
        {
            if (File.Exists(settings.MusicFile))
            {
                Directory.CreateDirectory("audio_export");
                string outputPath = Path.Combine("audio_export", $"audio_{DateTime.Now:yyyyMMdd-HHmmss}.wav");
                double? duration = _useFixedDuration ? _durationSeconds : null;
                AudioExporter.ExportAudioOnly(manager.Recorder.FfmpegPath, settings.MusicFile, outputPath, duration, msg => _exportStatus = msg);
            }
            else
            {
                _exportStatus = $"No music file loaded ({settings.MusicFile})";
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Export Subtitles (.srt)"))
        {
            Directory.CreateDirectory("subtitles");
            string outputPath = Path.Combine("subtitles", $"timeline_{DateTime.Now:yyyyMMdd-HHmmss}.srt");
            int count = SrtExporter.Export(timeline, outputPath);
            _exportStatus = count > 0 ? $"Exported {count} subtitle entries to {outputPath}" : "No Text clips on the timeline to export.";
        }

        if (_exportStatus.Length > 0)
        {
            ImGui.TextWrapped(_exportStatus);
        }
    }
}
