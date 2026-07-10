// Copyright (c) 2026 Patrick JAILLET
namespace ShaderDemo.Core.Rendering;

public sealed class AdaptiveResolutionController
{
    private const float MinScale = 0.5f;
    private const float MaxScale = 1.0f;
    private const float StepDown = 0.1f;
    private const float StepUp = 0.05f;
    private const double OverBudgetFactor = 1.15;
    private const double UnderBudgetFactor = 0.85;

    private double _smoothedFrameTimeMs = -1.0;

    public bool Enabled { get; set; }

    public float CurrentScale { get; private set; } = MaxScale;

    public bool Update(double frameTimeMs, double budgetMs)
    {
        if (!Enabled)
        {
            _smoothedFrameTimeMs = -1.0;
            if (CurrentScale != MaxScale)
            {
                CurrentScale = MaxScale;
                return true;
            }

            return false;
        }

        _smoothedFrameTimeMs = _smoothedFrameTimeMs < 0.0
            ? frameTimeMs
            : (_smoothedFrameTimeMs * 0.9) + (frameTimeMs * 0.1);

        float previousScale = CurrentScale;

        if (_smoothedFrameTimeMs > budgetMs * OverBudgetFactor)
        {
            CurrentScale = Math.Max(MinScale, CurrentScale - StepDown);
        }
        else if (_smoothedFrameTimeMs < budgetMs * UnderBudgetFactor)
        {
            CurrentScale = Math.Min(MaxScale, CurrentScale + StepUp);
        }

        return MathF.Abs(CurrentScale - previousScale) > 0.001f;
    }
}
