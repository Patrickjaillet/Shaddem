// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Export;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.App;

public static class MegademoShowcaseTest
{
    public static void Run(string shaderDirectory, string imagePath, string modelPath)
    {
        using var window = new GlWindow(AppInfo.Name, 1280, 720, fullscreen: false);
        ShaderManager? engine = null;
        var timeline = new TimelineEngine { Active = true };
        int frame = 0;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            int loaded = ShaderLoader.LoadFromDirectory(engine, shaderDirectory, Console.WriteLine);
            Console.WriteLine($"[megademo-showcase] Loaded {loaded} shader(s)");

            if (engine.ShaderNames.Count > 0)
            {
                engine.SelectShader(0);
            }

            engine.Timeline = timeline;

            timeline.Add(0.0, 8.0, ClipType.Image, imagePath, new Dictionary<string, object?>
            {
                ["opacity"] = 0.7,
                ["fade_in"] = 1.0,
                ["fade_out"] = 1.0,
                ["scale"] = 0.65,
                ["position_y"] = -0.15,
                ["blend_mode"] = "Screen",
                ["fit_mode"] = "Fit",
            });

            timeline.Add(1.0, 6.0, ClipType.Text, "Megademo Showcase", new Dictionary<string, object?>
            {
                ["size"] = 70.0,
                ["color"] = new double[] { 255, 255, 255 },
                ["position"] = "center",
                ["fade_in"] = 0.5,
                ["fade_out"] = 0.5,
            });

            timeline.Add(0.0, 8.0, ClipType.Model3D, modelPath, new Dictionary<string, object?>
            {
                ["position_x"] = 2.2,
                ["position_z"] = -4.0,
                ["auto_rotate_y"] = 1.2,
                ["scale"] = 0.7,
            });

            Console.WriteLine("[megademo-showcase] Timeline built: image clip (0-8s, fading), text clip (1-7s), model clip (0-8s)");
        };

        window.RenderFrame += _ =>
        {
            if (engine == null) return;

            float simulatedTime = 4.0f;
            engine.SetElapsedTime(simulatedTime);
            timeline.ApplyEffects(engine.Effects, engine.ElapsedTime);
            timeline.ApplyShader(engine, engine.ElapsedTime);
            timeline.ApplyImageLayers(engine, engine.ElapsedTime);
            timeline.ApplyModelLayers(engine, engine.ElapsedTime);
            timeline.ApplyLayerAutomation(engine, engine.ElapsedTime);

            engine.RenderFrame();

            frame++;
            if (frame == 20)
            {
                string path = ScreenshotService.Save(engine.LastComposedFrame ?? engine.Pipeline.SceneFbo, "screenshots");
                Console.WriteLine($"[megademo-showcase] Hero screenshot saved: {path}");
                Console.WriteLine($"[megademo-showcase] Active layers at t={simulatedTime}: {string.Join(", ", engine.Layers.Select(l => $"{l.SourceType}(opacity={l.Opacity:F2})"))}");
            }

            if (frame == 25)
            {
                window.RequestClose();
            }
        };

        window.Closing += () => engine?.Dispose();
        window.Run();
    }
}
