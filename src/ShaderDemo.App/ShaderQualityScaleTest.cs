// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.App;

public static class ShaderQualityScaleTest
{
    private const int WarmupFrames = 15;
    private const int SampleFrames = 15;

    public static void Run(string shaderDirectory, string shaderName)
    {
        using var window = new GlWindow(AppInfo.Name, PerformanceBudget.BaselineWidth, PerformanceBudget.BaselineHeight, fullscreen: false, enableImGui: false);
        ShaderManager? engine = null;
        int frame = 0;
        var fullQualitySamples = new List<double>();
        var scaledQualitySamples = new List<double>();
        bool scaledDown = false;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            ShaderLoader.LoadFromDirectory(engine, shaderDirectory, null);
            int idx = engine.ShaderNames.ToList().IndexOf(shaderName);
            if (idx < 0)
            {
                Console.WriteLine($"[shader-quality-scale-test] RESULT: FAIL ('{shaderName}' not found)");
                window.RequestClose();
                return;
            }

            engine.SelectShader(idx);
            Console.WriteLine($"[shader-quality-scale-test] Testing '{shaderName}' at CurrentScale={engine.AdaptiveResolution.CurrentScale:F2} (full quality) first, then forced down to the Low-tier floor.");
        };

        window.RenderFrame += deltaSeconds =>
        {
            if (engine == null || engine.ShaderNames.Count == 0) return;

            engine.RenderFrame();
            frame++;

            if (!scaledDown)
            {
                if (frame > WarmupFrames && frame <= WarmupFrames + SampleFrames)
                {
                    fullQualitySamples.Add(deltaSeconds * 1000.0);
                }

                if (frame > WarmupFrames + SampleFrames)
                {
                    engine.AdaptiveResolution.Enabled = true;
                    for (int i = 0; i < 10; i++)
                    {
                        engine.AdaptiveResolution.Update(1000.0, PerformanceBudget.FrameTimeBudgetMs);
                    }

                    Console.WriteLine($"[shader-quality-scale-test] Forced AdaptiveResolution.CurrentScale down to {engine.AdaptiveResolution.CurrentScale:F2}");
                    scaledDown = true;
                    frame = 0;
                }

                return;
            }

            if (frame > WarmupFrames && frame <= WarmupFrames + SampleFrames)
            {
                scaledQualitySamples.Add(deltaSeconds * 1000.0);
            }

            if (frame > WarmupFrames + SampleFrames)
            {
                window.RequestClose();
            }
        };

        window.Closing += () => engine?.Dispose();
        window.Run();

        if (fullQualitySamples.Count == 0 || scaledQualitySamples.Count == 0)
        {
            return;
        }

        double fullAvg = fullQualitySamples.Average();
        double scaledAvg = scaledQualitySamples.Average();
        double reductionPercent = fullAvg > 0 ? (1.0 - scaledAvg / fullAvg) * 100.0 : 0.0;

        Console.WriteLine($"[shader-quality-scale-test] Full quality (CurrentScale=1.0): avg={fullAvg:F3} ms");
        Console.WriteLine($"[shader-quality-scale-test] Scaled quality (CurrentScale={engine!.AdaptiveResolution.CurrentScale:F2}): avg={scaledAvg:F3} ms");
        Console.WriteLine($"[shader-quality-scale-test] Reduction: {reductionPercent:F1}%");
        Console.WriteLine(scaledAvg < fullAvg ? "[shader-quality-scale-test] RESULT: PASS (scaled render is measurably cheaper)" : "[shader-quality-scale-test] RESULT: FAIL (no measured improvement)");
    }
}
