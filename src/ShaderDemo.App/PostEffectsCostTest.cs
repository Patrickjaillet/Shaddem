// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.App;

public static class PostEffectsCostTest
{
    private sealed record Scenario(string Name, float Bloom, float FeedbackOpacity);

    private static readonly Scenario[] Scenarios =
    {
        new("Bloom=0, Feedback=0", 0.0f, 0.0f),
        new("Bloom=0.6, Feedback=0", 0.6f, 0.0f),
        new("Bloom=0, Feedback=0.3", 0.0f, 0.3f),
        new("Bloom=0.6, Feedback=0.3", 0.6f, 0.3f),
    };

    public static void Run(string shaderDirectory, float renderScale)
    {
        using var window = new GlWindow(AppInfo.Name, PerformanceBudget.BaselineWidth, PerformanceBudget.BaselineHeight, fullscreen: false);
        ShaderManager? engine = null;
        int scenarioIndex = 0;
        int warmupFrames = 0;
        var timings = new List<double>();
        const int warmupCount = 20;
        const int sampleCount = 30;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            ShaderLoader.LoadFromDirectory(engine, shaderDirectory, null);
            int idx = engine.ShaderNames.ToList().IndexOf("Mandelbulb_I.glsl");
            engine.SelectShader(idx >= 0 ? idx : 0);
            engine.Pipeline.SetRenderScale(renderScale);
            engine.Effects.Bloom = Scenarios[0].Bloom;
            engine.Effects.FeedbackOpacity = Scenarios[0].FeedbackOpacity;
            Console.WriteLine($"[posteffects-cost-test] RenderScale={renderScale}, internal SceneFbo={engine.Pipeline.SceneFbo.Width}x{engine.Pipeline.SceneFbo.Height}, output={PerformanceBudget.BaselineWidth}x{PerformanceBudget.BaselineHeight}");
        };

        window.RenderFrame += deltaSeconds =>
        {
            if (engine == null) return;

            engine.RenderFrame();

            if (warmupFrames < warmupCount)
            {
                warmupFrames++;
                return;
            }

            timings.Add(deltaSeconds * 1000.0);

            if (timings.Count >= sampleCount)
            {
                double avg = timings.Average();
                Console.WriteLine($"[posteffects-cost-test] {Scenarios[scenarioIndex].Name}: avg={avg:F3} ms ({1000.0 / avg:F1} fps avg)");

                timings.Clear();
                warmupFrames = 0;
                scenarioIndex++;

                if (scenarioIndex >= Scenarios.Length)
                {
                    Console.WriteLine("[posteffects-cost-test] Done.");
                    window.RequestClose();
                    return;
                }

                engine.Effects.Bloom = Scenarios[scenarioIndex].Bloom;
                engine.Effects.FeedbackOpacity = Scenarios[scenarioIndex].FeedbackOpacity;
            }
        };

        window.Closing += () => engine?.Dispose();
        window.Run();
    }
}
