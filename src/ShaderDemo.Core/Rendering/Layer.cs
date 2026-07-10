// Copyright (c) 2026 Patrick JAILLET
namespace ShaderDemo.Core.Rendering;

public sealed class Layer
{
    private static int _nextId = 1;

    public string Name { get; set; } = $"Layer{_nextId++}";
    public LayerSourceType SourceType { get; set; }
    public string ShaderName { get; set; }
    public string? ImagePath { get; set; }
    public BlendMode BlendMode { get; set; }
    public float Opacity { get; set; }
    public bool Enabled { get; set; }

    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float Scale { get; set; } = 1.0f;
    public float Rotation { get; set; }
    public ImageFitMode FitMode { get; set; } = ImageFitMode.Fit;
    public bool IsTimelineManaged { get; set; }
    public Model3DState ModelState { get; set; } = new();

    public Layer(string shaderName, BlendMode blendMode = BlendMode.Replace, float opacity = 1.0f, bool enabled = true)
    {
        SourceType = LayerSourceType.Shader;
        ShaderName = shaderName;
        BlendMode = blendMode;
        Opacity = opacity;
        Enabled = enabled;
    }

    public static Layer CreateImage(string imagePath, BlendMode blendMode = BlendMode.Normal, float opacity = 1.0f, bool enabled = true)
    {
        return new Layer("", blendMode, opacity, enabled)
        {
            SourceType = LayerSourceType.Image,
            ImagePath = imagePath,
        };
    }

    public static Layer CreateModel3D(string modelPath, string? texturePath = null, BlendMode blendMode = BlendMode.Normal, float opacity = 1.0f, bool enabled = true)
    {
        var layer = new Layer("", blendMode, opacity, enabled)
        {
            SourceType = LayerSourceType.Model3D,
        };
        layer.ModelState.CurrentModelFilename = modelPath;
        layer.ModelState.CurrentTextureFilename = texturePath;
        return layer;
    }

    public string DisplayName => SourceType switch
    {
        LayerSourceType.Image => string.IsNullOrEmpty(ImagePath) ? "(no image)" : System.IO.Path.GetFileName(ImagePath),
        LayerSourceType.Text => "(timeline caption)",
        LayerSourceType.Model3D => string.IsNullOrEmpty(ModelState.CurrentModelFilename) ? "(no model)" : System.IO.Path.GetFileName(ModelState.CurrentModelFilename),
        _ => ShaderName,
    };
}
