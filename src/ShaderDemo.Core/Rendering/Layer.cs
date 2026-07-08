// Copyright (c) 2026 Patrick JAILLET
namespace ShaderDemo.Core.Rendering;

public sealed class Layer
{
    public string ShaderName { get; set; }
    public BlendMode BlendMode { get; set; }
    public float Opacity { get; set; }
    public bool Enabled { get; set; }

    public Layer(string shaderName, BlendMode blendMode = BlendMode.Replace, float opacity = 1.0f, bool enabled = true)
    {
        ShaderName = shaderName;
        BlendMode = blendMode;
        Opacity = opacity;
        Enabled = enabled;
    }
}
