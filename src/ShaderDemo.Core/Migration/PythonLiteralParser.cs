// Copyright (c) 2026 Patrick JAILLET
using System.Globalization;

namespace ShaderDemo.Core.Migration;

public static class PythonLiteralParser
{
    public static float ParseFloat(string text)
    {
        return float.Parse(text.Trim(), CultureInfo.InvariantCulture);
    }

    public static int ParseInt(string text)
    {
        return (int)ParseFloat(text);
    }

    public static bool ParseBool(string text)
    {
        return text.Trim() == "True";
    }

    public static string ParseString(string text)
    {
        string trimmed = text.Trim();
        if (trimmed.Length >= 2 && (trimmed[0] == '\'' || trimmed[0] == '"'))
        {
            return trimmed[1..^1];
        }

        return trimmed;
    }

    public static float[] ParseTuple(string text)
    {
        string trimmed = text.Trim().TrimEnd(',').Trim();
        if (trimmed.StartsWith('(') || trimmed.StartsWith('['))
        {
            trimmed = trimmed[1..];
        }

        if (trimmed.EndsWith(')') || trimmed.EndsWith(']'))
        {
            trimmed = trimmed[..^1];
        }

        string[] parts = trimmed.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var values = new float[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            values[i] = ParseFloat(parts[i]);
        }

        return values;
    }

    public static string ToPascalCaseFromUpperSnake(string upperSnakeCase)
    {
        string[] parts = upperSnakeCase.Split('_', StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + p[1..].ToLowerInvariant()));
    }
}
