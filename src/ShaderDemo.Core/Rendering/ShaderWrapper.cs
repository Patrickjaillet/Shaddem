// Copyright (c) 2026 Patrick JAILLET
namespace ShaderDemo.Core.Rendering;

public static class ShaderWrapper
{
    private const string UniformDeclarations = """
        uniform vec2 iResolution;
        uniform float iTime;
        uniform vec2 iOffset;
        uniform vec4 iMouse;
        uniform vec4 customColor;
        uniform float customSpeed;
        uniform float customIntensity;
        uniform float customStrobe;
        uniform float customVignette;
        uniform float customNoise;
        uniform float customScanlines;
        uniform float customFisheye;
        uniform float customPixelate;
        uniform float customRgbSplit;
        uniform float customWave;
        uniform float customMirror;
        uniform float customRotation;
        uniform float customRotationSpeed;
        uniform float customBloom;
        uniform float customGlitch;
        uniform float customVortex;
        uniform float customGlitchHard;
        uniform float customTintIntensity;
        uniform vec3 customTintColor;
        uniform float customPosterize;
        uniform float customChromaticAberration;
        uniform float customSepia;
        uniform float customInvert;
        uniform float customSolarize;
        uniform float customSwirl;
        uniform float customMosaic;
        uniform float customVhs;
        uniform float customHueShift;
        uniform float customEdgeDetect;
        uniform float customCrosshatch;
        uniform float customDither;
        uniform float customHalftone;
        uniform float customNightVision;
        uniform float customThermal;
        uniform float customFrostedGlass;
        uniform float customEmboss;
        uniform float customSharpen;
        uniform float customBlur;
        uniform float customZoomBlur;
        uniform float customRgbShiftVert;
        uniform float customGlitchAnalog;
        uniform float customCrt;
        uniform float customKaleidoscope;
        uniform float customPolar;
        uniform float customRipple;
        uniform float customSpiral;
        uniform float customBlockNoise;
        uniform float customColorReduce;
        uniform float customGamma;
        uniform float customExposure;
        uniform float customVibrance;
        uniform float customSobelNeon;
        uniform float customDotMatrix;
        uniform float customFilmGrain;
        uniform float customBrightness;
        uniform float customContrast;
        uniform float customSaturation;
        uniform float customScale;
        uniform float customShaderParam1;
        uniform float customAudioKick;
        uniform float customQualityScale;
        uniform sampler2D iChannel0;
        uniform sampler2D iChannel1;
        uniform sampler2D iChannel2;
        uniform sampler2D iChannel3;
        """;

    private const string Helpers = """
        float saturate(float x) { return clamp(x, 0.0, 1.0); }
        vec2 saturate(vec2 x) { return clamp(x, 0.0, 1.0); }
        vec3 saturate(vec3 x) { return clamp(x, 0.0, 1.0); }
        vec4 saturate(vec4 x) { return clamp(x, 0.0, 1.0); }
        #define mul(a,b) (a*b)
        vec3 hueShift(vec3 color, float hue) {
            const vec3 k = vec3(0.57735, 0.57735, 0.57735);
            float cosAngle = cos(hue * 6.28318);
            return vec3(color * cosAngle + cross(k, color) * sin(hue * 6.28318) + k * dot(k, color) * (1.0 - cosAngle));
        }
        """;

