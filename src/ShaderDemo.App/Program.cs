// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using System.Runtime.Versioning;
using ShaderDemo.Core;
using ShaderDemo.Core.Gui;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;
using ShaderDemo.Core.Timeline;

[assembly: SupportedOSPlatform("windows")]

Console.WriteLine($"{AppInfo.Name} v{AppInfo.Version}");
Console.WriteLine(AppInfo.Copyright);

if (args.Length > 0 && args[0] == "--test-full")
{
    int testResult = ShaderDemo.App.FullFeatureTest.Run(args).Result;
    Environment.Exit(testResult);
}

if (args.Length > 1 && args[0] == "--test-shader")
{
    string targetShaderName = args[1];
    using var testWindow = new GlWindow(AppInfo.Name, 640, 360, fullscreen: false);
    ShaderManager? testEngine = null;
    int testFrame = 0;

    testWindow.Load += () =>
    {
        testEngine = new ShaderManager(testWindow.Api!, testWindow.Width, testWindow.Height);
        ShaderLoader.LoadFromDirectory(testEngine, Path.Combine(AppContext.BaseDirectory, "shaders"), Console.WriteLine);
        int idx = testEngine.ShaderNames.ToList().IndexOf(targetShaderName);
        if (idx < 0)
        {
            Console.WriteLine($"[test-shader] '{targetShaderName}' not found or failed to compile");
        }
        else
        {
            testEngine.SelectShader(idx);
            Console.WriteLine($"[test-shader] Selected '{targetShaderName}' at index {idx}");
        }
    };
    testWindow.RenderFrame += _ =>
    {
        testEngine?.RenderFrame();
        testFrame++;
        if (testFrame == 30)
        {
            if (testEngine != null)
            {
                string path = ShaderDemo.Core.Export.ScreenshotService.Save(testEngine.LastComposedFrame ?? testEngine.Pipeline.SceneFbo, "screenshots");
                Console.WriteLine($"[test-shader] Screenshot saved: {path}");
            }

            testWindow.RequestClose();
        }
    };
    testWindow.Closing += () => testEngine?.Dispose();
    testWindow.Run();
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
Console.WriteLine(File.Exists(settingsFilePath) ? $"Loaded settings from {settingsFilePath}" : $"No settings.json found, using defaults ({settingsFilePath})");

using var window = new GlWindow(AppInfo.Name, appSettings.WindowWidth, appSettings.WindowHeight, fullscreen: false);
ShaderManager? engine = null;
var previewWindow = new SecondaryWindow(window.NativeWindow);
using var shaderWatcher = new ShaderHotReloadWatcher(shaderDirectory);

window.Load += () =>
{
    engine = new ShaderManager(window.Api!, window.Width, window.Height);
    engine.RegisterShader("TestPattern", testPatternShader);

    int loaded = ShaderLoader.LoadFromDirectory(engine, shaderDirectory, Console.WriteLine);
    Console.WriteLine(loaded > 0 ? $"Loaded {loaded} shader(s) from {shaderDirectory}" : $"No shaders found in {shaderDirectory}, using built-in test pattern");

    engine.SelectShader(0);
    engine.Effects.CopyFrom(appSettings.Effects);
    engine.Audio.Enabled = appSettings.AudioReactive;
    engine.Timeline = timeline;

    if (window.Keyboard != null)
    {
        window.Keyboard.KeyDown += (_, key, _) =>
        {
            if (key == Silk.NET.Input.Key.Right) engine.NextShader();
            else if (key == Silk.NET.Input.Key.Left) engine.PreviousShader();
        };
    }
};

window.UpdateFrame += deltaSeconds =>
{
    engine?.Update(deltaSeconds);

    if (window.Mouse != null && engine != null)
    {
        engine.MousePosition = new Vector2(window.Mouse.Position.X, window.Mouse.Position.Y);
    }

    if (timeline.Active && engine != null)
    {
        timeline.ApplyEffects(engine.Effects, engine.ElapsedTime);
        timeline.ApplyShader(engine, engine.ElapsedTime);
    }

    if (engine != null)
    {
        shaderWatcher.ProcessPending(engine, Console.WriteLine);
    }
};

window.RenderFrame += _ =>
{
    engine?.RenderFrame();
    if (previewWindow.IsOpen) previewWindow.RenderFrame(engine?.LastComposedFrame);
};

window.ImGuiRender += () =>
{
    if (engine != null)
    {
        EffectsPanel.Draw(engine, appSettings, settingsFilePath, layersFilePath, timeline, timelineFilePath, presetsFilePath, previewWindow);
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
            Console.WriteLine($"Auto-saved settings to {settingsFilePath}");
        }
    }

    previewWindow.Dispose();
    engine?.Dispose();
};

window.Run();
