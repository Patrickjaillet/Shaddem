// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;

namespace ShaderDemo.Core.Rendering;

public sealed class Model3DState
{
    public bool ShowModel;
    public Vector3 Position = new(0.0f, 0.0f, -3.0f);
    public Vector3 Rotation = Vector3.Zero;
    public Vector3 Scale = Vector3.One;
    public Vector3 AutoRotateSpeed = Vector3.Zero;
    public Vector3 LightDir = new(0.5f, 1.0f, 0.5f);
    public bool Wireframe;
    public Vector3 WireframeColor = Vector3.One;
    public string? CurrentModelFilename;
    public string? CurrentTextureFilename;
}
