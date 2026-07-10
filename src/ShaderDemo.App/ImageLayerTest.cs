// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Export;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.App;

public static class ImageLayerTest
{
    public static void Run(string shaderDirectory, string imagePath)
    {
        using var window = new GlWindow(AppInfo.Name, 1280, 720, fullscreen: false);
        ShaderManager? engine = null;
        int frame = 0;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            int loaded = ShaderLoader.LoadFromDirectory(engine, shaderDirectory, Console.WriteLine);
            Console.WriteLine($"[image-layer-test] Loaded {loaded} shader(s)");

            if (engine.ShaderNames.Count > 0)
            {
                engine.SelectShader(0);
            }

            Layer imageLayer = Layer.CreateImage(imagePath, BlendMode.Normal, 0.75f);
            imageLayer.FitMode = ImageFitMode.Fit;
            imageLayer.Scale = 0.8f;
            engine.Layers.Add(imageLayer);
            Console.WriteLine($"[image-layer-test] Added image layer: {imagePath}");
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
                    Console.WriteLine($"[image-layer-test] Screenshot saved: {path}");
                }

                window.RequestClose();
            }
        };

        window.Closing += () => engine?.Dispose();
        window.Run();
    }
}
