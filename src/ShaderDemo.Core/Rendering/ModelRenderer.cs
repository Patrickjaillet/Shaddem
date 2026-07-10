// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using Silk.NET.OpenGL;

namespace ShaderDemo.Core.Rendering;

public sealed class ModelRenderer : IDisposable
{
    private readonly GL _gl;
    private readonly ShaderProgram _program;

    public ObjModel? Model { get; private set; }
    public Texture? ModelTexture { get; private set; }

    public ModelRenderer(GL gl)
    {
        _gl = gl;
        _program = new ShaderProgram(gl, "Model3D", BuiltinShaders.Vertex3DModel, BuiltinShaders.Fragment3DModel);
    }

    public void LoadModel(string path, Action<string>? log = null)
    {
        ObjModel? loaded = ObjModel.Load(_gl, path, log);
        if (loaded == null) return;

        Model?.Dispose();
        Model = loaded;
    }

    public void LoadTexture(string path, Action<string>? log = null)
    {
        Texture? loaded = TextureLoader.Load(_gl, path, log);
        if (loaded == null) return;

        ModelTexture?.Dispose();
        ModelTexture = loaded;
    }

    public void Render(Model3DState state, float currentTime, float aspect, Texture fallbackTexture)
    {
        if (!state.ShowModel || Model == null) return;

        _gl.Enable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.CullFace);
        if (state.Wireframe) _gl.PolygonMode(GLEnum.FrontAndBack, Silk.NET.OpenGL.PolygonMode.Line);

        Matrix4x4 proj = MathUtils.Mat4Perspective(60.0f, aspect, 0.1f, 100.0f);
        Matrix4x4 view = MathUtils.Mat4Translate(0.0f, 0.0f, 0.0f);

        Matrix4x4 model = Matrix4x4.Identity * MathUtils.Mat4Scale(state.Scale.X, state.Scale.Y, state.Scale.Z);

        float rx = state.Rotation.X + (state.AutoRotateSpeed.X * currentTime * 20.0f);
        float ry = state.Rotation.Y + (state.AutoRotateSpeed.Y * currentTime * 20.0f);
        float rz = state.Rotation.Z + (state.AutoRotateSpeed.Z * currentTime * 20.0f);

        model = model
            * MathUtils.Mat4RotateX(float.DegreesToRadians(rx))
            * MathUtils.Mat4RotateY(float.DegreesToRadians(ry))
            * MathUtils.Mat4RotateZ(float.DegreesToRadians(rz));
        model *= MathUtils.Mat4Translate(state.Position.X, state.Position.Y, state.Position.Z);

        _program.Use();
        _program.SetUniform("m_proj", proj);
        _program.SetUniform("m_view", view);
        _program.SetUniform("m_model", model);

        Texture tex = ModelTexture ?? fallbackTexture;
        tex.Bind(0);
        _program.SetUniform("Texture", 0);
        _program.SetUniform("lightDir", state.LightDir);
        _program.SetUniform("use_solid_color", state.Wireframe ? 1 : 0);
        _program.SetUniform("solid_color", state.WireframeColor);

        Model.Draw();

        if (state.Wireframe) _gl.PolygonMode(GLEnum.FrontAndBack, Silk.NET.OpenGL.PolygonMode.Fill);
        _gl.Disable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.CullFace);
    }

    public void Dispose()
    {
        Model?.Dispose();
        ModelTexture?.Dispose();
        _program.Dispose();
    }
}
