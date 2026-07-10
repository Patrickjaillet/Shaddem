// Copyright (c) 2026 Patrick JAILLET
using System.Diagnostics;
using ShaderDemo.Core;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.App;

public static class ShaderCacheTest
{
    public static void RunSingleProcessScenario(string shaderDirectory, bool useCache, string cacheDir)
    {
        ShaderBinaryCache.CacheDirectory = cacheDir;
        ShaderManager.UseShaderBinaryCache = useCache;

        using var window = new GlWindow(AppInfo.Name, 320, 240, fullscreen: false);
        ShaderManager? engine = null;
        var sw = new Stopwatch();

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            sw.Start();
            int loaded = ShaderLoader.LoadFromDirectory(engine, shaderDirectory, null);
            sw.Stop();
            Console.WriteLine($"[shader-cache-test] Loaded {loaded} shaders in {sw.Elapsed.TotalMilliseconds:F1} ms (useCache={useCache}, cacheDir={cacheDir})");
        };

        window.RenderFrame += _ =>
        {
            engine?.RenderFrame();
            window.RequestClose();
        };

        window.Closing += () => engine?.Dispose();
        window.Run();
    }
}
