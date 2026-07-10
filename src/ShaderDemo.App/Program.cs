// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using System.Runtime.Versioning;
using ShaderDemo.Core;
using ShaderDemo.Core.Audio;
using ShaderDemo.Core.Export;
using ShaderDemo.Core.Gui;
using ShaderDemo.Core.Logging;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;
using ShaderDemo.Core.Timeline;

[assembly: SupportedOSPlatform("windows")]

AppLog.Initialize(Path.Combine(AppContext.BaseDirectory, "logs", "shaderdemo.log"));
CrashHandler.Install(AppContext.BaseDirectory);

AppLog.Info($"{AppInfo.Name} v{AppInfo.Version}");
AppLog.Info(AppInfo.Copyright);

if (args.Length > 0 && args[0] == "--test-full")
{
    int testResult = ShaderDemo.App.FullFeatureTest.Run(args).Result;
    Environment.Exit(testResult);
}

if (args.Length > 1 && args[0] == "--test-shader")
{
    ShaderDemo.App.ShaderCompileTest.Run(args[1]);
    Environment.Exit(0);
}

if (args.Length > 0 && args[0] == "--test-timeline-ui")
{
    ShaderDemo.App.TimelineUiTest.Run();
    Environment.Exit(0);
}

if (args.Length > 1 && args[0] == "--test-audio")
{
    ShaderDemo.App.AudioAnalysisTest.Run(args[1]);
    Environment.Exit(0);
}

if (args.Length > 0 && args[0] == "--test-audio-live")
{
    ShaderDemo.App.AudioAnalysisTest.RunLiveDeviceCheck();
    Environment.Exit(0);
}

if (args.Length > 0 && args[0] == "--test-gui-panels")
{
    ShaderDemo.App.GuiPanelsTest.Run(Path.Combine(AppContext.BaseDirectory, "shaders"));
    Environment.Exit(0);
}

if (args.Length > 0 && args[0] == "--test-project-features")
{
    ShaderDemo.App.ProjectFeaturesTest.Run();
    Environment.Exit(0);
}

if (args.Length > 0 && args[0] == "--test-window-features")
{
    ShaderDemo.App.WindowFeaturesTest.Run();
    Environment.Exit(0);
}

if (args.Length > 1 && args[0] == "--test-migration")
{
    ShaderDemo.App.MigrationTest.Run(args[1]);
    Environment.Exit(0);
}

if (args.Length > 1 && args[0] == "--test-video-export")
{
    ShaderDemo.App.VideoExportTest.Run(args[1], Path.Combine(AppContext.BaseDirectory, "shaders"));
    Environment.Exit(0);
}

if (args.Length > 1 && args[0] == "--test-gif-export")
{
    ShaderDemo.App.VideoExportTest.Run(args[1], Path.Combine(AppContext.BaseDirectory, "shaders"), isGif: true);
    Environment.Exit(0);
}

if (args.Length > 0 && args[0] == "--test-font-glyphs")
{
    ShaderDemo.App.FontGlyphTest.Run();
    Environment.Exit(0);
}

if (args.Length > 0 && args[0] == "--test-shader-revert")
{
    ShaderDemo.App.ShaderRevertTest.Run();
    Environment.Exit(0);
}

if (args.Length > 0 && args[0] == "--test-effects-panel")
{
    ShaderDemo.App.EffectsPanelTest.Run(Path.Combine(AppContext.BaseDirectory, "shaders"));
    Environment.Exit(0);
}

if (args.Length > 0 && args[0] == "--test-docked-workspace")
{
    ShaderDemo.App.DockedWorkspaceTest.Run(Path.Combine(AppContext.BaseDirectory, "shaders"));
    Environment.Exit(0);
}

if (args.Length > 0 && args[0] == "--test-workflow-panels")
{
    ShaderDemo.App.WorkflowPanelsTest.Run(Path.Combine(AppContext.BaseDirectory, "shaders"));
    Environment.Exit(0);
}

if (args.Length > 0 && args[0] == "--test-particle-benchmark")
{
    ShaderDemo.App.ParticleBenchmarkTest.Run();
    Environment.Exit(0);
}

if (args.Length > 1 && args[0] == "--test-shader-cache")
{
    bool useCache = args[1] == "on";
    string cacheDir = Path.Combine(Path.GetTempPath(), "shaderdemo_cache_test");
    ShaderDemo.App.ShaderCacheTest.RunSingleProcessScenario(Path.Combine(AppContext.BaseDirectory, "shaders"), useCache, cacheDir);
    Environment.Exit(0);
}

