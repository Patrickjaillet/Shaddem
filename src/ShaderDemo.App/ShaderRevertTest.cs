// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.App;

public static class ShaderRevertTest
{
    public static void Run()
    {
        using var window = new GlWindow(AppInfo.Name, 320, 240, fullscreen: false);
        ShaderManager? engine = null;
        string tempPath = Path.Combine(Path.GetTempPath(), "shaderdemo_revert_test.glsl");

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);

            const string validSource = """
                void mainImage(out vec4 fragColor, in vec2 fragCoord) {
                    fragColor = vec4(1.0, 0.0, 0.0, 1.0);
                }
                """;
            File.WriteAllText(tempPath, validSource);

            bool loadedOk = ShaderLoader.LoadFile(engine, tempPath, Console.WriteLine);
            Console.WriteLine($"[shader-revert-test] Initial load succeeded: {loadedOk}");
            Console.WriteLine($"[shader-revert-test] Has error after valid load: {ShaderErrorLog.Errors.ContainsKey("shaderdemo_revert_test.glsl")}");
            Console.WriteLine($"[shader-revert-test] Has cached last-good source: {ShaderErrorLog.TryGetLastGoodSource("shaderdemo_revert_test.glsl", out _)}");

            const string brokenSource = """
                void mainImage(out vec4 fragColor, in vec2 fragCoord) {
                    fragColor = this is not valid glsl;
                }
                """;
            File.WriteAllText(tempPath, brokenSource);

            bool loadedBroken = ShaderLoader.LoadFile(engine, tempPath, Console.WriteLine);
            Console.WriteLine($"[shader-revert-test] Broken load succeeded (expect False): {loadedBroken}");
            Console.WriteLine($"[shader-revert-test] Has error after broken load (expect True): {ShaderErrorLog.Errors.ContainsKey("shaderdemo_revert_test.glsl")}");

            bool hasLastGood = ShaderErrorLog.TryGetLastGoodSource("shaderdemo_revert_test.glsl", out string lastGoodSource);
            Console.WriteLine($"[shader-revert-test] Still has cached last-good source after failure (expect True): {hasLastGood}");

            if (hasLastGood)
            {
                engine.RegisterShader("shaderdemo_revert_test.glsl", lastGoodSource);
                ShaderErrorLog.ClearError("shaderdemo_revert_test.glsl");
                Console.WriteLine($"[shader-revert-test] After revert, error cleared (expect True): {!ShaderErrorLog.Errors.ContainsKey("shaderdemo_revert_test.glsl")}");
                Console.WriteLine($"[shader-revert-test] After revert, shader still registered (expect True): {engine.ShaderNames.Contains("shaderdemo_revert_test.glsl")}");
            }

            File.Delete(tempPath);
            window.RequestClose();
        };

        window.Closing += () => engine?.Dispose();
        window.Run();
        Console.WriteLine("[shader-revert-test] Done.");
    }
}
