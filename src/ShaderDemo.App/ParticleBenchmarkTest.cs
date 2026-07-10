// Copyright (c) 2026 Patrick JAILLET
using System.Diagnostics;
using ShaderDemo.Core;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.App;

public static class ParticleBenchmarkTest
{
    private static readonly int[] CountsToTest = { 2000, 5000, 10000, 20000, 50000, 100000, 200000 };

    public static void Run()
    {
        using var window = new GlWindow(AppInfo.Name, 320, 240, fullscreen: false, enableImGui: false);
        ParticleSystem? particles = null;
        int countIndex = 0;
        int warmupFrames = 0;
        var timings = new List<double>();
        const int samplesPerCount = 30;

        window.Load += () =>
        {
            particles = new ParticleSystem(window.Api!);
            particles.Resize(CountsToTest[0]);
            Console.WriteLine("[particle-benchmark] Measuring ParticleSystem.Update() CPU cost per particle count (30 samples each, after 5-frame warmup).");
        };

        window.RenderFrame += _ =>
        {
            if (particles == null) return;

            if (warmupFrames < 5)
            {
                particles.Update(0.016f, 0.0f);
                warmupFrames++;
                return;
            }

            var sw = Stopwatch.StartNew();
            particles.Update(0.016f, 0.5f);
            sw.Stop();
            timings.Add(sw.Elapsed.TotalMilliseconds);

            particles.Render(0.0f, 320.0f / 240.0f, 4.0f, new System.Numerics.Vector4(1, 1, 1, 1));

            if (timings.Count >= samplesPerCount)
            {
                double avg = timings.Average();
                double max = timings.Max();
                double perFrameBudgetFraction = (avg / PerformanceBudget.FrameTimeBudgetMs) * 100.0;
                Console.WriteLine($"[particle-benchmark] {CountsToTest[countIndex],7} particles: avg={avg:F3} ms, max={max:F3} ms ({perFrameBudgetFraction:F1}% of {PerformanceBudget.FrameTimeBudgetMs:F1}ms frame budget)");

                timings.Clear();
                warmupFrames = 0;
                countIndex++;

                if (countIndex >= CountsToTest.Length)
                {
                    Console.WriteLine("[particle-benchmark] Done.");
                    window.RequestClose();
                    return;
                }

                particles.Resize(CountsToTest[countIndex]);
            }
        };

        window.Closing += () => particles?.Dispose();
        window.Run();
    }
}
