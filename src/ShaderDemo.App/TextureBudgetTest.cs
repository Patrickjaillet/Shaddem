// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.App;

public static class TextureBudgetTest
{
    public static void Run(string shaderDirectory, string largeImagePath)
    {
        using var window = new GlWindow(AppInfo.Name, 320, 240, fullscreen: false);
        ShaderManager? engine = null;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            ShaderLoader.LoadFromDirectory(engine, shaderDirectory, null);
            if (engine.ShaderNames.Count > 0) engine.SelectShader(0);

            Texture? unclamped = TextureLoader.Load(window.Api!, largeImagePath, null, maxDimension: 0);
            long unclampedBytes = unclamped == null ? 0 : (long)unclamped.Width * unclamped.Height * 3;
            Console.WriteLine($"[texture-budget-test] Unclamped load: {unclamped?.Width}x{unclamped?.Height} ({unclampedBytes / (1024.0 * 1024.0):F1} MB raw)");
            unclamped?.Dispose();

            engine.MaxLayerImageDimension = 1024;
            Layer clampedLayer = Layer.CreateImage(largeImagePath, BlendMode.Normal, 1.0f);
            engine.Layers.Add(clampedLayer);
        };

        int frame = 0;
        window.RenderFrame += _ =>
        {
            engine?.RenderFrame();
            frame++;
            if (frame == 3 && engine != null)
            {
                Texture? clamped = TextureLoader.Load(engine.Gl, largeImagePath, null, engine.MaxLayerImageDimension);
                if (clamped != null)
                {
                    long clampedBytes = (long)clamped.Width * clamped.Height * 3;
                    Console.WriteLine($"[texture-budget-test] Clamped (MaxLayerImageDimension=1024) load: {clamped.Width}x{clamped.Height} ({clampedBytes / (1024.0 * 1024.0):F1} MB raw)");

                    bool pass = clamped.Width <= 1024 && clamped.Height <= 1024;
                    clamped.Dispose();
                    Console.WriteLine(pass ? "[texture-budget-test] RESULT: PASS" : "[texture-budget-test] RESULT: FAIL (clamp did not apply)");
                }
                else
                {
                    Console.WriteLine("[texture-budget-test] RESULT: FAIL (layer texture not loaded)");
                }

                window.RequestClose();
            }
        };

        window.Closing += () => engine?.Dispose();
        window.Run();
    }
}
