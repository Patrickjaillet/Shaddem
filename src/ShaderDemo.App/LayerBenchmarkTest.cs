// Copyright (c) 2026 Patrick JAILLET
using System.Diagnostics;
using ShaderDemo.Core;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.App;

public static class LayerBenchmarkTest
{
    private static readonly int[] LayerCountsToTest = { 1, 2, 4, 8, 16, 32 };

    public static void Run(string shaderDirectory, string imagePathOrDirectory)
    {
        string[] imagePaths = Directory.Exists(imagePathOrDirectory)
            ? Directory.GetFiles(imagePathOrDirectory, "*.png")
            : new[] { imagePathOrDirectory };

        using var window = new GlWindow(AppInfo.Name, PerformanceBudget.BaselineWidth / 2, PerformanceBudget.BaselineHeight / 2, fullscreen: false);
        ShaderManager? engine = null;
        int countIndex = 0;
        int warmupFrames = 0;
        var timings = new List<double>();
        const int samplesPerCount = 30;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            engine.Profiler.Enabled = true;
            ShaderLoader.LoadFromDirectory(engine, shaderDirectory, null);
            if (engine.ShaderNames.Count > 0) engine.SelectShader(0);

            Console.WriteLine($"[layer-benchmark] Measuring ShaderManager.RenderFrame() CPU cost, GPU 'Scene' section cost (RenderLayerChain + LayerFbo ping-pong), and VRAM as simultaneous image layers scale up (30 samples each, after 5-frame warmup), cycling across {imagePaths.Length} distinct source image(s).");
            AddLayersUpTo(engine, imagePaths, LayerCountsToTest[0]);
        };

        window.RenderFrame += _ =>
        {
            if (engine == null) return;

            if (warmupFrames < 20)
            {
                engine.RenderFrame();
                warmupFrames++;
                return;
            }

            var sw = Stopwatch.StartNew();
            engine.RenderFrame();
            sw.Stop();
            timings.Add(sw.Elapsed.TotalMilliseconds);

            if (timings.Count >= samplesPerCount)
            {
                double avg = timings.Average();
                double max = timings.Max();
                double perFrameBudgetFraction = (avg / PerformanceBudget.FrameTimeBudgetMs) * 100.0;
                double vramMb = GpuResourceTracker.EstimatedVramBytes / (1024.0 * 1024.0);
                double gpuSceneMs = engine.Profiler.GetTimingsMilliseconds().TryGetValue("Scene", out double sceneMs) ? sceneMs : 0.0;
                Console.WriteLine($"[layer-benchmark] {LayerCountsToTest[countIndex],3} layers: CPU avg={avg:F3} ms, max={max:F3} ms ({perFrameBudgetFraction:F1}% of {PerformanceBudget.FrameTimeBudgetMs:F1}ms budget), GPU 'Scene'={gpuSceneMs:F3} ms, estimated VRAM={vramMb:F1} MB");

                timings.Clear();
                warmupFrames = 0;
                countIndex++;

                if (countIndex >= LayerCountsToTest.Length)
                {
                    Console.WriteLine("[layer-benchmark] Done.");
                    window.RequestClose();
                    return;
                }

                AddLayersUpTo(engine, imagePaths, LayerCountsToTest[countIndex]);
            }
        };

        window.Closing += () => engine?.Dispose();
        window.Run();
    }

    private static void AddLayersUpTo(ShaderManager engine, string[] imagePaths, int targetCount)
    {
        while (engine.Layers.Count < targetCount)
        {
            string imagePath = imagePaths[engine.Layers.Count % imagePaths.Length];
            engine.Layers.Add(Layer.CreateImage(imagePath, BlendMode.Normal, 0.5f));
        }
    }
}
