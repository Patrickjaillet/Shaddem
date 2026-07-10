// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.App;

public static class GCPressureTest
{
    private const int Frames = 300;

    private static readonly string[] EffectFields =
    {
        "shake", "strobe", "vignette", "noise", "scanlines", "fisheye", "rgb_split", "wave",
        "mirror", "rotation", "bloom", "glitch", "vortex", "glitch_hard", "motion_blur",
        "datamosh", "feedback_opacity", "feedback_scale", "particles_size", "pixelate",
    };

    public static void Run(string shaderDirectory)
    {
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        using var window = new GlWindow(AppInfo.Name, 640, 480, fullscreen: false, enableImGui: false);
        ShaderManager? engine = null;
        var timeline = new TimelineEngine();
        int frame = 0;
        bool started = false;
        long gen0Before = 0, gen1Before = 0, gen2Before = 0;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            ShaderLoader.LoadFromDirectory(engine, shaderDirectory, _ => { });
            if (engine.ShaderNames.Count > 0) engine.SelectShader(0);
            engine.Timeline = timeline;

            foreach (string field in EffectFields)
            {
                timeline.Add(0.0, 1000.0, ClipType.Effect, field, new Dictionary<string, object?>
                {
                    ["start_value"] = 0.0,
                    ["end_value"] = 1.0,
                });
            }

            timeline.Add(0.0, 1000.0, ClipType.Shader, engine.ShaderNames[0]);
            timeline.Active = true;

            Console.WriteLine($"[gc-pressure-test] {Frames} frames, {EffectFields.Length} effect clips + 1 shader clip active (consolidated ApplyAll + cached reflection).");
        };

        window.RenderFrame += _ =>
        {
            if (engine == null) return;

            if (!started && frame == 50)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                gen0Before = GC.CollectionCount(0);
                gen1Before = GC.CollectionCount(1);
                gen2Before = GC.CollectionCount(2);
                started = true;
            }

            engine.SetElapsedTime(1.0f);
            timeline.ApplyAll(engine, engine.Effects, engine.ElapsedTime);

            frame++;
            if (frame == 50 + Frames)
            {
                long gen0After = GC.CollectionCount(0);
                long gen1After = GC.CollectionCount(1);
                long gen2After = GC.CollectionCount(2);
                Console.WriteLine($"[gc-pressure-test] Gen0={gen0After - gen0Before}, Gen1={gen1After - gen1Before}, Gen2={gen2After - gen2Before} over {Frames} frames");
                Console.WriteLine("[gc-pressure-test] RESULT: DONE");
                window.RequestClose();
            }
        };

        window.Closing += () => engine?.Dispose();
        window.Run();
    }
}
