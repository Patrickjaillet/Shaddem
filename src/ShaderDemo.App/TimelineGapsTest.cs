// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Export;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.App;

public static class TimelineGapsTest
{
    public static void Run(string shaderDirectory, string modelPath, string audioPath)
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
            Console.WriteLine($"[timeline-gaps-test] Loaded {loaded} shader(s)");

            if (engine.ShaderNames.Count > 0)
            {
                engine.SelectShader(0);
            }

            engine.Timeline = timeline;

            timeline.Add(0.0, 10.0, ClipType.Model3D, modelPath, new Dictionary<string, object?>
            {
                ["position_z"] = -3.0,
                ["auto_rotate_y"] = 1.0,
            });

            timeline.Add(0.0, 10.0, ClipType.Music, audioPath, new Dictionary<string, object?>
            {
                ["volume"] = 0.5,
            });

            Layer manualLayer = Layer.CreateImage(Path.Combine(AppContext.BaseDirectory, "assets", "screenshot-workspace.png"), BlendMode.Normal, 0.0f);
            manualLayer.Name = "logo";
            engine.Layers.Add(manualLayer);

            timeline.Add(0.0, 10.0, ClipType.LayerAutomation, "logo", new Dictionary<string, object?>
            {
                ["property"] = "opacity",
                ["start_value"] = 0.0,
                ["end_value"] = 1.0,
            });

            Console.WriteLine("[timeline-gaps-test] Added Model3D, Music and LayerAutomation clips");
        };

        window.RenderFrame += _ =>
        {
            if (engine == null) return;

            try
            {
                float simulatedTime = frame switch
                {
                    < 5 => 0.5f,
                    < 15 => 5.0f,
                    _ => 12.0f,
                };
                engine.SetElapsedTime(simulatedTime);
                timeline.ApplyModelLayers(engine, engine.ElapsedTime);
                timeline.ApplyMusicClip(engine, engine.ElapsedTime);
                timeline.ApplyLayerAutomation(engine, engine.ElapsedTime);
                engine.RenderFrame();

                int modelLayers = engine.Layers.Count(l => l.SourceType == LayerSourceType.Model3D);
                Layer? logoLayer = engine.Layers.FirstOrDefault(l => l.Name == "logo");
                bool clipActive = simulatedTime < 10.0f;

                if (frame == 3)
                {
                    Console.WriteLine($"[timeline-gaps-test] frame {frame} t={simulatedTime}: modelLayers={modelLayers}, logo.Opacity={logoLayer?.Opacity:F3}, Player.IsPlaying={engine.Player.IsPlaying}, Player.CurrentFilePath={engine.Player.CurrentFilePath}");
                    if (modelLayers != 1) throw new InvalidOperationException($"expected 1 model layer, got {modelLayers}");
                    if (!engine.Player.IsPlaying) throw new InvalidOperationException("expected music to be playing while Music clip active");
                    if (logoLayer == null) throw new InvalidOperationException("logo layer missing");
                }

                if (frame == 12)
                {
                    Console.WriteLine($"[timeline-gaps-test] frame {frame} t={simulatedTime}: logo.Opacity={logoLayer?.Opacity:F3}");
                    if (logoLayer == null || logoLayer.Opacity < 0.4f)
                    {
                        throw new InvalidOperationException($"expected mid-clip opacity ~0.5, got {logoLayer?.Opacity}");
                    }

                    string path = ScreenshotService.Save(engine.LastComposedFrame ?? engine.Pipeline.SceneFbo, "screenshots");
                    Console.WriteLine($"[timeline-gaps-test] Mid-clip screenshot saved: {path}");
                }

                if (!clipActive && frame == 20)
                {
                    Console.WriteLine($"[timeline-gaps-test] frame {frame} t={simulatedTime}: modelLayers={modelLayers}, Player.IsPlaying={engine.Player.IsPlaying}");
                    if (modelLayers != 0) throw new InvalidOperationException($"expected model layer removed after clip end, got {modelLayers}");
                    if (engine.Player.IsPlaying) throw new InvalidOperationException("expected music stopped after clip end");
                }
            }
            catch (Exception ex)
            {
                hadException = true;
                Console.WriteLine($"[timeline-gaps-test] EXCEPTION: {ex}");
            }

            frame++;
            if (frame == 25)
            {
                Console.WriteLine(hadException ? "[timeline-gaps-test] RESULT: FAIL (exception thrown)" : "[timeline-gaps-test] RESULT: PASS");
                window.RequestClose();
            }
        };

        window.Closing += () => engine?.Dispose();
        window.Run();
    }
}
