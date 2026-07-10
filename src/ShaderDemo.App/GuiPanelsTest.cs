// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core;
using ShaderDemo.Core.Gui;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.App;

public static class GuiPanelsTest
{
    public static void Run(string shaderDirectory)
    {
        using var window = new GlWindow(AppInfo.Name, 900, 700, fullscreen: false);
        ShaderManager? engine = null;
        var timeline = new TimelineEngine();
        var settings = new AppSettings();
        int frame = 0;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            ShaderLoader.LoadFromDirectory(engine, shaderDirectory, Console.WriteLine);
            engine.SelectShader(0);
            engine.Profiler.Enabled = true;
            Console.WriteLine($"[gui-test] Loaded {engine.ShaderNames.Count} shaders, current='{engine.CurrentShaderName}'");
        };

        window.UpdateFrame += dt => engine?.Update(dt);
        window.RenderFrame += _ => engine?.RenderFrame();
        window.ImGuiRender += () =>
        {
            if (engine == null) return;

            try
            {
                ImGui.Begin("GuiPanelsTest");
                ImGui.Text($"Icons: {ShaderDemo.Core.Gui.Theme.Icons.Stop} {ShaderDemo.Core.Gui.Theme.Icons.Record} {ShaderDemo.Core.Gui.Theme.Icons.Up} {ShaderDemo.Core.Gui.Theme.Icons.Down} {ShaderDemo.Core.Gui.Theme.Icons.Close} {ShaderDemo.Core.Gui.Theme.Icons.Bullet}");

                GeneralPanel.Draw(engine, settings, timeline);
                SystemPanel.Draw(engine, settings, timeline, "test_settings.json", "test_layers.json", "test_timeline.json", window);
                WindowPanel.Draw(new SecondaryWindow(window.NativeWindow), engine);

                if (frame == 5)
                {
                    ImGui.OpenPopup("Create Shader");
                }

                PopupsPanel.Draw(engine, timeline, shaderDirectory);

                if (frame == 5)
                {
                    ImGui.OpenPopup("Manage Tracks");
                }

                if (frame == 20)
                {
                    timeline.Tracks.Add(new Track("TestAdd", new List<ClipType> { ClipType.Effect }, 30, new System.Numerics.Vector4(1, 0, 0, 1)));
                    Console.WriteLine($"[gui-test] Tracks after manual add: {timeline.Tracks.Count}");
                }

                if (frame == 10) ImGui.OpenPopup("Confirm New Project");
                if (frame == 15) ImGui.OpenPopup("Confirm Overwrite Project");

                MediaPanel.Draw(engine);
                LiveCodingPanel.Draw(engine, shaderDirectory);
                ExportPanel.Draw(engine, settings, timeline);
                TimelinePanel.Draw(timeline, engine, "test_timeline2.json");
                Model3DPanel.Draw(engine);
                DebugOverlayPanel.Draw(engine);

                ImGui.End();

                if (frame == 30)
                {
                    Console.WriteLine("[gui-test] 30 frames rendered with no exceptions from General/System/Popups/Media panels.");
                    Console.WriteLine($"[gui-test] Track count: {timeline.Tracks.Count}");
                }

                if (frame == 35)
                {
                    foreach ((string name, double ms) in engine.Profiler.GetTimingsMilliseconds())
                    {
                        Console.WriteLine($"[gui-test] GPU timer '{name}': {ms:F4} ms");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[gui-test] EXCEPTION: {ex}");
            }

            frame++;
            if (frame == 40) window.RequestClose();
        };

        window.Closing += () => engine?.Dispose();
        window.Run();
    }
}
