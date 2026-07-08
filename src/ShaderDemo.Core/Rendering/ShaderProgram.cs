// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using Silk.NET.OpenGL;

namespace ShaderDemo.Core.Rendering;

public sealed class ShaderCompilationException : Exception
{
    public ShaderCompilationException(string message) : base(message)
    {
    }
}

public sealed class ShaderProgram : IDisposable
{
    private readonly GL _gl;
    private readonly uint _handle;
    private readonly Dictionary<string, int> _uniformLocations = new();

    public string Name { get; }

    public ShaderProgram(GL gl, string name, string vertexSource, string fragmentSource)
    {
        _gl = gl;
        Name = name;

        uint vertexShader = CompileShader(gl, ShaderType.VertexShader, vertexSource, name);
        uint fragmentShader = CompileShader(gl, ShaderType.FragmentShader, fragmentSource, name);

        _handle = gl.CreateProgram();
        gl.AttachShader(_handle, vertexShader);
        gl.AttachShader(_handle, fragmentShader);
        gl.LinkProgram(_handle);

        gl.GetProgram(_handle, ProgramPropertyARB.LinkStatus, out int linkStatus);
        if (linkStatus == 0)
        {
            string log = gl.GetProgramInfoLog(_handle);
            gl.DeleteProgram(_handle);
            throw new ShaderCompilationException($"Failed to link shader program '{name}': {log}");
        }

        gl.DetachShader(_handle, vertexShader);
        gl.DetachShader(_handle, fragmentShader);
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);
    }

    private static uint CompileShader(GL gl, ShaderType type, string source, string ownerName)
    {
        uint shader = gl.CreateShader(type);
        gl.ShaderSource(shader, source);
        gl.CompileShader(shader);

        gl.GetShader(shader, ShaderParameterName.CompileStatus, out int compileStatus);
        if (compileStatus == 0)
        {
            string log = gl.GetShaderInfoLog(shader);
            gl.DeleteShader(shader);
            throw new ShaderCompilationException($"Failed to compile {type} for '{ownerName}': {log}");
        }

        return shader;
    }

    public void Use()
    {
        _gl.UseProgram(_handle);
    }

    private int GetUniformLocation(string name)
    {
        if (_uniformLocations.TryGetValue(name, out int cached))
        {
            return cached;
        }

        int location = _gl.GetUniformLocation(_handle, name);
        _uniformLocations[name] = location;
        return location;
    }

    public bool HasUniform(string name)
    {
        return GetUniformLocation(name) != -1;
    }

    public void SetUniform(string name, int value)
    {
        int location = GetUniformLocation(name);
        if (location != -1) _gl.Uniform1(location, value);
    }

    public void SetUniform(string name, float value)
    {
        int location = GetUniformLocation(name);
        if (location != -1) _gl.Uniform1(location, value);
    }

    public void SetUniform(string name, Vector2 value)
    {
        int location = GetUniformLocation(name);
        if (location != -1) _gl.Uniform2(location, value.X, value.Y);
    }

    public void SetUniform(string name, Vector3 value)
    {
        int location = GetUniformLocation(name);
        if (location != -1) _gl.Uniform3(location, value.X, value.Y, value.Z);
    }

    public void SetUniform(string name, Vector4 value)
    {
        int location = GetUniformLocation(name);
        if (location != -1) _gl.Uniform4(location, value.X, value.Y, value.Z, value.W);
    }

    public unsafe void SetUniform(string name, Matrix4x4 value)
    {
        int location = GetUniformLocation(name);
        if (location == -1) return;
        _gl.UniformMatrix4(location, 1, false, (float*)&value);
    }

    public void Dispose()
    {
        _gl.DeleteProgram(_handle);
    }
}
