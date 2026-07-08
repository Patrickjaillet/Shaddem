// Copyright (c) 2026 Patrick JAILLET
namespace ShaderDemo.Core.Rendering;

public static class BuiltinShaders
{
    public const string VertexPassthrough = """
        #version 330
        layout(location = 0) in vec2 in_vert;
        out vec2 v_uv;
        void main() {
            v_uv = in_vert * 0.5 + 0.5;
            gl_Position = vec4(in_vert, 0.0, 1.0);
        }
        """;

    public const string FragmentTexture = """
        #version 330
        uniform sampler2D tex;
        in vec2 v_uv;
        out vec4 fragColor;
        void main() {
            fragColor = texture(tex, v_uv);
        }
        """;

    public const string FragmentBlend = """
        #version 330
        in vec2 v_uv;
        out vec4 fragColor;
        uniform sampler2D tex_base;
        uniform sampler2D tex_layer;
        uniform int mode;
        uniform float opacity;
        float blendOverlay(float base, float blend) {
            return base < 0.5 ? (2.0 * base * blend) : (1.0 - 2.0 * (1.0 - base) * (1.0 - blend));
        }
        void main() {
            vec4 base = texture(tex_base, v_uv);
            vec4 layer = texture(tex_layer, v_uv);
            vec3 baseColor = base.rgb;
            vec3 layerColor = layer.rgb;
            float layerAlpha = layer.a;
            vec3 result = baseColor;
            if (mode == 0) result = layerColor;
            else if (mode == 1) result = baseColor + layerColor;
            else if (mode == 2) result = baseColor * layerColor;
            else if (mode == 3) result = 1.0 - (1.0 - baseColor) * (1.0 - layerColor);
            else if (mode == 4) result = vec3(blendOverlay(baseColor.r, layerColor.r), blendOverlay(baseColor.g, layerColor.g), blendOverlay(baseColor.b, layerColor.b));
            else if (mode == 5) result = mix(baseColor, layerColor, layerAlpha);
            else if (mode == 6) result = abs(baseColor - layerColor);
            else if (mode == 7) result = baseColor + layerColor - 2.0 * baseColor * layerColor;
            fragColor = vec4(mix(baseColor, result, opacity), 1.0);
        }
        """;

    public const string FragmentFallback = """
        #version 330
        out vec4 fragColor;
        void main() {
            fragColor = vec4(0.1, 0.1, 0.1, 1.0);
        }
        """;

    public const string FragmentTransition = """
        #version 330
        uniform sampler2D tex_prev;
        uniform sampler2D tex_next;
        uniform float progress;
        uniform int type;
        in vec2 v_uv;
        out vec4 fragColor;
        void main() {
            vec4 c1 = texture(tex_prev, v_uv);
            vec4 c2 = texture(tex_next, v_uv);

            if (type == 1) {
                fragColor = mix(c1, c2, progress);
            } else if (type == 2) {
                vec2 center = vec2(0.5);
                float scale1 = mix(1.0, 0.5, progress);
                vec2 uv_prev = (v_uv - center) * scale1 + center;
                float scale2 = mix(0.5, 1.0, progress);
                vec2 uv_next = (v_uv - center) * scale2 + center;
                fragColor = mix(texture(tex_prev, uv_prev), texture(tex_next, uv_next), progress);
            } else if (type == 3) {
                float dist = abs(progress - 0.5) * 2.0;
                float blocks = mix(10.0, 1000.0, dist);
                vec2 uv_pix = floor(v_uv * blocks) / blocks + (0.5 / blocks);
                fragColor = mix(texture(tex_prev, uv_pix), texture(tex_next, uv_pix), progress);
            } else {
                float mix_val = step(progress, v_uv.x);
                float line = 1.0 - abs(v_uv.x - progress) * 50.0;
                vec4 col = mix(c2, c1, mix_val);
                fragColor = col + vec4(1.0) * clamp(line, 0.0, 1.0) * 0.5;
            }
        }
        """;

    public const string FragmentMotionBlur = """
        #version 330
        uniform sampler2D tex_new;
        uniform sampler2D tex_old;
        uniform float blur_amount;
        in vec2 v_uv;
        out vec4 fragColor;
        void main() {
            vec4 c_new = texture(tex_new, v_uv);
            vec4 c_old = texture(tex_old, v_uv);
            fragColor = mix(c_new, c_old, blur_amount);
        }
        """;

