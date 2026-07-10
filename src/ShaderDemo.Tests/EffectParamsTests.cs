// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Tests;

public class EffectParamsTests
{
    [Fact]
    public void ClampToValidRanges_ClampsOutOfRangeFieldsIntoBounds()
    {
        var effects = new EffectParams
        {
            Intensity = -10.0f,
            Bloom = 999.0f,
            Gamma = 0.0f,
            RotationSpeed = 999.0f,
            Pixelate = 0.0f,
            ParticlesCount = -5,
            FeedbackScale = 10.0f,
        };

        effects.ClampToValidRanges();

        Assert.InRange(effects.Intensity, 0.0f, 5.0f);
        Assert.InRange(effects.Bloom, 0.0f, 5.0f);
        Assert.InRange(effects.Gamma, 0.1f, 3.0f);
        Assert.InRange(effects.RotationSpeed, -180.0f, 180.0f);
        Assert.InRange(effects.Pixelate, 1.0f, 50.0f);
        Assert.InRange(effects.ParticlesCount, 100, 10000);
        Assert.InRange(effects.FeedbackScale, 0.5f, 1.5f);
    }

    [Fact]
    public void ClampToValidRanges_LeavesInRangeValuesUnchanged()
    {
        var effects = new EffectParams { Intensity = 2.0f, Bloom = 1.0f, Speed = 1.5f };

        effects.ClampToValidRanges();

        Assert.Equal(2.0f, effects.Intensity);
        Assert.Equal(1.0f, effects.Bloom);
        Assert.Equal(1.5f, effects.Speed);
    }

    [Fact]
    public void CopyFrom_CopiesAllScalarFields()
    {
        var source = new EffectParams { Intensity = 3.3f, Bloom = 2.2f, Shake = 1.1f };
        var target = new EffectParams();

        target.CopyFrom(source);

        Assert.Equal(3.3f, target.Intensity);
        Assert.Equal(2.2f, target.Bloom);
        Assert.Equal(1.1f, target.Shake);
    }
}