    private const string MainBody = """
        if (customGlitchHard > 0.0) {
            float t = iTime * 20.0;
            float seed = floor(fragCoord.y / (5.0 + 40.0 * fract(sin(t) * 43758.5453)));
            float shift = fract(sin(dot(vec2(seed, floor(t)), vec2(12.9898,78.233))) * 43758.5453) - 0.5;
            if (abs(shift) < customGlitchHard * 0.5) {
                fragCoord.x += shift * iResolution.x * customGlitchHard;
            }
        }

        if (customGlitch > 0.0) {
            float t = iTime * 15.0;
            float trigger = fract(sin(dot(vec2(floor(t), 0.0), vec2(12.9898,78.233))) * 43758.5453);
            if (trigger < customGlitch) {
                float block = floor(fragCoord.y / 20.0 + t);
                float noise = fract(sin(dot(vec2(block, floor(t)), vec2(12.9898,78.233))) * 43758.5453);
                fragCoord.x += (noise - 0.5) * iResolution.x * 0.1 * customGlitch;
            }
        }

        if (customRotation != 0.0) {
            vec2 center = iResolution.xy * 0.5;
            vec2 uv = fragCoord - center;
            float rad = radians(customRotation);
            float c = cos(rad);
            float s = sin(rad);
            uv = vec2(uv.x * c - uv.y * s, uv.x * s + uv.y * c);
            fragCoord = uv + center;
        }

        if (customFrostedGlass > 0.0) {
            float noise = fract(sin(dot(fragCoord.xy, vec2(12.9898,78.233))) * 43758.5453);
            fragCoord += (noise - 0.5) * customFrostedGlass * 20.0;
        }

        if (customGlitchAnalog > 0.0) {
            float offset = sin(fragCoord.y * 0.1 + iTime * 10.0) * customGlitchAnalog * 20.0;
            offset += sin(fragCoord.y * 0.01 + iTime * 2.0) * customGlitchAnalog * 50.0;
            fragCoord.x += offset;
        }

        if (customRipple > 0.0) {
            vec2 center = iResolution.xy * 0.5;
            vec2 uv = fragCoord - center;
            float dist = length(uv);
            fragCoord += (uv / dist) * sin(dist * 0.05 - iTime * 5.0) * customRipple * 10.0;
        }

        if (customSpiral != 0.0) {
            vec2 center = iResolution.xy * 0.5;
            vec2 uv = fragCoord - center;
            float dist = length(uv);
            float angle = atan(uv.y, uv.x) + dist * customSpiral * 0.001;
            fragCoord = center + vec2(cos(angle), sin(angle)) * dist;
        }

        if (customPolar > 0.0) {
            vec2 center = iResolution.xy * 0.5;
            vec2 uv = fragCoord - center;
            float radius = length(uv);
            float angle = atan(uv.y, uv.x);
            vec2 polar = vec2(angle / 6.28318 + 0.5, radius / iResolution.x);
            fragCoord = mix(fragCoord, polar * iResolution.xy, customPolar);
        }

        if (customKaleidoscope > 0.0) {
            vec2 center = iResolution.xy * 0.5;
            vec2 uv = (fragCoord - center) / iResolution.y;
            float angle = atan(uv.y, uv.x);
            float segments = 6.0 + customKaleidoscope * 10.0;
            angle = mod(angle, 6.28318 / segments);
            angle = abs(angle - 3.14159 / segments);
            fragCoord = center + vec2(cos(angle), sin(angle)) * length(uv) * iResolution.y;
        }

        if (customVortex != 0.0) {
            vec2 center = iResolution.xy * 0.5;
            vec2 uv = fragCoord - center;
            float dist = length(uv);
            float angle = atan(uv.y, uv.x);
            angle += (1.0 / (dist * 0.01 + 0.2)) * customVortex * 0.5;
            fragCoord = center + vec2(cos(angle), sin(angle)) * dist;
        }

        if (customSwirl != 0.0) {
            vec2 center = iResolution.xy * 0.5;
            vec2 uv = fragCoord - center;
            float dist = length(uv);
            float angle = atan(uv.y, uv.x) + customSwirl * smoothstep(iResolution.x, 0.0, dist);
            fragCoord = center + vec2(cos(angle), sin(angle)) * dist;
        }

        if (customVhs > 0.0) {
            float vhsNoise = fract(sin(dot(vec2(fragCoord.y * 0.01, iTime), vec2(12.9898,78.233))) * 43758.5453);
            if (vhsNoise > 0.99 - (customVhs * 0.05)) {
                fragCoord.x += (vhsNoise - 0.99) * 200.0 * customVhs;
            }
            if (fract(iTime * 0.5) > 0.9) {
                fragCoord.y += sin(iTime * 100.0) * 5.0 * customVhs;
            }
        }

        if (customScale != 1.0) {
            vec2 center = iResolution.xy * 0.5;
            vec2 uv = fragCoord - center;
            uv /= customScale;
            fragCoord = uv + center;
        }

        if (customMirror > 0.0) {
            vec2 uv = fragCoord / iResolution.xy;
            if (abs(customMirror - 1.0) < 0.1) { uv.x = abs(uv.x - 0.5) + 0.5; }
            else if (abs(customMirror - 2.0) < 0.1) { uv.y = abs(uv.y - 0.5) + 0.5; }
            else if (abs(customMirror - 3.0) < 0.1) { uv = abs(uv - 0.5) + 0.5; }
            fragCoord = uv * iResolution.xy;
        }

        if (customWave > 0.0) {
            vec2 uv = fragCoord / iResolution.xy;
            uv.x += sin(uv.y * 10.0 + iTime * 2.0) * 0.02 * customWave;
            uv.y += cos(uv.x * 10.0 + iTime * 2.0) * 0.02 * customWave;
            fragCoord = uv * iResolution.xy;
        }

        if (customFisheye != 0.0) {
            vec2 uv = fragCoord / iResolution.xy;
            vec2 center = vec2(0.5);
            vec2 d = uv - center;
            float r = length(d);
            uv = center + d * (1.0 + customFisheye * r * r);
            fragCoord = uv * iResolution.xy;
        }

        if (customPixelate > 1.0) {
            fragCoord = floor(fragCoord / customPixelate) * customPixelate + (customPixelate * 0.5);
        }

        if (customMosaic > 0.0) {
            float size = customMosaic * 20.0;
            fragCoord = floor(fragCoord / size) * size + (size * 0.5);
        }

        if (customBlockNoise > 0.0) {
            float size = customBlockNoise * 50.0 + 1.0;
            fragCoord = floor(fragCoord / size) * size + (size * 0.5);
        }

        if (customCrt > 0.0) {
            vec2 uv = fragCoord / iResolution.xy;
            uv = uv * 2.0 - 1.0;
            uv *= 1.0 + dot(uv, uv) * customCrt * 0.2;
            uv = uv * 0.5 + 0.5;
            fragCoord = uv * iResolution.xy;
        }

        if (customChromaticAberration > 0.0) {
            vec2 center = iResolution.xy * 0.5;
            vec2 dist = fragCoord - center;

            float r2 = dot(dist, dist) / dot(center, center);

            float f = customChromaticAberration * (0.02 + r2 * 0.08);

            vec4 colR, colG, colB;

            mainImage(colR, center + dist * (1.0 - f));
            mainImage(colG, fragCoord);
            mainImage(colB, center + dist * (1.0 + f));

            fragColor = vec4(colR.r, colG.g, colB.b, 1.0);
        }
        else if (customRgbSplit > 0.0) {
            vec4 colR, colG, colB;
            vec2 offset = vec2(customRgbSplit * 10.0, 0.0);
            mainImage(colR, fragCoord + offset);
            mainImage(colG, fragCoord);
            mainImage(colB, fragCoord - offset);
            fragColor = vec4(colR.r, colG.g, colB.b, 1.0);
        }
        else if (customRgbShiftVert > 0.0) {
            vec4 colR, colG, colB;
            mainImage(colR, fragCoord + vec2(0.0, customRgbShiftVert * 20.0));
            mainImage(colG, fragCoord);
            mainImage(colB, fragCoord - vec2(0.0, customRgbShiftVert * 20.0));
            fragColor = vec4(colR.r, colG.g, colB.b, 1.0);
        } else {
            fragColor = vec4(0.0);
            mainImage(fragColor, fragCoord);
        }

        if (customBlur > 0.0) {
            vec4 sum = vec4(0.0);
            float size = customBlur * 5.0;
            for(float x=-1.0; x<=1.0; x++) {
                for(float y=-1.0; y<=1.0; y++) {
                    vec4 tmp;
                    mainImage(tmp, fragCoord + vec2(x, y) * size);
                    sum += tmp;
                }
            }
            fragColor = sum / 9.0;
        }

        if (customZoomBlur > 0.0) {
            vec2 center = iResolution.xy * 0.5;
            vec4 sum = vec4(0.0);
            for(float i=0.0; i<10.0; i++) {
                vec2 pos = center + (fragCoord - center) * (1.0 - customZoomBlur * i * 0.01);
                vec4 tmp;
                mainImage(tmp, pos);
                sum += tmp;
            }
            fragColor = sum / 10.0;
        }

        if (customSharpen > 0.0) {
            vec4 center = fragColor;
            vec4 sum = vec4(0.0);
            sum += center * 5.0;
            vec4 tmp; mainImage(tmp, fragCoord + vec2(1.0, 0.0)); sum -= tmp;
            mainImage(tmp, fragCoord + vec2(-1.0, 0.0)); sum -= tmp;
            mainImage(tmp, fragCoord + vec2(0.0, 1.0)); sum -= tmp;
            mainImage(tmp, fragCoord + vec2(0.0, -1.0)); sum -= tmp;
            fragColor = mix(fragColor, sum, customSharpen);
        }

        fragColor.rgb *= customBrightness;

        fragColor.rgb = (fragColor.rgb - 0.5) * customContrast + 0.5;

        float luma = dot(fragColor.rgb, vec3(0.299, 0.587, 0.114));
        fragColor.rgb = mix(vec3(luma), fragColor.rgb, customSaturation);

        if (customVibrance != 0.0) {
            float maxComp = max(fragColor.r, max(fragColor.g, fragColor.b));
            float avg = (fragColor.r + fragColor.g + fragColor.b) / 3.0;
            float amt = (maxComp - avg) * (-customVibrance * 3.0);
            fragColor.rgb = mix(fragColor.rgb, vec3(maxComp), amt);
        }

        if (customGamma != 1.0 && customGamma > 0.0) {
            fragColor.rgb = pow(fragColor.rgb, vec3(1.0 / customGamma));
        }

        if (customExposure != 0.0) {
            fragColor.rgb *= pow(2.0, customExposure);
        }

        if (customTintIntensity > 0.0) {
            float gray = dot(fragColor.rgb, vec3(0.299, 0.587, 0.114));
            vec3 tinted = vec3(gray) * customTintColor;
            fragColor.rgb = mix(fragColor.rgb, tinted, customTintIntensity);
        }

        if (customPosterize > 0.0) {
            fragColor.rgb = floor(fragColor.rgb * customPosterize) / customPosterize;
        }

        if (customColorReduce > 0.0) {
            fragColor.rgb = floor(fragColor.rgb * (10.0 - customColorReduce * 8.0)) / (10.0 - customColorReduce * 8.0);
        }

        if (customSepia > 0.0) {
            vec3 sepiaColor = vec3(
                dot(fragColor.rgb, vec3(0.393, 0.769, 0.189)),
                dot(fragColor.rgb, vec3(0.349, 0.686, 0.168)),
                dot(fragColor.rgb, vec3(0.272, 0.534, 0.131))
            );
            fragColor.rgb = mix(fragColor.rgb, sepiaColor, customSepia);
        }

        if (customInvert > 0.0) {
            fragColor.rgb = mix(fragColor.rgb, 1.0 - fragColor.rgb, customInvert);
        }

        if (customSolarize > 0.0) {
            vec3 solar = abs(fragColor.rgb - 0.5) * 2.0;
            fragColor.rgb = mix(fragColor.rgb, solar, customSolarize);
        }

        if (customHueShift > 0.0) {
            fragColor.rgb = hueShift(fragColor.rgb, customHueShift);
        }

        if (customEdgeDetect > 0.0) {
            float edge = length(fwidth(fragColor.rgb));
            fragColor.rgb = mix(fragColor.rgb, vec3(edge * 5.0), customEdgeDetect);
        }

        if (customSobelNeon > 0.0) {
            float edge = length(fwidth(fragColor.rgb));
            fragColor.rgb += vec3(edge * 5.0) * customSobelNeon * vec3(0.0, 1.0, 1.0);
        }

        if (customThermal > 0.0) {
            float lum = dot(fragColor.rgb, vec3(0.299, 0.587, 0.114));
            vec3 thermal = mix(vec3(0.0, 0.0, 1.0), vec3(1.0, 1.0, 0.0), lum);
            thermal = mix(thermal, vec3(1.0, 0.0, 0.0), smoothstep(0.5, 1.0, lum));
            fragColor.rgb = mix(fragColor.rgb, thermal, customThermal);
        }

        if (customNightVision > 0.0) {
            float lum = dot(fragColor.rgb, vec3(0.299, 0.587, 0.114));
            vec3 green = vec3(0.0, 1.0, 0.0) * lum * 1.5;
            float noise = fract(sin(dot(gl_FragCoord.xy + iTime * 100.0, vec2(12.9898,78.233))) * 43758.5453);
            green += vec3(0.0, 0.2, 0.0) * noise;
            fragColor.rgb = mix(fragColor.rgb, green, customNightVision);
        }

        if (customCrosshatch > 0.0) {
            float lum = dot(fragColor.rgb, vec3(0.299, 0.587, 0.114));
            vec3 hatch = vec3(1.0);
            if (lum < 0.8) { if (mod(gl_FragCoord.x + gl_FragCoord.y, 10.0) == 0.0) hatch = vec3(0.0); }
            if (lum < 0.6) { if (mod(gl_FragCoord.x - gl_FragCoord.y, 10.0) == 0.0) hatch = vec3(0.0); }
            if (lum < 0.4) { if (mod(gl_FragCoord.x + gl_FragCoord.y - 5.0, 10.0) == 0.0) hatch = vec3(0.0); }
            if (lum < 0.2) { if (mod(gl_FragCoord.x - gl_FragCoord.y - 5.0, 10.0) == 0.0) hatch = vec3(0.0); }
            fragColor.rgb = mix(fragColor.rgb, hatch, customCrosshatch);
        }

        if (customDither > 0.0) {
            float dither = fract(sin(dot(gl_FragCoord.xy, vec2(12.9898,78.233))) * 43758.5453);
            vec3 col = fragColor.rgb + (dither - 0.5) * customDither;
            fragColor.rgb = floor(col * 4.0) / 4.0;
        }

        if (customHalftone > 0.0) {
            float size = customHalftone * 10.0;
            vec2 pos = floor(gl_FragCoord.xy / size) * size;
            vec2 center = pos + size * 0.5;
            float dist = length(gl_FragCoord.xy - center) / (size * 0.6);

            float lum = dot(fragColor.rgb, vec3(0.299, 0.587, 0.114));

            vec3 dotCol = vec3(0.1);
            vec3 bgCol = vec3(0.9);
            fragColor.rgb = mix(fragColor.rgb, mix(dotCol, bgCol, step(1.0 - lum, dist)), customHalftone);
        }

        if (customDotMatrix > 0.0) {
            float size = 10.0;
            vec2 pos = fract(gl_FragCoord.xy / size);
            float dist = length(pos - 0.5);
            if (dist > 0.4) fragColor.rgb *= 0.0;
            fragColor.rgb *= vec3(0.2, 1.0, 0.2);
        }

        if (customEmboss > 0.0) {
            float lum = dot(fragColor.rgb, vec3(0.333));
            vec4 tmp; mainImage(tmp, gl_FragCoord.xy + vec2(1.0, 1.0));
            float lum2 = dot(tmp.rgb, vec3(0.333));
            float val = (lum - lum2) * customEmboss * 5.0 + 0.5;
            fragColor.rgb = mix(fragColor.rgb, vec3(val), customEmboss);
        }

        if (customFilmGrain > 0.0) {
            float noise = (fract(sin(dot(gl_FragCoord.xy + iTime, vec2(12.9898,78.233))) * 43758.5453) - 0.5);
            fragColor.rgb += noise * customFilmGrain;
        }

        if (customStrobe > 0.0) { fragColor = mix(fragColor, vec4(1.0), customStrobe); }

        if (customVignette > 0.0) {
            vec2 uv = gl_FragCoord.xy / iResolution.xy;
            float v = 1.0 - dot(uv - 0.5, uv - 0.5) * customVignette * 2.0;
            fragColor.rgb *= clamp(v, 0.0, 1.0);
        }

        if (customNoise > 0.0) {
            float noise = (fract(sin(dot(gl_FragCoord.xy + iTime * 100.0, vec2(12.9898,78.233))) * 43758.5453) - 0.5) * 2.0;
            fragColor.rgb += noise * customNoise;
        }

        if (customScanlines > 0.0) {
            float scan = sin(gl_FragCoord.y * 0.8) * 0.5 + 0.5;
            fragColor.rgb *= 1.0 - (scan * customScanlines * 0.5);
        }
        """;

    public static string Wrap(string userFragmentCode)
    {
        return $$"""
            #version 330
            out vec4 fragColor;
            {{UniformDeclarations}}
            {{Helpers}}

            {{userFragmentCode}}

            void main() {
                vec2 fragCoord = gl_FragCoord.xy - iOffset;
                {{MainBody}}
            }
            """;
    }
}
