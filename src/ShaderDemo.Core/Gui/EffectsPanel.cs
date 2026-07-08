// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.Core.Gui;

public static class EffectsPanel
{
    public static void Draw(ShaderManager manager, AppSettings settings, string settingsFilePath, string layersFilePath, TimelineEngine timeline, string timelineFilePath, string presetsFilePath, SecondaryWindow preview)
    {
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(420, 640), ImGuiCond.FirstUseEver);
        ImGui.Begin("ShaderDemo");

        DrawSettingsBar(manager, settings, settingsFilePath);
        ImGui.Separator();
        DrawShaderSelector(manager);
        ImGui.Separator();

        if (ImGui.CollapsingHeader("Layers"))
        {
            LayersPanel.Draw(manager, layersFilePath);
        }

        if (ImGui.CollapsingHeader("Timeline"))
        {
            TimelinePanel.Draw(timeline, manager, timelineFilePath);
        }

        if (ImGui.CollapsingHeader("Audio"))
        {
            AudioPanel.Draw(manager, settings);
        }

        if (ImGui.CollapsingHeader("Media"))
        {
            MediaPanel.Draw(manager);
        }

        if (ImGui.CollapsingHeader("3D Model"))
        {
            Model3DPanel.Draw(manager);
        }

        if (ImGui.CollapsingHeader("Presets"))
        {
            PresetsPanel.Draw(manager, presetsFilePath);
        }

        if (ImGui.CollapsingHeader("Live Coding"))
        {
            LiveCodingPanel.Draw(manager);
        }

        if (ImGui.CollapsingHeader("Export"))
        {
            ExportPanel.Draw(manager, settings);
        }

        if (ImGui.CollapsingHeader("Migration"))
        {
            MigrationPanel.Draw(manager, settings, timeline, settingsFilePath, layersFilePath, timelineFilePath);
        }

        if (ImGui.CollapsingHeader("Window"))
        {
            WindowPanel.Draw(preview, manager);
        }

        if (ImGui.CollapsingHeader($"Shader Errors ({ShaderErrorLog.Errors.Count})"))
        {
            ShaderErrorsPanel.Draw();
        }

        ImGui.Separator();
        DrawEffectSliders(manager);