if (args.Length > 0 && args[0] == "--test-adaptive-resolution")
{
    ShaderDemo.App.AdaptiveResolutionTest.Run(Path.Combine(AppContext.BaseDirectory, "shaders"));
    Environment.Exit(0);
}

if (args.Length > 1 && args[0] == "--test-image-layer")
{
    ShaderDemo.App.ImageLayerTest.Run(Path.Combine(AppContext.BaseDirectory, "shaders"), args[1]);
    Environment.Exit(0);
}

if (args.Length > 1 && args[0] == "--test-timeline-image")
{
    ShaderDemo.App.TimelineImageLayerTest.Run(Path.Combine(AppContext.BaseDirectory, "shaders"), args[1]);
    Environment.Exit(0);
}

if (args.Length > 0 && args[0] == "--test-template-layer")
{
    ShaderDemo.App.TemplateLayerTest.Run(Path.Combine(AppContext.BaseDirectory, "shaders"));
    Environment.Exit(0);
}

if (args.Length > 1 && args[0] == "--test-text-model-layer")
{
    ShaderDemo.App.TextModelLayerTest.Run(Path.Combine(AppContext.BaseDirectory, "shaders"), args[1]);
    Environment.Exit(0);
}

if (args.Length > 2 && args[0] == "--test-timeline-gaps")
{
    ShaderDemo.App.TimelineGapsTest.Run(Path.Combine(AppContext.BaseDirectory, "shaders"), args[1], args[2]);
    Environment.Exit(0);
}

if (args.Length > 1 && args[0] == "--test-export-resolution")
{
    ShaderDemo.App.ExportResolutionTest.Run(Path.Combine(AppContext.BaseDirectory, "shaders"), args[1]);
    Environment.Exit(0);
}

if (args.Length > 2 && args[0] == "--test-megademo-showcase")
{
    ShaderDemo.App.MegademoShowcaseTest.Run(Path.Combine(AppContext.BaseDirectory, "shaders"), args[1], args[2]);
    Environment.Exit(0);
}

if (args.Length > 1 && args[0] == "--test-layer-benchmark")
{
    ShaderDemo.App.LayerBenchmarkTest.Run(Path.Combine(AppContext.BaseDirectory, "shaders"), args[1]);
    Environment.Exit(0);
}

if (args.Length > 1 && args[0] == "--test-texture-budget")
{
    ShaderDemo.App.TextureBudgetTest.Run(Path.Combine(AppContext.BaseDirectory, "shaders"), args[1]);
    Environment.Exit(0);
}

if (args.Length > 0 && args[0] == "--benchmark")
{
    double benchmarkDuration = args.Length > 1 && double.TryParse(args[1], out double parsedDuration) ? parsedDuration : 15.0;
    ShaderDemo.App.BenchmarkTest.Run(Path.Combine(AppContext.BaseDirectory, "shaders"), benchmarkDuration);
    Environment.Exit(0);
}

const string testPatternShader = """
    void mainImage(out vec4 fragColor, in vec2 fragCoord) {
        vec2 uv = fragCoord / iResolution.xy;
        vec3 color = 0.5 + 0.5 * cos(iTime + uv.xyx + vec3(0.0, 2.0, 4.0));
        fragColor = vec4(color, 1.0);
    }
    """;

string shaderDirectory = Path.Combine(AppContext.BaseDirectory, "shaders");
if (!Directory.Exists(shaderDirectory))
{
    shaderDirectory = Path.Combine(Directory.GetCurrentDirectory(), "shaders");
}

string settingsFilePath = Path.Combine(AppContext.BaseDirectory, "settings.json");
string layersFilePath = Path.Combine(AppContext.BaseDirectory, "layers.json");
string timelineFilePath = Path.Combine(AppContext.BaseDirectory, "timeline.json");
string presetsFilePath = Path.Combine(AppContext.BaseDirectory, "presets.json");
var timeline = new TimelineEngine();
AppSettings appSettings = SettingsService.Load(settingsFilePath);
AppLog.Info(File.Exists(settingsFilePath) ? $"Loaded settings from {settingsFilePath}" : $"No settings.json found, using defaults ({settingsFilePath})");

