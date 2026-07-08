// Copyright (c) 2026 Patrick JAILLET
using System.Text.RegularExpressions;

namespace ShaderDemo.Core.Rendering;

public static partial class HlslToGlslConverter
{
    public static string Convert(string hlslCode)
    {
        string glsl = hlslCode;

        glsl = IChannelDefineRegex().Replace(glsl, "");
        glsl = glsl.Replace("static", "");
        glsl = ConstVectorRegex().Replace(glsl, "$1");

        glsl = glsl.Replace("float2x2", "mat2");
        glsl = glsl.Replace("float3x3", "mat3");
        glsl = glsl.Replace("float4x4", "mat4");
        glsl = glsl.Replace("float4", "vec4");
        glsl = glsl.Replace("float3", "vec3");
        glsl = glsl.Replace("float2", "vec2");
        glsl = glsl.Replace("lerp(", "mix(");
        glsl = glsl.Replace("frac(", "fract(");
        glsl = glsl.Replace("atan2(", "atan(");

        return glsl;
    }

    [GeneratedRegex(@"#define\s+iChannel\d\s+.*")]
    private static partial Regex IChannelDefineRegex();

    [GeneratedRegex(@"\bconst\s+(vec\d|float|mat\d)\b")]
    private static partial Regex ConstVectorRegex();
}
