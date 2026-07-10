// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.App;

public static class ShaderLodSurveyTest
{
    private const int WarmupFrames = 15;
    private const int SampleFrames = 15;

    public static void Run(string shaderDirectory)
    {
        using var window = new GlWindow(AppInfo.Name, PerformanceBudget.BaselineWidth, PerformanceBudget.BaselineHeight, fullscreen: false, enableImGui: false);
        ShaderManager? engine = null;
        int shaderIndex = 0;
        int frameInShader = 0;
        var samples = new List<double>();
        var results = new List<(string Name, double AvgMs)>();

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            int loaded = ShaderLoader.LoadFromDirectory(engine, shaderDirectory, null);
            Console.WriteLine($"[shader-lod-survey] Loaded {loaded} shader(s). Measuring steady-state frame time per shader ({WarmupFrames} warmup + {SampleFrames} sampled frames each, discarding the first-use driver-compile stall).");
            if (loaded > 0) engine.SelectShader(0);
        };

        window.RenderFrame += deltaSeconds =>
        {
            if (engine == null || engine.ShaderNames.Count == 0) return;

            engine.RenderFrame();
            frameInShader++;

            if (frameInShader > WarmupFrames)
            {
                samples.Add(deltaSeconds * 1000.0);
            }

            if (frameInShader >= WarmupFrames + SampleFrames)
            {
                results.Add((engine.ShaderNames[shaderIndex], samples.Average()));
                samples.Clear();
                frameInShader = 0;
                shaderIndex++;

                if (shaderIndex >= engine.ShaderNames.Count)
                {
                    window.RequestClose();
                    return;
                }

                engine.SelectShader(shaderIndex);
            }
        };

        window.Closing += () => engine?.Dispose();
        window.Run();

        var sorted = results.OrderByDescending(r => r.AvgMs).ToList();
        Console.WriteLine($"[shader-lod-survey] Measured {sorted.Count} shader(s). Slowest steady-state shaders (avg ms/frame, {PerformanceBudget.BaselineWidth}x{PerformanceBudget.BaselineHeight}):");
        foreach (var (name, avgMs) in sorted.Take(20))
        {
            double budgetFraction = avgMs / PerformanceBudget.FrameTimeBudgetMs * 100.0;
            Console.WriteLine($"[shader-lod-survey]   {name,-45} avg={avgMs:F3} ms ({budgetFraction:F0}% of {PerformanceBudget.FrameTimeBudgetMs:F1}ms budget)");
        }

        Console.WriteLine("[shader-lod-survey] Done.");
    }
}
