// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Export;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.App;

public static class TextModelLayerTest
{
    public static void Run(string shaderDirectory, string modelPath)
    {
        using var window = new GlWindow(AppInfo.Name, 1280, 720, fullscreen: false);
        ShaderManager? engine = null;
        var timeline = new TimelineEngine();
        int frame = 0;
        bool hadException = false;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            int loaded = ShaderLoader.LoadFromDirectory(engine, shaderDirectory, Console.WriteLine);
            Console.WriteLine($"[text-model-layer-test] Loaded {loaded} shader(s)");

            if (engine.ShaderNames.Count > 0)
            {
                engine.SelectShader(0);
            }

            engine.Timeline = timeline;
            timeline.Add(0.0, 10.0, ClipType.Text, "Megademo", new Dictionary<string, object?>
            {
                ["size"] = 80.0,
                ["color"] = new double[] { 255, 220, 80 },
                ["position"] = "center",
            });

            Layer modelLayer = Layer.CreateModel3D(modelPath, null, BlendMode.Normal, 1.0f);
            modelLayer.ModelState.Position = new System.Numerics.Vector3(0.0f, 0.0f, -3.0f);
            modelLayer.ModelState.AutoRotateSpeed = new System.Numerics.Vector3(0.0f, 1.0f, 0.0f);
            engine.Layers.Add(modelLayer);
            Console.WriteLine($"[text-model-layer-test] Added model layer: {modelPath}");
        };

        window.RenderFrame += _ =>
        {
            if (engine == null) return;

            try
            {
                float simulatedTime = frame < 20 ? 2.0f : 12.0f;
                engine.SetElapsedTime(simulatedTime);
                engine.RenderFrame();

                int textLayers = engine.Layers.Count(l => l.SourceType == LayerSourceType.Text);
                int modelLayers = engine.Layers.Count(l => l.SourceType == LayerSourceType.Model3D);
                if (frame == 5)
                {
                    Console.WriteLine($"[text-model-layer-test] frame {frame}: text layers={textLayers}, model layers={modelLayers}");
                    if (textLayers != 1 || modelLayers != 1)
                    {
                        throw new InvalidOperationException($"expected 1 text + 1 model layer, got text={textLayers} model={modelLayers}");
                    }
                }
            }
            catch (Exception ex)
            {
                hadException = true;
                Console.WriteLine($"[text-model-layer-test] EXCEPTION: {ex}");
            }

            frame++;
            if (frame == 15)
            {
                if (engine != null)
                {
                    string path = ScreenshotService.Save(engine.LastComposedFrame ?? engine.Pipeline.SceneFbo, "screenshots");
                    Console.WriteLine($"[text-model-layer-test] Screenshot saved: {path}");
                }
            }

            if (frame == 25)
            {
                int textLayersAfterClipEnds = engine!.Layers.Count(l => l.SourceType == LayerSourceType.Text);
                Console.WriteLine($"[text-model-layer-test] frame {frame}: text layers after clip window={textLayersAfterClipEnds}");
                Console.WriteLine(hadException ? "[text-model-layer-test] RESULT: FAIL (exception thrown)" : "[text-model-layer-test] RESULT: PASS");
                window.RequestClose();
            }
        };

        window.Closing += () => engine?.Dispose();
        window.Run();
    }
}