    public const string FragmentSpectrum = """
        #version 330
        uniform sampler2D tex_spectrum;
        uniform vec4 color;
        uniform float opacity;
        in vec2 v_uv;
        out vec4 fragColor;
        void main() {
            float intensity = texture(tex_spectrum, vec2(v_uv.x, 0.5)).r;
            float bar = step(v_uv.y, intensity);
            fragColor = color * bar * opacity;
        }
        """;

    public const string FragmentWaveform = """
        #version 330
        uniform sampler2D tex_waveform;
        uniform vec4 color;
        uniform float opacity;
        in vec2 v_uv;
        out vec4 fragColor;
        void main() {
            float val = texture(tex_waveform, vec2(v_uv.x, 0.5)).r;
            float y = val * 0.5 + 0.5;
            float dist = abs(v_uv.y - y);
            float line = smoothstep(0.015, 0.0, dist);
            fragColor = color * line * opacity;
        }
        """;

    public const string FragmentFade = """
        #version 330
        uniform sampler2D tex;
        uniform float decay;
        in vec2 v_uv;
        out vec4 fragColor;
        void main() {
            vec4 color = texture(tex, v_uv);
            fragColor = color * decay;
        }
        """;

    public const string FragmentOverlay = """
        #version 330
        uniform sampler2D text_texture;
        uniform float alpha;
        uniform vec2 uv_offset;
        uniform float glitch_intensity;
        uniform float time;
        uniform float wave_intensity;
        uniform float rainbow_intensity;
        uniform float typewriter_progress;
        in vec2 v_uv;
        out vec4 fragColor;

        float rand(vec2 co) {
            return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
        }

        void main() {
            vec2 uv = v_uv - uv_offset;

            if (wave_intensity > 0.0) {
                uv.y += sin(uv.x * 20.0 + time * 5.0) * 0.01 * wave_intensity;
            }

            if (uv.x > typewriter_progress) {
                discard;
            }

            vec4 col = vec4(0.0);

            if (glitch_intensity > 0.0) {
                float split = floor(uv.y * 20.0 + time * 10.0);
                float shift = rand(vec2(split, floor(time * 15.0)));

                if (shift < glitch_intensity * 0.3) {
                    uv.x += (rand(vec2(time, uv.y)) - 0.5) * 0.1 * glitch_intensity;
                }

                float r = texture(text_texture, uv + vec2(0.01 * glitch_intensity, 0.0)).r;
                float g = texture(text_texture, uv).g;
                float b = texture(text_texture, uv - vec2(0.01 * glitch_intensity, 0.0)).b;
                float a = max(max(texture(text_texture, uv).a, texture(text_texture, uv + vec2(0.01,0)).a), texture(text_texture, uv - vec2(0.01,0)).a);

                col = vec4(r, g, b, a);
            } else {
                col = texture(text_texture, uv);
            }

            if (rainbow_intensity > 0.0 && col.a > 0.0) {
                vec3 rainbow = 0.5 + 0.5 * cos(time + uv.xyx + vec3(0, 2, 4));
                col.rgb = mix(col.rgb, rainbow, rainbow_intensity);
            }

            fragColor = col * vec4(1.0, 1.0, 1.0, alpha);
        }
        """;

    public const string FragmentBloomDownsample = """
        #version 330
        uniform sampler2D tex;
        in vec2 v_uv;
        out vec4 fragColor;
        void main() {
            vec4 c = texture(tex, v_uv);
            float brightness = dot(c.rgb, vec3(0.2126, 0.7152, 0.0722));
            fragColor = (brightness > 0.7) ? c : vec4(0.0, 0.0, 0.0, 1.0);
        }
        """;

    public const string FragmentBloomBlur = """
        #version 330
        uniform sampler2D tex;
        uniform vec2 dir;
        in vec2 v_uv;
        out vec4 fragColor;
        void main() {
            vec4 sum = texture(tex, v_uv) * 0.227027;
            sum += texture(tex, v_uv + dir * 1.3846153846) * 0.3162162162;
            sum += texture(tex, v_uv - dir * 1.3846153846) * 0.3162162162;
            sum += texture(tex, v_uv + dir * 3.2307692308) * 0.0702702703;
            sum += texture(tex, v_uv - dir * 3.2307692308) * 0.0702702703;
            fragColor = sum;
        }
        """;

