// Copyright (c) 2026 Patrick JAILLET
namespace ShaderDemo.Core.Rendering;

public static class ShaderErrorLog
{
    private static readonly Dictionary<string, string> _errors = new();

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
}
