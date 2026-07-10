// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using System.Numerics;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.Core.Gui;

public static class EffectsPanel
{
    private static bool _simpleMode = true;
    private static string _selectedSection = "Home";
    private static string? _requestedSettingsTab;
    private static bool _dockLayoutInitialized;
    private static NativeDockBuilder.LayoutPreset _layoutPreset = NativeDockBuilder.LayoutPreset.Standard;
    private static readonly FadeTracker _inspectorFade = new();

    private static readonly Dictionary<string, (string Section, string? SettingsTab)> LegacyPanelMap = new()
    {
        ["General"] = ("Home", null),
        ["Home"] = ("Home", null),
        ["System"] = ("Settings", "System"),
        ["Layers"] = ("Layers", null),
        ["Timeline"] = ("Timeline", null),
        ["Audio"] = ("Audio", null),
        ["Media"] = ("Settings", "Media"),
        ["3D Model"] = ("3D", null),
        ["3D"] = ("3D", null),
        ["Templates"] = ("Shaders", null),
        ["Shaders"] = ("Shaders", null),
        ["Presets"] = ("Effects", null),
        ["Effects"] = ("Effects", null),
        ["Live Coding"] = ("Settings", "Live Coding"),
        ["Export"] = ("Export", null),
        ["Migration"] = ("Settings", "Migration"),
        ["Window"] = ("Settings", "Window"),
        ["Shader / Snippet / Track Management"] = ("Settings", "Shader Mgmt"),
        ["Debug / Performance"] = ("Settings", "Debug"),
        ["Settings"] = ("Settings", null),
        ["About"] = ("Settings", "About"),
    };

    private static readonly (string Tab, bool AdvancedOnly)[] SettingsTabs =
    {
        ("System", false),
        ("Media", false),
        ("Window", false),
        ("Live Coding", true),
        ("Migration", true),
        ("Shader Mgmt", true),
        ("Debug", true),
        ("Shader Errors", true),
        ("About", false),
    };

    public static void RequestOpenPanel(string panelName)
    {
        if (!LegacyPanelMap.TryGetValue(panelName, out var target))
        {
            target = ("Home", null);
        }

        _selectedSection = target.Section;
        _requestedSettingsTab = target.SettingsTab;

        bool advancedOnlyTarget = target.SettingsTab != null && Array.Exists(SettingsTabs, t => t.Tab == target.SettingsTab && t.AdvancedOnly);
        if (advancedOnlyTarget) _simpleMode = false;
    }

    public static void Draw(ShaderManager manager, AppSettings settings, string settingsFilePath, string layersFilePath, TimelineEngine timeline, string timelineFilePath, string presetsFilePath, SecondaryWindow preview, string shaderDirectory, uint dockspaceId, GlWindow window)
    {
        if (ImGui.GetIO().KeyCtrl && ImGui.IsKeyPressed(ImGuiKey.K))
        {
            CommandPalette.Toggle();
        }

        CommandPalette.Draw(manager, settings);
        WelcomePanel.Draw(manager, settings, settingsFilePath);
        GuidedTour.Draw(settings, settingsFilePath);
        DemoWizardPanel.Draw(manager, settings);
        QuickExportPanel.Draw(manager, settings, timeline);
        HelpPanel.Draw();

        if (!_dockLayoutInitialized && dockspaceId != 0)
        {
            _dockLayoutInitialized = true;
            NativeDockBuilder.ApplyPreset(dockspaceId, _layoutPreset, ImGui.GetMainViewport().Size);
        }

        DrawNavigation(manager, settings, settingsFilePath, dockspaceId);
        DrawPreview(manager);
        DrawInspector(manager, settings, settingsFilePath, layersFilePath, timeline, timelineFilePath, presetsFilePath, preview, shaderDirectory, window);

        if (_layoutPreset == NativeDockBuilder.LayoutPreset.PowerUser)
        {
            DrawTimelineDock(timeline, manager, timelineFilePath);
        }
    }

