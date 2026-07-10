// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Audio;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;

namespace ShaderDemo.Core.Gui;

public static class AudioPanel
{
    private static readonly string[] AudioSupportedExtensions = { ".wav", ".mp3", ".ogg", ".flac", ".aac", ".wma", ".m4a" };
    private static int _selectedDevice;

    public static void Draw(ShaderManager manager, AppSettings settings)
    {
        bool audioReactive = settings.AudioReactive;
        if (ImGui.Checkbox("Audio Reactivity", ref audioReactive))
        {
            settings.AudioReactive = audioReactive;
            manager.Audio.Enabled = audioReactive;
        }

        float volume = settings.MusicVolume;
        if (ImGui.SliderFloat("Volume", ref volume, 0.0f, 1.0f))
        {
            settings.MusicVolume = volume;
            manager.Player.SetVolume(volume);
        }

        ImGui.Separator();
        ImGui.Text("Source Audio (.wav, .mp3, .ogg, .flac, .aac, .wma, .m4a)");

        string musicFile = settings.MusicFile;
        if (ImGui.InputText("File", ref musicFile, 256))
        {
            settings.MusicFile = musicFile;
        }

        ImGui.SameLine();
        if (ImGui.Button("Browse...##Audio"))
        {
            string filter = NativeFileDialog.BuildFilter(("Audio Files", AudioSupportedExtensions));
            string? picked = NativeFileDialog.OpenFile("Select an audio file", filter, Path.GetDirectoryName(settings.MusicFile));
            if (picked != null) settings.MusicFile = picked;
        }

        FileValidationHint.Draw(settings.MusicFile, AudioSupportedExtensions);

        if (ImGui.Button("Load & Play"))
        {
            if (File.Exists(settings.MusicFile) && AudioAnalyzer.IsSupportedFile(settings.MusicFile))
            {
                manager.Audio.Load(settings.MusicFile, manager.ElapsedTime);
                manager.Audio.Enabled = settings.AudioReactive;
                manager.Player.Play(settings.MusicFile, settings.MusicVolume);

                var fullAnalysis = AudioAnalyzer.AnalyzeFull(settings.MusicFile);
                manager.AudioViz.SetAnalysis(fullAnalysis, manager.ElapsedTime);
                ToastManager.Show($"Playing: {Path.GetFileName(settings.MusicFile)}", ToastLevel.Success);
            }
            else
            {
                ToastManager.Show("Could not load audio file (not found or unsupported format)", ToastLevel.Danger);
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Stop"))
        {
            manager.Player.Stop();
            manager.Audio.Clear();
        }

        ImGui.Separator();
        ImGui.Text("Live Input (mic / line-in)");

        var devices = LiveAudioAnalyzer.ListDevices().ToArray();
        string[] deviceNames = devices.Length > 0
            ? devices.Select(d => d.ProductName).ToArray()
            : new[] { "No input device found" };

        _selectedDevice = Math.Clamp(_selectedDevice, 0, deviceNames.Length - 1);
        ImGui.Combo("Input Device", ref _selectedDevice, deviceNames, deviceNames.Length);

        bool liveActive = manager.Audio.IsLiveInputActive;
        if (devices.Length == 0) ImGui.BeginDisabled();
        if (!liveActive)
        {
            if (ImGui.Button("Start Live Input"))
            {
                manager.Player.Stop();
                manager.Audio.StartLiveInput(_selectedDevice);
            }
        }
        else
        {
            if (ImGui.Button("Stop Live Input"))
            {
                manager.Audio.StopLiveInput();
            }
        }

        if (devices.Length == 0) ImGui.EndDisabled();

        ImGui.Separator();

        if (manager.Audio.BassEnvelope != null || manager.Audio.IsLiveInputActive)
        {
            manager.Audio.TryGetBassValue(manager.ElapsedTime, out float bassVal);
            manager.Audio.TryGetMidValue(manager.ElapsedTime, out float midVal);
            manager.Audio.TryGetTrebleValue(manager.ElapsedTime, out float trebleVal);
            Theme.Heading("Audio Levels");

            bool pushedMono = Theme.PushFontIf(Theme.FontMono);
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, Theme.AudioBass);
            ImGui.ProgressBar(bassVal, new System.Numerics.Vector2(-1.0f, 0.0f), $"Bass   {bassVal:F2}");
            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, Theme.AudioMid);
            ImGui.ProgressBar(midVal, new System.Numerics.Vector2(-1.0f, 0.0f), $"Mid    {midVal:F2}");
            ImGui.PopStyleColor();
            ImGui.PushStyleColor(ImGuiCol.PlotHistogram, Theme.AudioTreble);
            ImGui.ProgressBar(trebleVal, new System.Numerics.Vector2(-1.0f, 0.0f), $"Treble {trebleVal:F2}");
            ImGui.PopStyleColor();
            Theme.PopFontIf(pushedMono);
        }
        else
        {
            ImGui.TextColored(Theme.Warning, "No spectral data available (music not loaded or analysis failed).");
        }

        ImGui.Separator();
        ImGui.Text("Overlay (Exportable)");

        ImGui.Checkbox("Show Overlay on Scene", ref manager.AudioViz.Enabled);
        if (manager.AudioViz.Enabled)
        {
            int mode = (int)manager.AudioViz.Mode;
            string[] modes = { "Spectrum (FFT)", "Waveform" };
            if (ImGui.Combo("Visualization Mode", ref mode, modes, modes.Length))
            {
                manager.AudioViz.Mode = (AudioVizMode)mode;
            }

            ImGui.SliderFloat("Height", ref manager.AudioViz.Height, 0.1f, 1.0f);
            ImGui.SliderFloat("Opacity", ref manager.AudioViz.Opacity, 0.0f, 1.0f);
            ImGui.ColorEdit4("Overlay Color", ref manager.AudioViz.Color);
            ImGui.SliderFloat("Trail (Decay)", ref manager.AudioViz.TrailDecay, 0.0f, 0.99f);
        }
    }
}
