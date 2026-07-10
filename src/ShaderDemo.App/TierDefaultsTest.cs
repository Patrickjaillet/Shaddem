// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.App;

public static class TierDefaultsTest
{
    public static void Run()
    {
        using var window = new GlWindow(AppInfo.Name, 320, 240, fullscreen: false);
        ShaderManager? engine = null;
        bool hadFailure = false;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);

            QualityTierDetector.ApplyTierDefaults(engine, QualityTier.Low);
            Console.WriteLine($"[tier-defaults-test] Low: Enabled={engine.AdaptiveResolution.Enabled}, MinScale={engine.AdaptiveResolution.MinScale}");
            if (!engine.AdaptiveResolution.Enabled || engine.AdaptiveResolution.MinScale != AdaptiveResolutionController.LowTierMinScale)
            {
                hadFailure = true;
                Console.WriteLine("[tier-defaults-test] FAIL: Low tier did not enable adaptive resolution with the lowered floor");
            }

            QualityTierDetector.ApplyTierDefaults(engine, QualityTier.Medium);
            Console.WriteLine($"[tier-defaults-test] Medium: Enabled={engine.AdaptiveResolution.Enabled}, MinScale={engine.AdaptiveResolution.MinScale}");
            if (engine.AdaptiveResolution.Enabled || engine.AdaptiveResolution.MinScale != AdaptiveResolutionController.DefaultMinScale)
            {
                hadFailure = true;
                Console.WriteLine("[tier-defaults-test] FAIL: Medium tier did not preserve the default (off, standard floor) behavior");
            }

            QualityTierDetector.ApplyTierDefaults(engine, QualityTier.High);
            Console.WriteLine($"[tier-defaults-test] High: Enabled={engine.AdaptiveResolution.Enabled}, MinScale={engine.AdaptiveResolution.MinScale}");
            if (engine.AdaptiveResolution.Enabled || engine.AdaptiveResolution.MinScale != AdaptiveResolutionController.DefaultMinScale)
            {
                hadFailure = true;
                Console.WriteLine("[tier-defaults-test] FAIL: High tier did not preserve the default (off, standard floor) behavior");
            }

            Console.WriteLine(hadFailure ? "[tier-defaults-test] RESULT: FAIL" : "[tier-defaults-test] RESULT: PASS");
            window.RequestClose();
        };

        window.Closing += () => engine?.Dispose();
        window.Run();
    }
}