        ImGui.End();
    }

    private static void DrawSettingsBar(ShaderManager manager, AppSettings settings, string settingsFilePath)
    {
        if (ImGui.Button("Save Settings"))
        {
            settings.Effects.CopyFrom(manager.Effects);
            SettingsService.Save(settings, settingsFilePath);
        }

        ImGui.SameLine();
        if (ImGui.Button("Load Settings"))
        {
            AppSettings loaded = SettingsService.Load(settingsFilePath);
            settings.Effects.CopyFrom(loaded.Effects);
            settings.MusicFile = loaded.MusicFile;
            settings.MusicVolume = loaded.MusicVolume;
            settings.AudioReactive = loaded.AudioReactive;
            settings.AutoSaveSettings = loaded.AutoSaveSettings;
            manager.Effects.CopyFrom(settings.Effects);
        }

        ImGui.SameLine();
        bool autoSave = settings.AutoSaveSettings;
        if (ImGui.Checkbox("Auto-Save on Exit", ref autoSave))
        {
            settings.AutoSaveSettings = autoSave;
        }
    }

    private static int _transitionType;
    private static float _transitionDuration = 1.5f;
    private static readonly string[] TransitionTypeNames = { "Wipe", "Fade", "Zoom", "Pixelize" };

    private static void DrawShaderSelector(ShaderManager manager)
    {
        ImGui.Text($"Current shader: {manager.CurrentShaderName ?? "<none>"}");
        if (manager.IsTransitioning) ImGui.TextColored(new System.Numerics.Vector4(0.4f, 0.8f, 1.0f, 1.0f), "Transitioning...");

        if (ImGui.Button("Previous")) manager.PreviousShader();
        ImGui.SameLine();
        if (ImGui.Button("Next")) manager.NextShader();

        if (manager.ShaderNames.Count > 0)
        {
            int index = manager.CurrentShaderIndex;
            string[] names = new string[manager.ShaderNames.Count];
            for (int i = 0; i < names.Length; i++) names[i] = manager.ShaderNames[i];

            if (ImGui.Combo("Shader list", ref index, names, names.Length))
            {
                manager.SelectShader(index);
            }

            ImGui.Combo("Transition Type", ref _transitionType, TransitionTypeNames, TransitionTypeNames.Length);
            ImGui.SliderFloat("Transition Duration (s)", ref _transitionDuration, 0.1f, 5.0f);

            if (ImGui.Button("Transition to Next"))
            {
                manager.StartTransition(manager.CurrentShaderIndex + 1, _transitionDuration, _transitionType);
            }

            ImGui.SameLine();
            if (ImGui.Button("Transition to Selected"))
            {
                manager.StartTransition(index, _transitionDuration, _transitionType);
            }
        }
    }

    private static void DrawEffectSliders(ShaderManager manager)
    {
        EffectParams e = manager.Effects;

        ImGui.Text("Color & Light");
        UiUtils.SliderWithReset("Intensity", ref e.Intensity, 0.0f, 5.0f, 1.0f);
        UiUtils.SliderWithReset("Brightness", ref e.Brightness, 0.0f, 3.0f, 1.0f);
        UiUtils.SliderWithReset("Contrast", ref e.Contrast, 0.0f, 3.0f, 1.0f);
        UiUtils.SliderWithReset("Saturation", ref e.Saturation, 0.0f, 3.0f, 1.0f);
        UiUtils.SliderWithReset("Gamma", ref e.Gamma, 0.1f, 3.0f, 1.0f);
        UiUtils.SliderWithReset("Exposure", ref e.Exposure, -2.0f, 2.0f, 0.0f);
        UiUtils.SliderWithReset("Vibrance", ref e.Vibrance, -1.0f, 1.0f, 0.0f);
        ImGui.ColorEdit4("Global Color", ref e.Color);
        UiUtils.SliderWithReset("Bloom", ref e.Bloom, 0.0f, 5.0f, 0.0f);
        UiUtils.SliderWithReset("Strobe", ref e.Strobe, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Kick Pulse", ref e.KickIntensity, 0.0f, 5.0f, 1.0f);

        ImGui.Separator();
        ImGui.Text("Color Grading");
        UiUtils.SliderWithReset("Tint Mix", ref e.TintIntensity, 0.0f, 1.0f, 0.0f);
        ImGui.ColorEdit3("Tint Color", ref e.TintColor);
        UiUtils.SliderWithReset("Posterize", ref e.Posterize, 0.0f, 32.0f, 0.0f);
        UiUtils.SliderWithReset("Sepia", ref e.Sepia, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Invert", ref e.Invert, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Solarize", ref e.Solarize, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Color Reduce", ref e.ColorReduce, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Hue Shift", ref e.HueShift, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Thermal", ref e.Thermal, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Night Vision", ref e.NightVision, 0.0f, 1.0f, 0.0f);

        ImGui.Separator();
        ImGui.Text("Geometry");
        UiUtils.SliderWithReset("Zoom / Scale", ref e.Scale, 0.1f, 5.0f, 1.0f);
        UiUtils.SliderWithReset("Fish Eye", ref e.Fisheye, -1.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Mirror (0-3)", ref e.Mirror, 0.0f, 3.0f, 0.0f);
        UiUtils.SliderWithReset("Vortex", ref e.Vortex, -1.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Wave", ref e.Wave, 0.0f, 5.0f, 0.0f);
        UiUtils.SliderWithReset("Swirl", ref e.Swirl, -10.0f, 10.0f, 0.0f);
        UiUtils.SliderWithReset("Spiral", ref e.Spiral, -10.0f, 10.0f, 0.0f);
        UiUtils.SliderWithReset("Ripple", ref e.Ripple, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Polar", ref e.Polar, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Kaleidoscope", ref e.Kaleidoscope, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Mosaic", ref e.Mosaic, 0.0f, 5.0f, 0.0f);
        UiUtils.SliderWithReset("Rotation", ref e.Rotation, 0.0f, 360.0f, 0.0f);
        UiUtils.SliderWithReset("Rotation Speed", ref e.RotationSpeed, -180.0f, 180.0f, 0.0f);

        ImGui.Separator();
        ImGui.Text("Glitch & Retro");
        UiUtils.SliderWithReset("Glitch", ref e.Glitch, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Glitch Hard", ref e.GlitchHard, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Glitch Analog", ref e.GlitchAnalog, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("RGB Split", ref e.RgbSplit, 0.0f, 0.1f, 0.0f);
        UiUtils.SliderWithReset("RGB Shift V", ref e.RgbShiftVert, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Chromatic Aberration", ref e.ChromaticAberration, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Pixelate", ref e.Pixelate, 1.0f, 50.0f, 1.0f);
        UiUtils.SliderWithReset("Data Moshing", ref e.Datamosh, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Block Noise", ref e.BlockNoise, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("VHS", ref e.Vhs, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("CRT", ref e.Crt, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Dot Matrix", ref e.DotMatrix, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Scanlines", ref e.Scanlines, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Noise", ref e.Noise, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Vignette", ref e.Vignette, 0.0f, 2.0f, 0.0f);

        ImGui.Separator();
        ImGui.Text("Artistic");
        UiUtils.SliderWithReset("Frosted Glass", ref e.FrostedGlass, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Blur", ref e.Blur, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Zoom Blur", ref e.ZoomBlur, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Sharpen", ref e.Sharpen, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Emboss", ref e.Emboss, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Edge Detect", ref e.EdgeDetect, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Sobel Neon", ref e.SobelNeon, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Crosshatch", ref e.Crosshatch, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Dither", ref e.Dither, 0.0f, 1.0f, 0.0f);
        UiUtils.SliderWithReset("Halftone", ref e.Halftone, 0.0f, 2.0f, 0.0f);
        UiUtils.SliderWithReset("Film Grain", ref e.FilmGrain, 0.0f, 1.0f, 0.0f);

        ImGui.Separator();
        ImGui.Text("Motion");
        UiUtils.SliderWithReset("Time Speed", ref e.Speed, 0.0f, 5.0f, 1.0f);
        UiUtils.SliderWithReset("Shake", ref e.Shake, 0.0f, 5.0f, 0.0f);
        UiUtils.SliderWithReset("Motion Blur", ref e.MotionBlur, 0.0f, 0.99f, 0.0f);

        ImGui.Text("Feedback Zoom");
        UiUtils.SliderWithReset("Feedback Opacity", ref e.FeedbackOpacity, 0.0f, 0.99f, 0.0f);
        UiUtils.SliderWithReset("Feedback Scale", ref e.FeedbackScale, 0.5f, 1.5f, 1.0f);
        UiUtils.SliderWithReset("Feedback Rotation", ref e.FeedbackRotation, -0.1f, 0.1f, 0.0f);

        ImGui.Separator();
        ImGui.Text("Particle System");
        ImGui.Checkbox("Enable Particles", ref e.ParticlesActive);
        if (e.ParticlesActive)
        {
            UiUtils.SliderWithReset("Point Size", ref e.ParticlesSize, 1.0f, 100.0f, 10.0f);
            ImGui.ColorEdit4("Particles Color", ref e.ParticlesColor);
            ImGui.SliderInt("Count (Reset)", ref e.ParticlesCount, 100, 10000);
        }

        if (manager.CurrentShaderName != null && manager.CurrentShaderName.Contains("mandelbulb"))
        {
            ImGui.Separator();
            ImGui.Text("Shader-Specific");
            UiUtils.SliderWithReset("Fractal Power", ref e.ShaderParam1, 1.0f, 20.0f, 8.0f);
        }
    }
}
