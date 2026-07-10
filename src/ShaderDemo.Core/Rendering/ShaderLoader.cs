// Copyright (c) 2026 Patrick JAILLET
using System.Text;

namespace ShaderDemo.Core.Rendering;

public static class ShaderLoader
{
    private static string StripNonAscii(string source)
    {
        var builder = new StringBuilder(source.Length);
        foreach (char c in source)
        {
            builder.Append(c < 128 ? c : ' ');
        }

        return builder.ToString();
    }

    public static int LoadFromDirectory(ShaderManager manager, string directoryPath, Action<string>? log = null)
    {
        if (!Directory.Exists(directoryPath))
        {
            log?.Invoke($"Shader directory not found: {directoryPath}");
            return 0;
        }

        string[] shaderFiles = Directory.GetFiles(directoryPath, "*.glsl")
            .Concat(Directory.GetFiles(directoryPath, "*.hlsl"))
            .ToArray();
        Array.Sort(shaderFiles, StringComparer.Ordinal);

        if (shaderFiles.Length == 0)
        {
            log?.Invoke($"No shaders found in {directoryPath}");
            return 0;
        }

        int loaded = 0;
        foreach (string shaderFile in shaderFiles)
        {
            if (LoadFile(manager, shaderFile, log)) loaded++;
        }

        return loaded;
    }

    public static bool LoadFile(ShaderManager manager, string shaderFile, Action<string>? log = null)
    {
        string name = Path.GetFileName(shaderFile);
        try
        {
            string sourceCode = StripNonAscii(File.ReadAllText(shaderFile));
            string glslCode = HlslToGlslConverter.Convert(sourceCode);
            manager.RegisterShader(name, glslCode);
            ShaderErrorLog.ClearError(name);
            ShaderErrorLog.SetLastGoodSource(name, glslCode);
            log?.Invoke($"Loaded shader: {name}");
            return true;
        }
        catch (Exception ex) when (ex is ShaderCompilationException or IOException)
        {
            ShaderErrorLog.SetError(name, ex.Message);
            log?.Invoke($"Failed to load shader {name}: {ex.Message}");
            return false;
        }
    }
}
