// Copyright (c) 2026 Patrick JAILLET
namespace ShaderDemo.Core.Rendering;

public static class ShaderErrorLog
{
    private static readonly Dictionary<string, string> _errors = new();
    private static readonly Dictionary<string, string> _lastGoodSource = new();

    public static IReadOnlyDictionary<string, string> Errors => _errors;

    public static void SetError(string name, string message)
    {
        _errors[name] = message;
    }

    public static void ClearError(string name)
    {
        _errors.Remove(name);
    }

    public static void Clear()
    {
        _errors.Clear();
    }

    public static void SetLastGoodSource(string name, string glslSource)
    {
        _lastGoodSource[name] = glslSource;
    }

    public static bool TryGetLastGoodSource(string name, out string glslSource)
    {
        return _lastGoodSource.TryGetValue(name, out glslSource!);
    }
}
