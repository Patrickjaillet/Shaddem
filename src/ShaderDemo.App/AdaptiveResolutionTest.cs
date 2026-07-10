// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.App;

public static class AdaptiveResolutionTest
{
    public static void Run(string shaderDirectory)
    {
        using var window = new GlWindow(AppInfo.Name, 640, 480, fullscreen: false);
        ShaderManager? engine = null;
        int frame = 0;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            ShaderLoader.LoadFromDirectory(engine, shaderDirectory, Console.WriteLine);
            engine.SelectShader(0);
            engine.AdaptiveResolution.Enabled = true;
            Console.WriteLine($"[adaptive-res-test] Initial scale: {engine.AdaptiveResolution.CurrentScale:F2}, Pipeline internal size: {engine.Pipeline.SceneFbo.Width}x{engine.Pipeline.SceneFbo.Height}");
        };

        window.RenderFrame += _ =>
        {
            if (engine == null) return;

            if (frame == 20)
            {
                Console.WriteLine($"[adaptive-res-test] After 20 real frames (machine is fast, no auto-degrade expected): scale={engine.AdaptiveResolution.CurrentScale:F2}, internal size={engine.Pipeline.SceneFbo.Width}x{engine.Pipeline.SceneFbo.Height}");
                engine.AdaptiveResolution.Enabled = false;
                Console.WriteLine("[adaptive-res-test] Now forcing manual scale changes to verify FBO resize + present don't crash...");
                engine.Pipeline.SetRenderScale(0.5f);
            }

            if (frame == 30)
            {
                Console.WriteLine($"[adaptive-res-test] After SetRenderScale(0.5): internal size={engine.Pipeline.SceneFbo.Width}x{engine.Pipeline.SceneFbo.Height} (expect 320x240)");
                engine.Pipeline.SetRenderScale(1.0f);
            }

            engine.RenderFrame();
            frame++;

            if (frame == 40)
            {
                Console.WriteLine($"[adaptive-res-test] After SetRenderScale(1.0): internal size={engine.Pipeline.SceneFbo.Width}x{engine.Pipeline.SceneFbo.Height} (expect 640x480), no exceptions across the whole run.");
                window.RequestClose();
            }
        };

        window.Closing += () => engine?.Dispose();
        window.Run();

        Console.WriteLine("[adaptive-res-test] Done.");
    }
}
