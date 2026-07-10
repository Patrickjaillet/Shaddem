// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Export;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.App;

public static class TimelineImageLayerTest
{
    public static void Run(string shaderDirectory, string imagePath)
    {
        using var window = new GlWindow(AppInfo.Name, 1280, 720, fullscreen: false);
        ShaderManager? engine = null;
        var timeline = new TimelineEngine { Active = true };
        int frame = 0;
        bool hadException = false;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            int loaded = ShaderLoader.LoadFromDirectory(engine, shaderDirectory, Console.WriteLine);
            Console.WriteLine($"[timeline-image-test] Loaded {loaded} shader(s)");

            if (engine.ShaderNames.Count > 0)
            {
                engine.SelectShader(0);
            }

            timeline.Add(0.0, 10.0, ClipType.Image, imagePath, new Dictionary<string, object?>
            {
                ["opacity"] = 0.8,
                ["fade_in"] = 0.5,
                ["fade_out"] = 0.5,
                ["scale"] = 0.7,
            });
        };

        window.RenderFrame += _ =>
        {
            if (engine == null) return;

            try
            {
                float simulatedTime = frame switch
                {
                    < 5 => 0.1f,
                    < 15 => 5.0f,
                    < 20 => 9.8f,
                    _ => 12.0f,
                };
                engine.SetElapsedTime(simulatedTime);
                timeline.ApplyImageLayers(engine, engine.ElapsedTime);

                int timelineLayerCount = engine.Layers.Count(l => l.IsTimelineManaged);
                bool shouldHaveLayer = simulatedTime < 10.0f;
                if (shouldHaveLayer != (timelineLayerCount == 1))
                {
                    throw new InvalidOperationException($"frame {frame} t={simulatedTime}: expected timeline layer present={shouldHaveLayer}, actual count={timelineLayerCount}");
                }

                if (shouldHaveLayer)
                {
                    Layer managed = engine.Layers.First(l => l.IsTimelineManaged);
                    Console.WriteLine($"[timeline-image-test] frame {frame} t={simulatedTime} opacity={managed.Opacity:F3}");
                }

                engine.RenderFrame();
            }
            catch (Exception ex)
            {
                hadException = true;
                Console.WriteLine($"[timeline-image-test] EXCEPTION: {ex}");
            }

            frame++;
            if (frame == 10 && engine != null)
            {
                string path = ScreenshotService.Save(engine.LastComposedFrame ?? engine.Pipeline.SceneFbo, "screenshots");
                Console.WriteLine($"[timeline-image-test] Mid-clip screenshot saved: {path}");
            }

            if (frame == 25)
            {
                Console.WriteLine(hadException ? "[timeline-image-test] RESULT: FAIL (exception thrown)" : "[timeline-image-test] RESULT: PASS");
                window.RequestClose();
            }
        };

        window.Closing += () => engine?.Dispose();
        window.Run();
    }
}
