// Copyright (c) 2026 Patrick JAILLET
namespace ShaderDemo.Core.Rendering;

public sealed class DemoTemplate
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string ShaderName { get; init; }
    public required Action<EffectParams> Configure { get; init; }
    public bool EnableParticles { get; init; }
    public Func<IEnumerable<Layer>>? Layers { get; init; }
}
