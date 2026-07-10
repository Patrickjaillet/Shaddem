// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using System.Text.Json;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Settings;

public static class LayerPersistence
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    private sealed class LayerData
    {
        public LayerSourceType SourceType { get; set; } = LayerSourceType.Shader;
        public string ShaderName { get; set; } = "";
        public string? ImagePath { get; set; }
        public BlendMode BlendMode { get; set; }
        public float Opacity { get; set; } = 1.0f;
        public bool Enabled { get; set; } = true;
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float Scale { get; set; } = 1.0f;
        public float Rotation { get; set; }
        public ImageFitMode FitMode { get; set; } = ImageFitMode.Fit;
        public string? ModelPath { get; set; }
        public string? ModelTexturePath { get; set; }
        public Vector3 ModelPosition { get; set; }
        public Vector3 ModelRotation { get; set; }
        public Vector3 ModelScale { get; set; } = Vector3.One;
        public Vector3 ModelAutoRotateSpeed { get; set; }
        public bool ModelWireframe { get; set; }
        public Vector3 ModelWireframeColor { get; set; } = Vector3.One;
    }

    public static void Save(IEnumerable<Layer> layers, string filePath)
    {
        var data = layers.Where(l => !l.IsTimelineManaged).Select(l => new LayerData
        {
            SourceType = l.SourceType,
            ShaderName = l.ShaderName,
            ImagePath = l.ImagePath,
            BlendMode = l.BlendMode,
            Opacity = l.Opacity,
            Enabled = l.Enabled,
            PositionX = l.PositionX,
            PositionY = l.PositionY,
            Scale = l.Scale,
            Rotation = l.Rotation,
            FitMode = l.FitMode,
            ModelPath = l.ModelState.CurrentModelFilename,
            ModelTexturePath = l.ModelState.CurrentTextureFilename,
            ModelPosition = l.ModelState.Position,
            ModelRotation = l.ModelState.Rotation,
            ModelScale = l.ModelState.Scale,
            ModelAutoRotateSpeed = l.ModelState.AutoRotateSpeed,
            ModelWireframe = l.ModelState.Wireframe,
            ModelWireframeColor = l.ModelState.WireframeColor,
        }).ToList();

        File.WriteAllText(filePath, JsonSerializer.Serialize(data, JsonOptions));
    }

    public static List<Layer> Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return new List<Layer>();
        }

        string json = File.ReadAllText(filePath);
        List<LayerData>? data = JsonSerializer.Deserialize<List<LayerData>>(json, JsonOptions);
        if (data == null)
        {
            return new List<Layer>();
        }

        return data.Select(d =>
        {
            var layer = new Layer(d.ShaderName, d.BlendMode, d.Opacity, d.Enabled)
            {
                SourceType = d.SourceType,
                ImagePath = d.ImagePath,
                PositionX = d.PositionX,
                PositionY = d.PositionY,
                Scale = d.Scale,
                Rotation = d.Rotation,
                FitMode = d.FitMode,
            };

            layer.ModelState.CurrentModelFilename = d.ModelPath;
            layer.ModelState.CurrentTextureFilename = d.ModelTexturePath;
            layer.ModelState.Position = d.ModelPosition;
            layer.ModelState.Rotation = d.ModelRotation;
            layer.ModelState.Scale = d.ModelScale;
            layer.ModelState.AutoRotateSpeed = d.ModelAutoRotateSpeed;
            layer.ModelState.Wireframe = d.ModelWireframe;
            layer.ModelState.WireframeColor = d.ModelWireframeColor;

            return layer;
        }).ToList();
    }
}
