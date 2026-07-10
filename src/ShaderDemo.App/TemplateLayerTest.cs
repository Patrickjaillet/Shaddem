// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Export;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;

namespace ShaderDemo.App;

public static class TemplateLayerTest
{
    public static void Run(string shaderDirectory)
    {
        using var window = new GlWindow(AppInfo.Name, 1280, 720, fullscreen: false);
        ShaderManager? engine = null;
        int frame = 0;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            int loaded = ShaderLoader.LoadFromDirectory(engine, shaderDirectory, Console.WriteLine);
            Console.WriteLine($"[template-layer-test] Loaded {loaded} shader(s)");

            DemoTemplate? template = TemplatesService.BuiltIn.FirstOrDefault(t => t.Name == "Image Layer Showcase");
            if (template == null)
            {
                Console.WriteLine("[template-layer-test] RESULT: FAIL (template not found)");
                window.RequestClose();
                return;
            }

            bool applied = TemplatesService.Apply(template, engine);
            Console.WriteLine($"[template-layer-test] Applied '{template.Name}': {applied}");
            Console.WriteLine($"[template-layer-test] Layers after apply: {engine.Layers.Count} (image layers: {engine.Layers.Count(l => l.SourceType == LayerSourceType.Image)})");

            if (!applied || engine.Layers.Count(l => l.SourceType == LayerSourceType.Image) != 1)
            {
                Console.WriteLine("[template-layer-test] RESULT: FAIL");
                window.RequestClose();
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
                    Console.WriteLine($"[template-layer-test] Screenshot saved: {path}");
                    Console.WriteLine("[template-layer-test] RESULT: PASS");
                }

                window.RequestClose();
            }
        };

        window.Closing += () => engine?.Dispose();
        window.Run();
    }
}