    private static void DrawNavigation(ShaderManager manager, AppSettings settings, string settingsFilePath, uint dockspaceId)
    {
        Elevation.PushDockedBorder();
        Elevation.SuppressDockMenuButton();
        ImGui.Begin("Navigation");

        bool pushedTitle = Theme.PushFontIf(Theme.FontTitle);
        ImGui.TextColored(Theme.Accent, "ShaderDemo");
        Theme.PopFontIf(pushedTitle);
        ImGui.TextColored(Theme.TextMuted, $"{manager.CurrentShaderName ?? "<none>"}");
        ImGui.Separator();

        DrawNavButton("Home");
        DrawNavButton("Shaders", Theme.SectionShaders);
        DrawNavButton("Layers", Theme.SectionLayers);
        DrawNavButton("Effects", Theme.SectionEffects);
        DrawNavButton("Audio", Theme.SectionAudio);
        DrawNavButton("Timeline", Theme.SectionTimeline);
        DrawNavButton("3D", Theme.Section3D);
        DrawNavButton("Export", Theme.SectionExport);
        DrawNavButton("Settings");

        ImGui.Separator();
        ImGui.Text("Mode:");
        if (ImGui.RadioButton("Simple", _simpleMode)) _simpleMode = true;
        ImGui.SameLine();
        if (ImGui.RadioButton("Advanced", !_simpleMode)) _simpleMode = false;

        ImGui.Separator();
        if (ImGui.Button("Command Palette (Ctrl+K)", new Vector2(-1, 0))) CommandPalette.Open();
        if (ImGui.Button("Demo Wizard", new Vector2(-1, 0))) DemoWizardPanel.Open();
        if (ImGui.Button("Quick Export", new Vector2(-1, 0))) QuickExportPanel.Open();
        if (ImGui.Button("Help", new Vector2(-1, 0))) HelpPanel.Open();

        ImGui.Separator();
        if (ImGui.Button("Save Settings", new Vector2(-1, 0)))
        {
            settings.Effects.CopyFrom(manager.Effects);
            SettingsService.Save(settings, settingsFilePath);
        }

        if (ImGui.Button("Load Settings", new Vector2(-1, 0)))
        {
            AppSettings loaded = SettingsService.Load(settingsFilePath);
            settings.Effects.CopyFrom(loaded.Effects);
            settings.MusicFile = loaded.MusicFile;
            settings.MusicVolume = loaded.MusicVolume;
            settings.AudioReactive = loaded.AudioReactive;
            settings.AutoSaveSettings = loaded.AutoSaveSettings;
            manager.Effects.CopyFrom(settings.Effects);
        }

        bool autoSave = settings.AutoSaveSettings;
        if (ImGui.Checkbox("Auto-Save on Exit", ref autoSave)) settings.AutoSaveSettings = autoSave;

        ImGui.Separator();
        ImGui.TextColored(Theme.TextMuted, "Layout preset:");
        DrawLayoutPresetButton("Simple", NativeDockBuilder.LayoutPreset.Simple, dockspaceId);
        DrawLayoutPresetButton("Standard", NativeDockBuilder.LayoutPreset.Standard, dockspaceId);
        DrawLayoutPresetButton("Power User", NativeDockBuilder.LayoutPreset.PowerUser, dockspaceId);

        ImGui.End();
        Elevation.PopDockedBorder();
    }

    private static void DrawNavButton(string section, Vector4? accentColor = null)
    {
        bool selected = _selectedSection == section;
        if (selected) ImGui.PushStyleColor(ImGuiCol.Button, Theme.AccentActive);
        bool clicked = ImGui.Button(section, new Vector2(-1, 0));
        if (selected) ImGui.PopStyleColor();

        if (accentColor is { } color)
        {
            Vector2 min = ImGui.GetItemRectMin();
            Vector2 max = ImGui.GetItemRectMax();
            ImGui.GetWindowDrawList().AddRectFilled(min, new Vector2(min.X + 3.0f, max.Y), ImGui.GetColorU32(color), 2.0f);
        }

        if (clicked) _selectedSection = section;
    }

    private static void DrawLayoutPresetButton(string label, NativeDockBuilder.LayoutPreset preset, uint dockspaceId)
    {
        if (ImGui.Button(label, new Vector2(-1, 0)) && dockspaceId != 0)
        {
            _layoutPreset = preset;
            NativeDockBuilder.ApplyPreset(dockspaceId, preset, ImGui.GetMainViewport().Size);
        }
    }

