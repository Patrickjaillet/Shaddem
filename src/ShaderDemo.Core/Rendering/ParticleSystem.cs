// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using Silk.NET.OpenGL;

namespace ShaderDemo.Core.Rendering;

public sealed class ParticleSystem : IDisposable
{
    private readonly GL _gl;
    private readonly ShaderProgram _program;
    private readonly uint _vao;
    private readonly uint _vbo;
    private readonly Random _random = new();

    private Vector4[] _positions = Array.Empty<Vector4>();
    private Vector4[] _velocities = Array.Empty<Vector4>();

    public int Count { get; private set; }

    public ParticleSystem(GL gl)
    {
        _gl = gl;
        _program = new ShaderProgram(gl, "Particles", BuiltinShaders.VertexParticle, BuiltinShaders.FragmentParticle);
        _vao = gl.GenVertexArray();
        _vbo = gl.GenBuffer();
    }

    public void Resize(int count)
    {
        Count = count;
        _positions = new Vector4[count];
        _velocities = new Vector4[count];

        for (int i = 0; i < count; i++)
        {
            _positions[i] = new Vector4(RandSigned() * 10.0f, RandSigned() * 10.0f, RandSigned() * 10.0f, 1.0f);
            _velocities[i] = new Vector4(RandSigned() * 0.5f, RandSigned() * 0.5f, RandSigned() * 0.5f, 0.0f);
        }

        UploadBuffer();
    }

    private float RandSigned()
    {
        return (float)(_random.NextDouble() - 0.5);
    }

    private unsafe void UploadBuffer()
    {
        _gl.BindVertexArray(_vao);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        fixed (Vector4* data = _positions)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(_positions.Length * sizeof(Vector4)), data, BufferUsageARB.DynamicDraw);
        }

        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, (uint)sizeof(Vector4), null);
        _gl.BindVertexArray(0);
    }

    public void Update(float dt, float kick)
    {
        if (Count == 0) return;

        for (int i = 0; i < Count; i++)
        {
            Vector4 p = _positions[i];
            Vector4 v = _velocities[i];

            p += v * dt * 5.0f;

            if (kick > 0.0f)
            {
                var xyz = new Vector3(p.X, p.Y, p.Z);
                if (xyz.LengthSquared() > 0.000001f)
                {
                    Vector3 push = Vector3.Normalize(xyz) * kick * 0.05f;
                    p.X += push.X;
                    p.Y += push.Y;
                    p.Z += push.Z;
                }
            }

            float length = MathF.Sqrt((p.X * p.X) + (p.Y * p.Y) + (p.Z * p.Z));
            if (length > 15.0f)
            {
                p = new Vector4(RandSigned() * 2.0f, RandSigned() * 2.0f, RandSigned() * 2.0f, 1.0f);
                v = new Vector4(RandSigned() * 0.5f, RandSigned() * 0.5f, RandSigned() * 0.5f, 0.0f);
            }

            _positions[i] = p;
            _velocities[i] = v;
        }

        UploadBuffer();
    }

    public void Render(float currentTime, float aspect, float pointSize, Vector4 color)
    {
        if (Count == 0) return;

        Matrix4x4 proj = MathUtils.Mat4Perspective(60.0f, aspect, 0.1f, 100.0f);
        float t = currentTime * 0.2f;
        Matrix4x4 view = MathUtils.Mat4Translate(0.0f, 0.0f, -15.0f) * MathUtils.Mat4RotateY(t) * MathUtils.Mat4RotateX(t * 0.5f);

        _gl.Enable(EnableCap.ProgramPointSize);
        _gl.Enable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _program.Use();
        _program.SetUniform("m_proj", proj);
        _program.SetUniform("m_view", view);
        _program.SetUniform("point_size", pointSize);
        _program.SetUniform("color", color);

        _gl.BindVertexArray(_vao);
        _gl.DrawArrays(PrimitiveType.Points, 0, (uint)Count);
        _gl.BindVertexArray(0);

        _gl.Disable(EnableCap.DepthTest);
        _gl.Disable(EnableCap.ProgramPointSize);
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
        _program.Dispose();
    }
}
