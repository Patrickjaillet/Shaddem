// Copyright (c) 2026 Patrick JAILLET
namespace ShaderDemo.Core.Rendering;

public static class PerformanceBudget
{
    public const int BaselineWidth = 1920;
    public const int BaselineHeight = 1080;
    public const int BaselineFps = 60;

    public const int HighEndWidth = 3840;
    public const int HighEndHeight = 2160;
    public const int HighEndFps = 60;

    public const double FrameTimeBudgetMs = 1000.0 / BaselineFps;

    public static bool IsWithinBudget(double frameTimeMs) => frameTimeMs <= FrameTimeBudgetMs;
}