    public const string FragmentBloomCombine = """
        #version 330
        uniform sampler2D tex_scene;
        uniform sampler2D tex_bloom;
        uniform float intensity;
        in vec2 v_uv;
        out vec4 fragColor;
        void main() {
            fragColor = texture(tex_scene, v_uv) + texture(tex_bloom, v_uv) * intensity;
        }
        """;

    public const string FragmentDatamosh = """
        #version 330
        uniform sampler2D tex_new;
        uniform sampler2D tex_old;
        uniform float amount;
        uniform float time;
        in vec2 v_uv;
        out vec4 fragColor;
        float hash(vec2 p) { return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453); }
        float noise(vec2 p) {
            vec2 i = floor(p);
            vec2 f = fract(p);
            f = f * f * (3.0 - 2.0 * f);
            return mix(mix(hash(i + vec2(0.0,0.0)), hash(i + vec2(1.0,0.0)), f.x), mix(hash(i + vec2(0.0,1.0)), hash(i + vec2(1.0,1.0)), f.x), f.y);
        }
        void main() {
            vec2 uv = v_uv;
            float blockSize = 20.0 + (1.0 - amount) * 50.0;
            vec2 blockUV = floor(uv * blockSize) / blockSize;
            vec2 flow = vec2(noise(blockUV * 5.0 + time), noise(blockUV * 5.0 + time + 10.0)) - 0.5;
            flow *= 0.1 * amount;
            vec4 old = texture(tex_old, uv - flow);
            vec4 current = texture(tex_new, uv);
            fragColor = mix(current, old, amount * 0.95);
        }
        """;

    public const string Vertex3DModel = """
        #version 330
        uniform mat4 m_proj;
        uniform mat4 m_view;
        uniform mat4 m_model;
        layout(location = 0) in vec3 in_position;
        layout(location = 1) in vec2 in_texcoord;
        layout(location = 2) in vec3 in_normal;
        out vec2 v_text;
        out vec3 v_norm;
        void main() {
            gl_Position = m_proj * m_view * m_model * vec4(in_position, 1.0);
            v_text = in_texcoord;
            v_norm = mat3(transpose(inverse(m_model))) * in_normal;
        }
        """;

    public const string Fragment3DModel = """
        #version 330
        uniform sampler2D Texture;
        uniform vec3 lightDir;
        uniform bool use_solid_color;
        uniform vec3 solid_color;
        in vec2 v_text;
        in vec3 v_norm;
        out vec4 f_color;
        void main() {
            if (use_solid_color) {
                f_color = vec4(solid_color, 1.0);
            } else {
                vec4 tex = texture(Texture, v_text);
                vec3 norm = normalize(v_norm);
                vec3 lDir = normalize(lightDir);
                float diff = max(dot(norm, lDir), 0.2);
                f_color = vec4(tex.rgb * diff, tex.a);
            }
        }
        """;

    public const string VertexParticle = """
        #version 330
        uniform mat4 m_proj;
        uniform mat4 m_view;
        uniform float point_size;
        layout(location = 0) in vec4 in_pos;
        void main() {
            gl_Position = m_proj * m_view * in_pos;
            gl_PointSize = point_size / gl_Position.w;
        }
        """;

    public const string FragmentParticle = """
        #version 330
        uniform vec4 color;
        out vec4 fragColor;
        void main() {
            vec2 circCoord = 2.0 * gl_PointCoord - 1.0;
            if (dot(circCoord, circCoord) > 1.0) discard;
            fragColor = color;
        }
        """;

    public const string FragmentFeedback = """
        #version 330
        uniform sampler2D tex_new;
        uniform sampler2D tex_old;
        uniform float zoom;
        uniform float rotation;
        uniform float opacity;
        in vec2 v_uv;
        out vec4 fragColor;
        void main() {
            vec2 uv = v_uv - 0.5;
            float c = cos(rotation);
            float s = sin(rotation);
            uv = mat2(c, -s, s, c) * uv;
            uv = uv / zoom + 0.5;
            vec4 old = texture(tex_old, uv);
            vec4 current = texture(tex_new, v_uv);
            fragColor = mix(current, old, opacity);
        }
        """;
}
