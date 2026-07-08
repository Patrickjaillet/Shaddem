// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;

namespace ShaderDemo.Core.Gui;

public static class AudioPanel
{
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
        ImGui.Text("Source Audio");

        string musicFile = settings.MusicFile;
        if (ImGui.InputText("File", ref musicFile, 256))
        {
            settings.MusicFile = musicFile;
        }

        if (ImGui.Button("Load & Play"))
        {
            if (File.Exists(settings.MusicFile))
            {
                manager.Audio.Load(settings.MusicFile, manager.ElapsedTime);
                manager.Audio.Enabled = settings.AudioReactive;
                manager.Player.Play(settings.MusicFile, settings.MusicVolume);

                var fullAnalysis = ShaderDemo.Core.Audio.AudioAnalyzer.AnalyzeFull(settings.MusicFile);
                manager.AudioViz.SetAnalysis(fullAnalysis, manager.ElapsedTime);
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Stop"))
        {
            manager.Player.Stop();
            manager.Audio.Clear();
        }

        ImGui.Separator();

        if (manager.Audio.BassEnvelope != null)
        {
            manager.Audio.TryGetBassValue(manager.ElapsedTime, out float bassVal);
            ImGui.Text("Audio Levels:");
            ImGui.ProgressBar(bassVal, new System.Numerics.Vector2(-1.0f, 0.0f), $"Bass: {bassVal:F2}");
        }
        else
        {
            ImGui.TextColored(new System.Numerics.Vector4(1.0f, 0.5f, 0.0f, 1.0f), "No spectral data available (music not loaded or analysis failed).");
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
