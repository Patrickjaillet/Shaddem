// Cloud&Fluide
// Shadertoy ID: 7XBGzD
// Description: // https://github.com/Patrickjaillet/Z-GL
// Tags: cloud

#define AA 1

float F(vec2 p, float r) {
    float v = 0.0, a = 0.5;
    mat2 R = mat2(cos(0.5), sin(0.5), -sin(0.5), cos(0.5));
    for (int i = 0; i < 6; i++) {
        v += a * dot(cos(p * 1.5), sin(p.yx * 1.5 + r));
        p = R * p * 2.1 + 100.0;
        a *= 0.48;
    }
    return v;
}
// https://github.com/Patrickjaillet/Z-GL
vec4 render(vec2 fragCoord, vec2 resolution, float time) {
    vec4 fragColor = vec4(0.0);
    vec2 uv = (fragCoord * 2.0 - resolution.xy) / resolution.y;
    vec3 ro = vec3(0.0, 0.0, -2.5), rd = normalize(vec3(uv, 1.2));
    float ta = time * 0.15;
    mat3 cam = mat3(cos(ta), 0.0, sin(ta), 0.0, 1.0, 0.0, -sin(ta), 0.0, cos(ta));
    rd = cam * rd; ro = cam * ro;
    float t = 0.0;
    for (int i = 0; i < 80; i++) {
        if (t > 40.0) break;
        vec3 p = ro + rd * t;
        float r = length(p);
        if (r < 0.01) { t += 0.1; continue; }
        vec3 sp = vec3(log(r) - time * 0.4, exp(r - p.z / r) * 0.2, time * 0.2 - atan(p.x, p.y));
        float e = sp.y - 0.5 + F(sp.xz * 2.0, r);
        float df = max(abs(e) * 0.15, 0.002 * t);
        if (df < 0.04) {
            float dens = exp(-df * 30.0);
            vec3 c = vec3(sin(sp.z + time), sin(sp.z + time + 2.0), sin(sp.z + time + 4.0)) * 0.5 + 0.5;
            c = mix(c, vec3(0.1, 0.4, 0.8), F(p.xy * 4.0, time)) * exp(-r * 0.15) * 0.04 * dens;
            fragColor.rgb += c; fragColor.a += dens * 0.02;
            if (fragColor.a > 0.95) { fragColor.a = 1.0; break; }
        }
        t += max(df * 0.6, 0.02);
    }
    fragColor.rgb += vec3(0.005, 0.01, 0.02) * (1.0 - exp(-t * 0.05));
    return fragColor;
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec3 color = vec3(0.0);
    for (int m = 0; m < AA; m++) {
        for (int n = 0; n < AA; n++) {
            vec2 offset = vec2(float(m), float(n)) / float(AA) - 0.5;
            color += render(fragCoord + offset, iResolution.xy, iTime).rgb;
        }
    }
    color = clamp((color /= float(AA * AA)) * 1.4 * (2.51 * color * 1.4 + 0.03) / (color * 1.4 * (2.43 * color * 1.4 + 0.59) + 0.14), 0.0, 1.0);
    fragColor = vec4(pow(color, vec3(1.0 / 2.2)), 1.0);
}