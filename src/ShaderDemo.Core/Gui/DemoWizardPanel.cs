// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using System.Numerics;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;

namespace ShaderDemo.Core.Gui;

public static class DemoWizardPanel
{
    public static bool IsOpen { get; private set; }
    private static int _step;
    private static bool _recordAsGif;
    private static float _durationSeconds = 15.0f;
    private static readonly FadeTracker _stepFade = new();

    public static void Open()
    {
        IsOpen = true;
        _step = 0;
    }

    public static void Draw(ShaderManager manager, AppSettings settings)
    {
        if (!IsOpen) return;

        ImGui.SetNextWindowSize(new Vector2(480, 420), ImGuiCond.Appearing);
        bool open = true;
        if (ImGui.Begin("Demo Wizard", ref open, ImGuiWindowFlags.NoCollapse))
        {
            Elevation.DrawShadow(ImGui.GetWindowPos(), ImGui.GetWindowSize());

            ImGui.TextColored(Theme.TextMuted, $"Step {_step + 1} of 5");
            ImGui.Separator();
            ImGui.Spacing();

            float alpha = _stepFade.Value(_step);
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, alpha);
            switch (_step)
            {
                case 0: DrawPickLook(manager, settings); break;
                case 1: DrawAddMusic(manager, settings); break;
                case 2: DrawIntensity(manager); break;
                case 3: DrawExportSettings(manager); break;
                case 4: DrawExport(manager, settings); break;
            }

            ImGui.PopStyleVar();
            ImGui.Spacing();
            ImGui.Separator();

            if (_step > 0 && ImGui.Button("Back"))
            {
                _step--;
            }

            ImGui.SameLine();
            if (_step < 4 && ImGui.Button("Next"))
            {
                _step++;
            }

            ImGui.SameLine();
            if (ImGui.Button("Close"))
            {
                IsOpen = false;
            }
        }

        ImGui.End();
        if (!open) IsOpen = false;
    }

    private static void DrawPickLook(ShaderManager manager, AppSettings settings)
    {
        Theme.Heading("Step 1 — Pick a Look", Theme.Accent);
        ImGui.TextWrapped("Choose a template, or pick any shader from the list below.");
        ImGui.Spacing();

        TemplatesPanel.DrawGrid(manager, settings);

        ImGui.Spacing();
        ImGui.Text($"Current shader: {manager.CurrentShaderName ?? "<none>"}");
        if (manager.ShaderNames.Count > 0)
        {
            int index = manager.CurrentShaderIndex;
            string[] names = new string[manager.ShaderNames.Count];
            for (int i = 0; i < names.Length; i++) names[i] = manager.ShaderNames[i];
            if (ImGui.Combo("Or pick any shader", ref index, names, names.Length))
            {
                manager.SelectShader(index);
            }
        }
    }

    private static void DrawAddMusic(ShaderManager manager, AppSettings settings)
    {
        Theme.Heading("Step 2 — Add Music (optional)", Theme.Accent);
        ImGui.TextWrapped("Load a track to drive audio-reactive effects, or drop an audio file anywhere on the window.");
        ImGui.Spacing();

        string musicFile = settings.MusicFile;
        if (ImGui.InputText("File", ref musicFile, 256)) settings.MusicFile = musicFile;

        ImGui.SameLine();
        if (ImGui.Button("Browse..."))
        {
            string filter = NativeFileDialog.BuildFilter(("Audio Files", new[] { ".wav", ".mp3", ".ogg", ".flac", ".aac", ".wma", ".m4a" }));
            string? picked = NativeFileDialog.OpenFile("Select an audio file", filter, Path.GetDirectoryName(settings.MusicFile));
            if (picked != null) settings.MusicFile = picked;
        }

        if (ImGui.Button("Load & Play"))
        {
            if (File.Exists(settings.MusicFile) && ShaderDemo.Core.Audio.AudioAnalyzer.IsSupportedFile(settings.MusicFile))
            {
                manager.Audio.Load(settings.MusicFile, manager.ElapsedTime);
                settings.AudioReactive = true;
                manager.Audio.Enabled = true;
                manager.Player.Play(settings.MusicFile, settings.MusicVolume);
                var fullAnalysis = ShaderDemo.Core.Audio.AudioAnalyzer.AnalyzeFull(settings.MusicFile);
                manager.AudioViz.SetAnalysis(fullAnalysis, manager.ElapsedTime);
                ToastManager.Show($"Playing: {Path.GetFileName(settings.MusicFile)}", ToastLevel.Success);
            }
            else
            {
                ToastManager.Show("Could not load audio file (not found or unsupported format)", ToastLevel.Danger);
            }
        }

        ImGui.TextColored(Theme.TextMuted, "No music? Skip this step, the demo will just run without audio reactivity.");
    }

    private static void DrawIntensity(ShaderManager manager)
    {
        Theme.Heading("Step 3 — Adjust Intensity", Theme.Accent);
        ImGui.TextWrapped("One slider that scales bloom, glitch, chromatic aberration, and vignette together.");
        ImGui.Spacing();
        MacroControlsPanel.DrawIntensityOnly(manager);
    }

    private static void DrawExportSettings(ShaderManager manager)
    {
        Theme.Heading("Step 4 — Export Length & Format", Theme.Accent);
        ImGui.Text($"Resolution: {manager.Pipeline.Width}x{manager.Pipeline.Height} (matches the current window)");
        ImGui.SliderFloat("Duration (s)", ref _durationSeconds, 1.0f, 300.0f);
        ImGui.Checkbox("Export as GIF (unchecked = MP4)", ref _recordAsGif);
    }

    private static void DrawExport(ShaderManager manager, AppSettings settings)
    {
        Theme.Heading("Step 5 — Export", Theme.Accent);

        if (!manager.Recorder.IsRecording)
        {
            ImGui.TextWrapped("Ready. This uses the same recorder as the Export panel's \"Start Recording\", with the settings from the previous step.");
            if (Icons.IconLabelButton("WizardStartRecording", Icon.Record, "Start Recording", new Vector2(ImGui.GetContentRegionAvail().X, 40)))
            {
                Directory.CreateDirectory("videos");
                string extension = _recordAsGif ? "gif" : "mp4";
                string filename = Path.Combine("videos", $"demo_{DateTime.Now:yyyyMMdd-HHmmss}.{extension}");
                manager.Recorder.Start(manager.Pipeline.Width, manager.Pipeline.Height, 60, _durationSeconds, filename, settings.MusicFile, 0.0, includeAudio: true, _recordAsGif, ShaderDemo.Core.Logging.AppLog.Info);
                ToastManager.Show("Recording started", ToastLevel.Info);
            }
        }
        else
        {
            ImGui.TextColored(Theme.Danger, $"{Theme.Icons.Record} Recording: {manager.Recorder.OutputFile}");
            float fraction = Math.Clamp((float)(manager.Recorder.EncodedSeconds / _durationSeconds), 0.0f, 1.0f);
            ImGui.ProgressBar(fraction, new Vector2(-1.0f, 0.0f), $"{manager.Recorder.EncodedSeconds:F1}s / {_durationSeconds:F1}s");

            if (Icons.IconLabelButton("WizardStopRecording", Icon.Stop, "Stop Recording"))
            {
                string finishedFile = manager.Recorder.OutputFile ?? "recording";
                manager.Recorder.Stop();
                ToastManager.Show($"Export finished: {finishedFile}", ToastLevel.Success);
            }
        }
    }
}
