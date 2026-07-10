// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.App;

public static class BenchmarkTest
{
    public static void Run(string shaderDirectory, double durationSeconds)
    {
        using var window = new GlWindow(AppInfo.Name, 1920, 1080, fullscreen: false, enableImGui: false);
        ShaderManager? engine = null;
        var frameTimesMs = new List<double>();
        double elapsed = 0.0;
        double shaderSwitchTimer = 0.0;
        int shaderCycleIndex = 0;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            int loaded = ShaderLoader.LoadFromDirectory(engine, shaderDirectory, Console.WriteLine);
            Console.WriteLine($"[benchmark] Loaded {loaded} shader(s), running for {durationSeconds:F0}s at 1920x1080");
            engine.Profiler.Enabled = true;

            if (loaded > 0) engine.SelectShader(0);

            engine.Effects.ParticlesActive = true;
            engine.Effects.ParticlesCount = 2000;
            if (engine.CurrentShaderName != null)
            {
                engine.Layers.Add(new Layer(engine.CurrentShaderName, BlendMode.Normal, 0.5f));
            }
        };

        window.UpdateFrame += dt => engine?.Update(dt);
        window.RenderFrame += deltaSeconds =>
        {
            if (engine == null) return;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            engine.RenderFrame();
            sw.Stop();
            frameTimesMs.Add(sw.Elapsed.TotalMilliseconds);

            elapsed += deltaSeconds;
            shaderSwitchTimer += deltaSeconds;

            if (shaderSwitchTimer > 1.0 && engine.ShaderNames.Count > 0)
            {
                shaderSwitchTimer = 0.0;
                shaderCycleIndex = (shaderCycleIndex + 1) % engine.ShaderNames.Count;
                engine.SelectShader(shaderCycleIndex);
            }

            if (elapsed >= durationSeconds)
            {
                window.RequestClose();
            }
        };

        window.Closing += () => engine?.Dispose();
        window.Run();

        if (frameTimesMs.Count == 0)
        {
            Console.WriteLine("[benchmark] No frames recorded.");
            return;
        }

        frameTimesMs.Sort();
        double min = frameTimesMs[0];
        double max = frameTimesMs[^1];
        double avg = frameTimesMs.Average();
        double p99 = frameTimesMs[(int)Math.Min(frameTimesMs.Count - 1, Math.Ceiling(frameTimesMs.Count * 0.99) - 1)];
        double avgFps = avg > 0 ? 1000.0 / avg : 0.0;

        Console.WriteLine($"[benchmark] Frames: {frameTimesMs.Count}");
        Console.WriteLine($"[benchmark] Min frame time: {min:F3} ms");
        Console.WriteLine($"[benchmark] Avg frame time: {avg:F3} ms ({avgFps:F1} fps avg)");
        Console.WriteLine($"[benchmark] P99 frame time: {p99:F3} ms");
        Console.WriteLine($"[benchmark] Max frame time: {max:F3} ms");
        Console.WriteLine($"[benchmark] Budget target: {PerformanceBudget.FrameTimeBudgetMs:F2} ms ({PerformanceBudget.BaselineWidth}x{PerformanceBudget.BaselineHeight}@{PerformanceBudget.BaselineFps}fps baseline)");
        Console.WriteLine($"[benchmark] Avg within budget: {PerformanceBudget.IsWithinBudget(avg)}");
        Console.WriteLine("[benchmark] Done.");
    }
}