var coldStartStopwatch = System.Diagnostics.Stopwatch.StartNew();
bool coldStartLogged = false;
bool shadersLoaded = false;
int splashFrames = 0;

using var window = new GlWindow(AppInfo.Name, appSettings.WindowWidth, appSettings.WindowHeight, fullscreen: false);
ShaderManager? engine = null;
var previewWindow = new SecondaryWindow(window.NativeWindow);
using var shaderWatcher = new ShaderHotReloadWatcher(shaderDirectory);
var dragDropHandler = new DragDropHandler();
bool showGui = true;
double lastDeltaSeconds = 0.0;

void HandleDroppedFiles(string[] files)
{
    if (engine == null) return;

    string[] imageExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };

    foreach (string file in files)
    {
        string ext = Path.GetExtension(file).ToLowerInvariant();

        if (AudioAnalyzer.IsSupportedFile(file))
        {
            appSettings.MusicFile = file;
            engine.Audio.Load(file, engine.ElapsedTime);
            engine.Audio.Enabled = appSettings.AudioReactive;
            engine.Player.Play(file, appSettings.MusicVolume);
            var fullAnalysis = AudioAnalyzer.AnalyzeFull(file);
            engine.AudioViz.SetAnalysis(fullAnalysis, engine.ElapsedTime);
            ToastManager.Show($"Dropped audio: {Path.GetFileName(file)}", ToastLevel.Success);
        }
        else if (Array.IndexOf(imageExtensions, ext) >= 0)
        {
            bool loaded = engine.LoadChannel0Texture(file, AppLog.Info);
            ToastManager.Show(loaded ? $"Dropped image: {Path.GetFileName(file)}" : "Failed to load dropped image", loaded ? ToastLevel.Success : ToastLevel.Danger);
        }
        else if (ext is ".glsl" or ".hlsl")
        {
            bool ok = ShaderLoader.LoadFile(engine, file, AppLog.Info);
            if (ok)
            {
                string shaderName = Path.GetFileName(file);
                for (int i = 0; i < engine.ShaderNames.Count; i++)
                {
                    if (engine.ShaderNames[i] == shaderName) { engine.SelectShader(i); break; }
                }
            }

            ToastManager.Show(ok ? $"Dropped shader loaded: {Path.GetFileName(file)}" : $"Failed to compile dropped shader: {Path.GetFileName(file)}", ok ? ToastLevel.Success : ToastLevel.Danger);
        }
        else if (ext == ".obj")
        {
            engine.Model.LoadModel(file, AppLog.Info);
            engine.ModelState.ShowModel = true;
            ToastManager.Show($"Dropped model: {Path.GetFileName(file)}", ToastLevel.Success);
        }
        else
        {
            ToastManager.Show($"Unsupported file type dropped: {Path.GetFileName(file)}", ToastLevel.Warning);
        }
    }
}

dragDropHandler.FilesDropped += HandleDroppedFiles;

window.Load += () =>
{
    engine = new ShaderManager(window.Api!, window.Width, window.Height);
    engine.RegisterShader("TestPattern", testPatternShader);
    engine.SelectShader(0);
    engine.Effects.CopyFrom(appSettings.Effects);
    engine.Audio.Enabled = appSettings.AudioReactive;
    engine.Timeline = timeline;

    dragDropHandler.Attach(window.NativeWindow);

    if (window.Keyboard != null)
    {
        window.Keyboard.KeyDown += (_, key, _) =>
        {
            switch (key)
            {
                case Silk.NET.Input.Key.Right:
                    engine.NextShader();
                    break;
                case Silk.NET.Input.Key.Left:
                    engine.PreviousShader();
                    break;
                case Silk.NET.Input.Key.Space:
                    engine.NextShader();
                    break;
                case Silk.NET.Input.Key.Escape:
                    window.RequestClose();
                    break;
                case Silk.NET.Input.Key.Tab:
                    showGui = !showGui;
                    break;
                case Silk.NET.Input.Key.F11:
                    window.ToggleFullscreen();
                    break;
                case Silk.NET.Input.Key.F1:
                    PerformanceHud.Toggle(engine);
                    break;
                case Silk.NET.Input.Key.F12:
                    string screenshotPath = ScreenshotService.Save(engine.LastComposedFrame ?? engine.Pipeline.SceneFbo, "screenshots");
                    AppLog.Info($"Screenshot: {screenshotPath}");
                    ToastManager.Show($"Screenshot saved: {screenshotPath}", ToastLevel.Success);
                    break;
                case Silk.NET.Input.Key.E:
                    if (engine.Recorder.IsRecording)
                    {
                        string finishedFile = engine.Recorder.OutputFile ?? "recording";
                        engine.Recorder.Stop();
                        AppLog.Info("Recording stopped.");
                        ToastManager.Show($"Export finished: {finishedFile}", ToastLevel.Success);
                    }
                    else
                    {
                        Directory.CreateDirectory("videos");
                        string filename = Path.Combine("videos", $"recording_{DateTime.Now:yyyyMMdd-HHmmss}.mp4");
                        engine.Recorder.Start(engine.Pipeline.Width, engine.Pipeline.Height, 60, null, filename, appSettings.MusicFile, 0.0, includeAudio: true, isGif: false, AppLog.Info);
                        ToastManager.Show("Recording started", ToastLevel.Info);
                    }

                    break;
            }
        };
    }
};

