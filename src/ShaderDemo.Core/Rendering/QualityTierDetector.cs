// Copyright (c) 2026 Patrick JAILLET
namespace ShaderDemo.Core.Rendering;

public static class QualityTierDetector
{
    private static readonly string[] LowTierRendererHints =
    {
        "Intel(R) HD Graphics",
        "Intel(R) UHD Graphics",
        "Intel(R) Iris",
        "Microsoft Basic Render Driver",
    };

    private const double LowTierFrameTimeMultiplier = 1.25;
    private const double HighTierFrameTimeMultiplier = 0.5;

    public static bool RendererStringSuggestsLowTier(string renderer)
    {
        foreach (string hint in LowTierRendererHints)
        {
            if (renderer.Contains(hint, StringComparison.OrdinalIgnoreCase)) return true;
        }

        return false;
    }

    public static QualityTier ClassifyFromGpuBenchmark(double measuredGpuFrameTimeMs, bool rendererSuggestsLowTier)
    {
        if (rendererSuggestsLowTier) return QualityTier.Low;

        if (measuredGpuFrameTimeMs > PerformanceBudget.FrameTimeBudgetMs * LowTierFrameTimeMultiplier) return QualityTier.Low;
        if (measuredGpuFrameTimeMs > PerformanceBudget.FrameTimeBudgetMs * HighTierFrameTimeMultiplier) return QualityTier.Medium;

        return QualityTier.High;
    }

    public static QualityTier ClassifyFromCpuFallback(double measuredCpuFrameTimeMs, bool rendererSuggestsLowTier)
    {
        if (rendererSuggestsLowTier) return QualityTier.Low;

        if (measuredCpuFrameTimeMs > PerformanceBudget.FrameTimeBudgetMs * LowTierFrameTimeMultiplier) return QualityTier.Low;

        return QualityTier.Medium;
    }

    public static void ApplyTierDefaults(ShaderManager manager, QualityTier tier)
    {
        if (tier == QualityTier.Low)
        {
            manager.AdaptiveResolution.Enabled = true;
            manager.AdaptiveResolution.MinScale = AdaptiveResolutionController.LowTierMinScale;
        }
        else
        {
            manager.AdaptiveResolution.Enabled = false;
            manager.AdaptiveResolution.MinScale = AdaptiveResolutionController.DefaultMinScale;
        }
    }
}