    private static void DrawPreview(ShaderManager manager)
    {
        Elevation.PushDockedBorder();
        Elevation.SuppressDockMenuButton();
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.Begin("Preview");

        Framebuffer? frame = manager.LastComposedFrame;
        if (frame == null)
        {
            ImGui.TextColored(Theme.TextMuted, "Rendering...");
        }
        else
        {
            Vector2 available = ImGui.GetContentRegionAvail();
            float aspect = (float)frame.Width / frame.Height;
            Vector2 size = available.X / available.Y > aspect
                ? new Vector2(available.Y * aspect, available.Y)
                : new Vector2(available.X, available.X / aspect);

            Vector2 offset = (available - size) * 0.5f;
            if (offset.X > 0 || offset.Y > 0)
            {
                ImGui.SetCursorPos(ImGui.GetCursorPos() + new Vector2(Math.Max(offset.X, 0), Math.Max(offset.Y, 0)));
            }

            ImGui.Image((nint)frame.ColorTexture, size, new Vector2(0, 1), new Vector2(1, 0));

            Vector2 imageMin = ImGui.GetItemRectMin();
            Vector2 imageMax = ImGui.GetItemRectMax();
            DrawPreviewVignette(imageMin, imageMax);
            DrawPreviewOverlay(manager, imageMin);
        }

        ImGui.End();
        ImGui.PopStyleVar();
        Elevation.PopDockedBorder();
    }

    private static void DrawPreviewVignette(Vector2 imageMin, Vector2 imageMax)
    {
        ImDrawListPtr dl = ImGui.GetWindowDrawList();
        const float thickness = 28.0f;
        uint transparent = ImGui.GetColorU32(new Vector4(0, 0, 0, 0));
        uint dark = ImGui.GetColorU32(new Vector4(0, 0, 0, 0.30f));

        dl.AddRectFilledMultiColor(imageMin, new Vector2(imageMax.X, imageMin.Y + thickness), dark, dark, transparent, transparent);
        dl.AddRectFilledMultiColor(new Vector2(imageMin.X, imageMax.Y - thickness), imageMax, transparent, transparent, dark, dark);
        dl.AddRectFilledMultiColor(imageMin, new Vector2(imageMin.X + thickness, imageMax.Y), dark, transparent, transparent, dark);
        dl.AddRectFilledMultiColor(new Vector2(imageMax.X - thickness, imageMin.Y), imageMax, transparent, dark, dark, transparent);
        dl.AddRect(imageMin, imageMax, ImGui.GetColorU32(Theme.Border), 0.0f, ImDrawFlags.None, 1.0f);
    }

    private static void DrawPreviewOverlay(ShaderManager manager, Vector2 imageMin)
    {
        float fps = ImGui.GetIO().Framerate;
        string fpsText = $"{fps:F0} FPS";
        bool recording = manager.Recorder.IsRecording;

        ImFontPtr font = Theme.FontMono ?? ImGui.GetFont();
        Vector2 fpsSize = ImGui.CalcTextSize(fpsText) * (13.0f / ImGui.GetFontSize());
        Vector2 pos = imageMin + new Vector2(Theme.SpaceSm, Theme.SpaceSm);

        ImDrawListPtr dl = ImGui.GetWindowDrawList();
        Vector2 badgeMax = pos + fpsSize + new Vector2(Theme.SpaceSm, Theme.SpaceXs);
        dl.AddRectFilled(pos - new Vector2(Theme.SpaceXs, Theme.SpaceXs * 0.5f), badgeMax, ImGui.GetColorU32(new Vector4(0, 0, 0, 0.55f)), 4.0f);
        dl.AddText(font, 13.0f, pos, ImGui.GetColorU32(Theme.Text), fpsText);

        if (recording)
        {
            Vector2 dotCenter = pos + new Vector2(fpsSize.X + Theme.SpaceMd, fpsSize.Y * 0.5f);
            float pulse = 0.5f + 0.5f * MathF.Sin((float)ImGui.GetTime() * 4.0f);
            Vector4 dotColor = Vector4.Lerp(Theme.Danger, new Vector4(1, 1, 1, 1), pulse * 0.35f);
            dl.AddCircleFilled(dotCenter, 5.0f, ImGui.GetColorU32(dotColor), 12);
        }
    }

