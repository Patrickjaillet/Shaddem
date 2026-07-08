// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;

namespace ShaderDemo.Core.Rendering;

public static class MathUtils
{
    public static Matrix4x4 Mat4Perspective(float fovDegrees, float aspect, float near, float far)
    {
        float f = 1.0f / MathF.Tan(float.DegreesToRadians(fovDegrees) / 2.0f);
        return new Matrix4x4(
            f / aspect, 0, 0, 0,
            0, f, 0, 0,
            0, 0, (far + near) / (near - far), -1,
            0, 0, (2 * far * near) / (near - far), 0);
    }

    public static Matrix4x4 Mat4Translate(float x, float y, float z)
    {
        return new Matrix4x4(
            1, 0, 0, 0,
            0, 1, 0, 0,
            0, 0, 1, 0,
            x, y, z, 1);
    }

    public static Matrix4x4 Mat4Scale(float x, float y, float z)
    {
        return new Matrix4x4(
            x, 0, 0, 0,
            0, y, 0, 0,
            0, 0, z, 0,
            0, 0, 0, 1);
    }

    public static Matrix4x4 Mat4RotateX(float angleRadians)
    {
        float c = MathF.Cos(angleRadians);
        float s = MathF.Sin(angleRadians);
        return new Matrix4x4(
            1, 0, 0, 0,
            0, c, -s, 0,
            0, s, c, 0,
            0, 0, 0, 1);
    }

    public static Matrix4x4 Mat4RotateY(float angleRadians)
    {
        float c = MathF.Cos(angleRadians);
        float s = MathF.Sin(angleRadians);
        return new Matrix4x4(
            c, 0, s, 0,
            0, 1, 0, 0,
            -s, 0, c, 0,
            0, 0, 0, 1);
    }

    public static Matrix4x4 Mat4RotateZ(float angleRadians)
    {
        float c = MathF.Cos(angleRadians);
        float s = MathF.Sin(angleRadians);
        return new Matrix4x4(
            c, -s, 0, 0,
            s, c, 0, 0,
            0, 0, 1, 0,
            0, 0, 0, 1);
    }
}
