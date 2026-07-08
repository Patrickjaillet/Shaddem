// Copyright (c) 2026 Patrick JAILLET
using System.Globalization;
using System.Numerics;
using Silk.NET.OpenGL;

namespace ShaderDemo.Core.Rendering;

public sealed class ObjModel : IDisposable
{
    private readonly GL _gl;
    private readonly uint _vao;
    private readonly uint _vbo;

    public int VertexCount { get; }

    private ObjModel(GL gl, uint vao, uint vbo, int vertexCount)
    {
        _gl = gl;
        _vao = vao;
        _vbo = vbo;
        VertexCount = vertexCount;
    }

    public static unsafe ObjModel? Load(GL gl, string path, Action<string>? log = null)
    {
        if (!File.Exists(path))
        {
            log?.Invoke($"Model not found: {path}");
            return null;
        }

        try
        {
            var vertices = new List<Vector3>();
            var texcoords = new List<Vector2>();
            var normals = new List<Vector3>();
            var buffer = new List<float>();

            foreach (string line in File.ReadAllLines(path))
            {
                if (line.StartsWith('#')) continue;
                string[] values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (values.Length == 0) continue;

                switch (values[0])
                {
                    case "v":
                        vertices.Add(new Vector3(ParseF(values[1]), ParseF(values[2]), ParseF(values[3])));
                        break;
                    case "vt":
                        texcoords.Add(new Vector2(ParseF(values[1]), ParseF(values[2])));
                        break;
                    case "vn":
                        normals.Add(new Vector3(ParseF(values[1]), ParseF(values[2]), ParseF(values[3])));
                        break;
                    case "f":
                        var face = new List<(int V, int Vt, int Vn)>();
                        for (int i = 1; i < values.Length; i++)
                        {
                            string[] w = values[i].Split('/');
                            int v = int.Parse(w[0], CultureInfo.InvariantCulture) - 1;
                            int vt = w.Length > 1 && w[1].Length > 0 ? int.Parse(w[1], CultureInfo.InvariantCulture) - 1 : -1;
                            int vn = w.Length > 2 && w[2].Length > 0 ? int.Parse(w[2], CultureInfo.InvariantCulture) - 1 : -1;
                            face.Add((v, vt, vn));
                        }

                        for (int i = 1; i < face.Count - 1; i++)
                        {
                            AppendVertex(buffer, face[0], vertices, texcoords, normals);
                            AppendVertex(buffer, face[i], vertices, texcoords, normals);
                            AppendVertex(buffer, face[i + 1], vertices, texcoords, normals);
                        }

                        break;
                }
            }

            float[] data = buffer.ToArray();
            int vertexCount = data.Length / 8;

            uint vao = gl.GenVertexArray();
            uint vbo = gl.GenBuffer();
            gl.BindVertexArray(vao);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

            fixed (float* ptr = data)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(data.Length * sizeof(float)), ptr, BufferUsageARB.StaticDraw);
            }

            const uint stride = 8 * sizeof(float);
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, null);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)(3 * sizeof(float)));
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, (void*)(5 * sizeof(float)));

            gl.BindVertexArray(0);
            return new ObjModel(gl, vao, vbo, vertexCount);
        }
        catch (Exception ex)
        {
            log?.Invoke($"Failed to load OBJ {path}: {ex.Message}");
            return null;
        }
    }

    private static void AppendVertex(List<float> buffer, (int V, int Vt, int Vn) point, List<Vector3> vertices, List<Vector2> texcoords, List<Vector3> normals)
    {
        Vector3 v = vertices[point.V];
        Vector2 vt = point.Vt >= 0 ? texcoords[point.Vt] : Vector2.Zero;
        Vector3 vn = point.Vn >= 0 ? normals[point.Vn] : new Vector3(0, 1, 0);

        buffer.Add(v.X); buffer.Add(v.Y); buffer.Add(v.Z);
        buffer.Add(vt.X); buffer.Add(vt.Y);
        buffer.Add(vn.X); buffer.Add(vn.Y); buffer.Add(vn.Z);
    }

    private static float ParseF(string s) => float.Parse(s, CultureInfo.InvariantCulture);

    public void Draw()
    {
        _gl.BindVertexArray(_vao);
        _gl.DrawArrays(PrimitiveType.Triangles, 0, (uint)VertexCount);
        _gl.BindVertexArray(0);
    }

    public void Dispose()
    {
        _gl.DeleteVertexArray(_vao);
        _gl.DeleteBuffer(_vbo);
    }
}
