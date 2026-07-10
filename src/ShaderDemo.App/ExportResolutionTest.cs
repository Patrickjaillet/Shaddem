// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Export;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.App;

public static class ExportResolutionTest
{
    public static void Run(string shaderDirectory, string imagePath)
    {
        using var window = new GlWindow(AppInfo.Name, 640, 360, fullscreen: false);
        ShaderManager? engine = null;
        int frame = 0;
        bool hadException = false;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            int loaded = ShaderLoader.LoadFromDirectory(engine, shaderDirectory, Console.WriteLine);
            Console.WriteLine($"[export-resolution-test] Loaded {loaded} shader(s)");

            if (engine.ShaderNames.Count > 0)
            {
                engine.SelectShader(0);
            }

            Layer imageLayer = Layer.CreateImage(imagePath, BlendMode.Normal, 1.0f);
            imageLayer.FitMode = ImageFitMode.Fit;
            engine.Layers.Add(imageLayer);
        };

        window.RenderFrame += _ =>
        {
            if (engine == null) return;

            try
            {
                engine.RenderFrame();

                if (frame == 5)
                {
                    Console.WriteLine($"[export-resolution-test] Before resize: Pipeline={engine.Pipeline.Width}x{engine.Pipeline.Height}, LayerFbo={engine.Pipeline.LayerFbo.Width}x{engine.Pipeline.LayerFbo.Height}");
                    string pathA = ScreenshotService.Save(engine.LastComposedFrame ?? engine.Pipeline.SceneFbo, "screenshots");
                    Console.WriteLine($"[export-resolution-test] Screenshot A (640x360) saved: {pathA}");

                    if (engine.Pipeline.Width != 640 || engine.Pipeline.Height != 360)
                    {
                        throw new InvalidOperationException($"expected pipeline 640x360 before resize, got {engine.Pipeline.Width}x{engine.Pipeline.Height}");
                    }

                    engine.Resize(1920, 1080);
                    Console.WriteLine($"[export-resolution-test] Called engine.Resize(1920, 1080)");
                }

                if (frame == 10)
                {
                    Console.WriteLine($"[export-resolution-test] After resize: Pipeline={engine.Pipeline.Width}x{engine.Pipeline.Height}, LayerFbo={engine.Pipeline.LayerFbo.Width}x{engine.Pipeline.LayerFbo.Height}");

                    if (engine.Pipeline.Width != 1920 || engine.Pipeline.Height != 1080)
                    {
                        throw new InvalidOperationException($"expected pipeline 1920x1080 after resize, got {engine.Pipeline.Width}x{engine.Pipeline.Height}");
                    }

                    if (engine.Pipeline.LayerFbo.Width != 1920 || engine.Pipeline.LayerFbo.Height != 1080)
                    {
                        throw new InvalidOperationException($"expected LayerFbo 1920x1080 after resize (fit-mode math reads this live), got {engine.Pipeline.LayerFbo.Width}x{engine.Pipeline.LayerFbo.Height}");
                    }

                    string pathB = ScreenshotService.Save(engine.LastComposedFrame ?? engine.Pipeline.SceneFbo, "screenshots");
                    Console.WriteLine($"[export-resolution-test] Screenshot B (1920x1080) saved: {pathB}");

                    if (engine.LastComposedFrame == null || engine.LastComposedFrame.Width != 1920 || engine.LastComposedFrame.Height != 1080)
                    {
                        throw new InvalidOperationException("expected LastComposedFrame to reflect the new export resolution");
                    }
                }
            }
            catch (Exception ex)
            {
                hadException = true;
                Console.WriteLine($"[export-resolution-test] EXCEPTION: {ex}");
            }

            frame++;
            if (frame == 15)
            {
                Console.WriteLine(hadException ? "[export-resolution-test] RESULT: FAIL (exception thrown)" : "[export-resolution-test] RESULT: PASS");
                window.RequestClose();
            }
        };

        window.Closing += () => engine?.Dispose();
        window.Run();
    }
}
