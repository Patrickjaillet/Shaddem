// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Export;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;

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

    public static void Draw(ShaderManager manager, AppSettings settings)
    {
        if (ImGui.Button("Screenshot"))
        {
            string path = ScreenshotService.Save(manager.LastComposedFrame ?? manager.Pipeline.SceneFbo, "screenshots");
            Console.WriteLine($"Screenshot: {path}");
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
                _detectedHardwareEncoder = FfmpegCapabilities.DetectHardwareEncoder(manager.Recorder.FfmpegPath, Console.WriteLine);
                _hardwareDetectionAttempted = true;
            }
        }

        if (_useHardwareEncoding)
        {
            ImGui.TextColored(
                new System.Numerics.Vector4(0.6f, 0.8f, 1.0f, 1.0f),
                _detectedHardwareEncoder != null ? $"Detected: {_detectedHardwareEncoder}" : "No hardware encoder found, falling back to libx264");
        }

        if (!manager.Recorder.IsRecording)
        {
            if (ImGui.Button("Start Recording"))
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
                    Console.WriteLine);
            }
        }
        else
        {
            ImGui.TextColored(new System.Numerics.Vector4(1.0f, 0.2f, 0.2f, 1.0f), $"Recording: {manager.Recorder.OutputFile}");

            if (_useFixedDuration && _durationSeconds > 0)
            {
                float fraction = Math.Clamp((float)(manager.Recorder.EncodedSeconds / _durationSeconds), 0.0f, 1.0f);
                ImGui.ProgressBar(fraction, new System.Numerics.Vector2(-1.0f, 0.0f), $"{manager.Recorder.EncodedSeconds:F1}s / {_durationSeconds:F1}s");
            }
            else
            {
                ImGui.Text($"Encoded: {manager.Recorder.EncodedSeconds:F1}s ({manager.Recorder.EncodedFrames} frames)");
            }

            if (ImGui.Button("Stop Recording"))
            {
                manager.Recorder.Stop();
            }
        }
    }
}