    private static void DrawTimelineDock(TimelineEngine timeline, ShaderManager manager, string timelineFilePath)
    {
        Elevation.PushDockedBorder();
        Elevation.SuppressDockMenuButton();
        ImGui.Begin("Timeline Dock");
        TimelinePanel.Draw(timeline, manager, timelineFilePath);
        ImGui.End();
        Elevation.PopDockedBorder();
    }

    private static void DrawInspector(ShaderManager manager, AppSettings settings, string settingsFilePath, string layersFilePath, TimelineEngine timeline, string timelineFilePath, string presetsFilePath, SecondaryWindow preview, string shaderDirectory, GlWindow window)
    {
        Elevation.PushDockedBorder();
        Elevation.SuppressDockMenuButton();
        ImGui.Begin("Inspector");

        float inspectorAlpha = _inspectorFade.Value(_selectedSection);
        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, inspectorAlpha);

        switch (_selectedSection)
        {
            case "Home":
                PanelDescriptions.Draw("General");
                GeneralPanel.Draw(manager, settings, timeline);
                break;

            case "Shaders":
                DrawShaderSelector(manager);
                ImGui.Separator();
                PanelDescriptions.Draw("Templates");
                TemplatesPanel.Draw(manager, settings);
                break;

            case "Layers":
                PanelDescriptions.Draw("Layers");
                LayersPanel.Draw(manager, layersFilePath);
                break;

            case "Timeline":
                if (_layoutPreset == NativeDockBuilder.LayoutPreset.PowerUser)
                {
                    ImGui.TextColored(Theme.TextMuted, "Pinned at the bottom of the workspace in the Power User layout.");
                }
                else
                {
                    PanelDescriptions.Draw("Timeline");
                    TimelinePanel.Draw(timeline, manager, timelineFilePath);
                }

                break;

            case "Audio":
                PanelDescriptions.Draw("Audio");
                AudioPanel.Draw(manager, settings);
                break;

            case "3D":
                PanelDescriptions.Draw("3D Model");
                Model3DPanel.Draw(manager);
                break;

            case "Effects":
                if (_simpleMode) MacroControlsPanel.Draw(manager);
                else DrawEffectSliders(manager);
                ImGui.Separator();
                PanelDescriptions.Draw("Presets");
                PresetsPanel.Draw(manager, presetsFilePath);
                break;

            case "Export":
                PanelDescriptions.Draw("Export");
                ExportPanel.Draw(manager, settings, timeline);
                break;

            case "Settings":
                DrawSettingsTabs(manager, settings, layersFilePath, timeline, timelineFilePath, settingsFilePath, preview, shaderDirectory, window);
                break;
        }