window.UpdateFrame += deltaSeconds =>
{
    lastDeltaSeconds = deltaSeconds;
    engine?.Update(deltaSeconds);

    if (window.Mouse != null && engine != null)
    {
        engine.MousePosition = new Vector2(window.Mouse.Position.X, window.Mouse.Position.Y);
    }

    if (timeline.Active && engine != null)
    {
        timeline.ApplyEffects(engine.Effects, engine.ElapsedTime);
        timeline.ApplyShader(engine, engine.ElapsedTime);
        timeline.ApplyImageLayers(engine, engine.ElapsedTime);
        timeline.ApplyModelLayers(engine, engine.ElapsedTime);
        timeline.ApplyMusicClip(engine, engine.ElapsedTime);
        timeline.ApplyLayerAutomation(engine, engine.ElapsedTime);
    }

    if (engine != null)
    {
        shaderWatcher.ProcessPending(engine, msg =>
        {
            AppLog.Info(msg);
            ToastManager.Show(msg, msg.Contains("Failed", StringComparison.Ordinal) ? ToastLevel.Danger : ToastLevel.Info);
        });
    }
};

window.RenderFrame += _ =>
{
    if (engine != null) engine.PresentToScreenEnabled = !showGui;
    engine?.RenderFrame();
    if (previewWindow.IsOpen) previewWindow.RenderFrame(engine?.LastComposedFrame);

    if (!coldStartLogged)
    {
        coldStartLogged = true;
        coldStartStopwatch.Stop();
        AppLog.Info($"Cold start (window creation to first rendered frame): {coldStartStopwatch.Elapsed.TotalMilliseconds:F0} ms");
    }
};

window.ImGuiRender += () =>
{
    if (engine != null && !shadersLoaded)
    {
        SplashScreen.Draw();
        splashFrames++;

        if (splashFrames >= 2)
        {
            int loaded = ShaderLoader.LoadFromDirectory(engine, shaderDirectory, AppLog.Info);
            AppLog.Info(loaded > 0 ? $"Loaded {loaded} shader(s) from {shaderDirectory}" : $"No shaders found in {shaderDirectory}, using built-in test pattern");
            AppLog.Info($"Shaders ready: {coldStartStopwatch.Elapsed.TotalMilliseconds:F0} ms since window creation");
            shadersLoaded = true;
        }

        return;
    }

    if (engine != null && showGui)
    {
        EffectsPanel.Draw(engine, appSettings, settingsFilePath, layersFilePath, timeline, timelineFilePath, presetsFilePath, previewWindow, shaderDirectory, window.DockspaceId);
    }

    ToastManager.Draw((float)lastDeltaSeconds);

    if (engine != null)
    {
        RecordingIndicator.Draw(engine);
        PerformanceHud.Draw(engine);
    }
};

window.Resized += (width, height) =>
{
    engine?.Resize(width, height);
};

window.Closing += () =>
{
    if (engine != null)
    {
        appSettings.Effects.CopyFrom(engine.Effects);
        appSettings.WindowWidth = window.Width;
        appSettings.WindowHeight = window.Height;

        if (appSettings.AutoSaveSettings)
        {
            SettingsService.Save(appSettings, settingsFilePath);
            AppLog.Info($"Auto-saved settings to {settingsFilePath}");
        }
    }

    previewWindow.Dispose();
    engine?.Dispose();
};

window.Run();
