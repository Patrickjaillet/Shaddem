// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Export;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.App;

public static class ShaderCompileTest
{
    public static void Run(string shaderName)
    {
        using var window = new GlWindow(AppInfo.Name, 640, 360, fullscreen: false);
        ShaderManager? engine = null;
        int frame = 0;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            ShaderLoader.LoadFromDirectory(engine, Path.Combine(AppContext.BaseDirectory, "shaders"), Console.WriteLine);
            int idx = engine.ShaderNames.ToList().IndexOf(shaderName);
            if (idx < 0)
            {
                Console.WriteLine($"[shader-test] '{shaderName}' not found or failed to compile");
            }
            else
            {
                engine.SelectShader(idx);
                Console.WriteLine($"[shader-test] Selected '{shaderName}' at index {idx}");
            }
        };

        window.RenderFrame += _ =>
        {
            engine?.RenderFrame();
            frame++;
            if (frame == 30)
            {
                if (engine != null)
                {
                    string path = ScreenshotService.Save(engine.LastComposedFrame ?? engine.Pipeline.SceneFbo, "screenshots");
                    Console.WriteLine($"[shader-test] Screenshot saved: {path}");
                }

                window.RequestClose();
            }
        };

        window.Closing += () => engine?.Dispose();
        window.Run();
    }
}