        ImGui.PopStyleVar();
        ImGui.End();
        Elevation.PopDockedBorder();
    }

    private static string _selectedSettingsTab = "System";

    private static void DrawSettingsTabs(ShaderManager manager, AppSettings settings, string layersFilePath, TimelineEngine timeline, string timelineFilePath, string settingsFilePath, SecondaryWindow preview, string shaderDirectory, GlWindow window)
    {
        if (_requestedSettingsTab != null)
        {
            _selectedSettingsTab = _requestedSettingsTab;
            _requestedSettingsTab = null;
        }

        bool first = true;
        foreach ((string tab, bool advancedOnly) in SettingsTabs)
        {
            if (advancedOnly && _simpleMode && _selectedSettingsTab != tab) continue;

            if (!first) ImGui.SameLine();
            first = false;

            bool selected = _selectedSettingsTab == tab;
            if (selected) ImGui.PushStyleColor(ImGuiCol.Button, Theme.AccentActive);
            if (ImGui.SmallButton(tab)) _selectedSettingsTab = tab;
            if (selected) ImGui.PopStyleColor();
        }

        ImGui.Separator();
        ImGui.Spacing();

        switch (_selectedSettingsTab)
        {
            case "System":
                PanelDescriptions.Draw("System");
                SystemPanel.Draw(manager, settings, timeline, settingsFilePath, layersFilePath, timelineFilePath, window);
                break;
            case "Media":
                PanelDescriptions.Draw("Media");
                MediaPanel.Draw(manager);
                break;
            case "Window":
                PanelDescriptions.Draw("Window");
                WindowPanel.Draw(preview, manager);
                break;
            case "Live Coding":
                PanelDescriptions.Draw("Live Coding");
                LiveCodingPanel.Draw(manager, shaderDirectory);
                break;
            case "Migration":
                PanelDescriptions.Draw("Migration");
                MigrationPanel.Draw(manager, settings, timeline, settingsFilePath, layersFilePath, timelineFilePath);
                break;
            case "Shader Mgmt":
                PanelDescriptions.Draw("Shader / Snippet / Track Management");
                PopupsPanel.Draw(manager, timeline, shaderDirectory);
                break;
            case "Debug":
                PanelDescriptions.Draw("Debug / Performance");
                DebugOverlayPanel.Draw(manager);
                break;
            case "Shader Errors":
                ImGui.Text($"{ShaderErrorLog.Errors.Count} error(s)");
                ShaderErrorsPanel.Draw(manager);
                break;
            case "About":
                AboutPanel.Draw();
                break;
        }
    }

    private static int _transitionType;
    private static float _transitionDuration = 1.5f;
    private static readonly string[] TransitionTypeNames = { "Wipe", "Fade", "Zoom", "Pixelize" };

    private static void DrawShaderSelector(ShaderManager manager)
    {
        ImGui.Text($"Current shader: {manager.CurrentShaderName ?? "<none>"}");
        if (manager.IsTransitioning) ImGui.TextColored(Theme.Info, "Transitioning...");

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

        Elevation.BeginCard();
        Theme.Heading("Color & Light");
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
        Elevation.EndCard();

        Elevation.BeginCard();
        Theme.Heading("Color Grading");
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
        Elevation.EndCard();

        Elevation.BeginCard();
        Theme.Heading("Geometry");
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
        Elevation.EndCard();

        Elevation.BeginCard();
        Theme.Heading("Glitch & Retro");
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
        Elevation.EndCard();

        Elevation.BeginCard();
        Theme.Heading("Artistic");
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
        Elevation.EndCard();

        Elevation.BeginCard();
        Theme.Heading("Motion");
        UiUtils.SliderWithReset("Time Speed", ref e.Speed, 0.0f, 5.0f, 1.0f);
        UiUtils.SliderWithReset("Shake", ref e.Shake, 0.0f, 5.0f, 0.0f);
        UiUtils.SliderWithReset("Motion Blur", ref e.MotionBlur, 0.0f, 0.99f, 0.0f);
        Elevation.EndCard();

        Elevation.BeginCard();
        Theme.Heading("Feedback Zoom");
        UiUtils.SliderWithReset("Feedback Opacity", ref e.FeedbackOpacity, 0.0f, 0.99f, 0.0f);
        UiUtils.SliderWithReset("Feedback Scale", ref e.FeedbackScale, 0.5f, 1.5f, 1.0f);
        UiUtils.SliderWithReset("Feedback Rotation", ref e.FeedbackRotation, -0.1f, 0.1f, 0.0f);
        Elevation.EndCard();

        Elevation.BeginCard();
        Theme.Heading("Particle System");
        ImGui.Checkbox("Enable Particles", ref e.ParticlesActive);
        if (e.ParticlesActive)
        {
            UiUtils.SliderWithReset("Point Size", ref e.ParticlesSize, 1.0f, 100.0f, 10.0f);
            ImGui.ColorEdit4("Particles Color", ref e.ParticlesColor);
            ImGui.SliderInt("Count (Reset)", ref e.ParticlesCount, 100, 10000);
        }

        Elevation.EndCard();

        if (manager.CurrentShaderName != null && manager.CurrentShaderName.Contains("mandelbulb"))
        {
            Elevation.BeginCard();
            Theme.Heading("Shader-Specific");
            UiUtils.SliderWithReset("Fractal Power", ref e.ShaderParam1, 1.0f, 20.0f, 8.0f);
            Elevation.EndCard();
        }
    }
}
