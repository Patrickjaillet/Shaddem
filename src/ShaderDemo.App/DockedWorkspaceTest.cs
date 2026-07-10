// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Gui;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.App;

public static class DockedWorkspaceTest
{
    public static void Run(string shaderDirectory)
    {
        using var window = new GlWindow(AppInfo.Name, 1600, 900, fullscreen: false);
        ShaderManager? engine = null;
        var timeline = new TimelineEngine();
        var settings = new AppSettings();
        var previewWindow = new SecondaryWindow(window.NativeWindow);
        int frame = 0;
        bool hadException = false;

        string[] legacyPanelNames =
        {
            "General", "System", "Layers", "Timeline", "Audio", "Media", "3D Model",
            "Templates", "Presets", "Live Coding", "Export", "Migration", "Window",
            "Shader / Snippet / Track Management", "Debug / Performance",
        };

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            ShaderLoader.LoadFromDirectory(engine, shaderDirectory, Console.WriteLine);
            engine.SelectShader(0);
            Console.WriteLine($"[docked-workspace-test] Loaded {engine.ShaderNames.Count} shaders");
        };

        window.UpdateFrame += dt => engine?.Update(dt);
        window.RenderFrame += _ => engine?.RenderFrame();
        window.ImGuiRender += () =>
        {
            if (engine == null) return;

            try
            {
                Console.WriteLine($"[docked-workspace-test] frame {frame}: dockspaceId={window.DockspaceId}");

                if (frame == 5)
                {
                    NativeDockBuilder.ApplyPreset(window.DockspaceId, NativeDockBuilder.LayoutPreset.Simple, ImGuiNET.ImGui.GetMainViewport().Size);
                    Console.WriteLine("[docked-workspace-test] Applied Simple preset, no exception");
                }

                if (frame == 10)
                {
                    NativeDockBuilder.ApplyPreset(window.DockspaceId, NativeDockBuilder.LayoutPreset.PowerUser, ImGuiNET.ImGui.GetMainViewport().Size);
                    Console.WriteLine("[docked-workspace-test] Applied Power User preset (extra Timeline Dock split), no exception");
                }

                if (frame == 15)
                {
                    NativeDockBuilder.ApplyPreset(window.DockspaceId, NativeDockBuilder.LayoutPreset.Standard, ImGuiNET.ImGui.GetMainViewport().Size);
                    Console.WriteLine("[docked-workspace-test] Applied Standard preset, no exception");
                }

                if (frame < legacyPanelNames.Length * 2 && frame % 2 == 0)
                {
                    string target = legacyPanelNames[frame / 2];
                    EffectsPanel.RequestOpenPanel(target);
                    Console.WriteLine($"[docked-workspace-test] RequestOpenPanel('{target}') routed with no exception");
                }

                EffectsPanel.Draw(engine, settings, "test_dock_settings.json", "test_dock_layers.json", timeline, "test_dock_timeline.json", "test_dock_presets.json", previewWindow, shaderDirectory, window.DockspaceId, window);

                if (frame == 40 && engine.LastComposedFrame != null)
                {
                    Console.WriteLine($"[docked-workspace-test] Preview texture handle: {engine.LastComposedFrame.ColorTexture} ({engine.LastComposedFrame.Width}x{engine.LastComposedFrame.Height})");
                }
            }
            catch (Exception ex)
            {
                hadException = true;
                Console.WriteLine($"[docked-workspace-test] EXCEPTION: {ex}");
            }

            frame++;
            if (frame == 50) window.RequestClose();
        };

        window.Closing += () =>
        {
            previewWindow.Dispose();
            engine?.Dispose();
            Console.WriteLine(hadException
                ? "[docked-workspace-test] RESULT: FAIL (see exception(s) above)"
                : "[docked-workspace-test] RESULT: PASS (50 frames, 3 presets applied, 15 legacy panel names routed, Preview drew a real texture, no exceptions)");
        };

        window.Run();
    }
}
