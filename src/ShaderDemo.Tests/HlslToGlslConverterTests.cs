// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Tests;

public class HlslToGlslConverterTests
{
    [Theory]
    [InlineData("float4", "vec4")]
    [InlineData("float3", "vec3")]
    [InlineData("float2", "vec2")]
    [InlineData("float2x2", "mat2")]
    [InlineData("float3x3", "mat3")]
    [InlineData("float4x4", "mat4")]
    public void Convert_MapsHlslTypesToGlslTypes(string hlslType, string expectedGlslType)
    {
        string result = HlslToGlslConverter.Convert($"{hlslType} value;");

        Assert.Contains(expectedGlslType, result);
        Assert.DoesNotContain(hlslType, result);
    }

    [Fact]
    public void Convert_RewritesLerpToMix()
    {
        string result = HlslToGlslConverter.Convert("float3 c = lerp(a, b, t);");

        Assert.Contains("mix(", result);
        Assert.DoesNotContain("lerp(", result);
    }

    [Fact]
    public void Convert_RewritesFracToFract()
    {
        string result = HlslToGlslConverter.Convert("float x = frac(y);");

        Assert.Contains("fract(", result);
    }

    [Fact]
    public void Convert_RewritesAtan2ToAtan()
    {
        string result = HlslToGlslConverter.Convert("float a = atan2(y, x);");

        Assert.Contains("atan(", result);
        Assert.DoesNotContain("atan2(", result);
    }

    [Fact]
    public void Convert_StripsIChannelDefines()
    {
        string result = HlslToGlslConverter.Convert("#define iChannel0 someTexture\nvoid main() {}");

        Assert.DoesNotContain("#define iChannel0", result);
    }

    [Fact]
    public void Convert_StripsStaticKeyword()
    {
        string result = HlslToGlslConverter.Convert("static float3 foo = float3(1.0, 0.0, 0.0);");

        Assert.DoesNotContain("static", result);
    }

    [Fact]
    public void Convert_StripsConstBeforeScalarFloat()
    {
        string result = HlslToGlslConverter.Convert("const float PI = 3.14159;");

        Assert.Equal("float PI = 3.14159;", result);
    }
}
