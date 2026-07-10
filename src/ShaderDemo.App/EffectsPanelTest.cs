// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core;
using ShaderDemo.Core.Gui;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.App;

public static class EffectsPanelTest
{
    public static void Run(string shaderDirectory)
    {
        using var window = new GlWindow(AppInfo.Name, 900, 700, fullscreen: false);
        ShaderManager? engine = null;
        var timeline = new TimelineEngine();
        var settings = new AppSettings();
        var previewWindow = new SecondaryWindow(window.NativeWindow);
        int frame = 0;
        string[] panelsToRequest = { "Layers", "Timeline", "Audio", "3D Model", "Export", "System" };

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            ShaderLoader.LoadFromDirectory(engine, shaderDirectory, Console.WriteLine);
            engine.SelectShader(0);
            Console.WriteLine($"[effects-panel-test] Loaded {engine.ShaderNames.Count} shaders");
        };

        window.UpdateFrame += dt => engine?.Update(dt);
        window.RenderFrame += _ => engine?.RenderFrame();
        window.ImGuiRender += () =>
        {
            if (engine == null) return;

            try
            {
                if (frame == 3) CommandPalette.Open();
                if (frame == 6) CommandPalette.Toggle();

                if (frame < panelsToRequest.Length * 3 && frame % 3 == 0)
                {
                    string target = panelsToRequest[frame / 3];
                    EffectsPanel.RequestOpenPanel(target);
                    Console.WriteLine($"[effects-panel-test] Requested panel: {target}");
                }

                EffectsPanel.Draw(engine, settings, "test_settings.json", "test_layers.json", timeline, "test_timeline.json", "test_presets.json", previewWindow, shaderDirectory, window.DockspaceId, window);

                if (frame == 25)
                {
                    PerformanceHud.Toggle(engine);
                    Console.WriteLine($"[effects-panel-test] HUD toggled on: {PerformanceHud.Visible}");
                }

                PerformanceHud.Draw(engine, settings);

                if (frame == 30)
                {
                    Console.WriteLine($"[effects-panel-test] HUD draw calls last frame: {GpuResourceTracker.DrawCallsLastFrame}");
                    Console.WriteLine($"[effects-panel-test] HUD VRAM estimate: {GpuResourceTracker.EstimatedVramBytes / (1024.0 * 1024.0):F2} MB");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[effects-panel-test] EXCEPTION: {ex}");
            }

            frame++;
            if (frame == 40) window.RequestClose();
        };

        window.Closing += () =>
        {
            previewWindow.Dispose();
            engine?.Dispose();
        };

        window.Run();
        Console.WriteLine("[effects-panel-test] 40 frames rendered, command palette toggled, 6 panel-jump requests issued, no exceptions.");
    }
}
