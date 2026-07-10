// LiquidFractWater
// Shadertoy ID: sXBXWt
// Description: LiquidFractWater
// Tags: fractal

#define R(a) mat2(cos(a), sin(a), -sin(a), cos(a))

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 r = iResolution.xy;
    vec2 uv = (fragCoord - 0.5 * r) / r.y;
    float t = iTime;

    vec3 ro = vec3(2.5 * sin(t * 0.15), -3.0 + t * 0.2, 3.5 + 0.8 * cos(t * 0.25));
    vec3 lookAt = vec3(0.0, t * 0.2 + 2.0, 0.0);
    
    vec3 fwd = normalize(lookAt - ro);
    vec3 rgt = normalize(cross(vec3(0.0, 0.0, 1.0), fwd));
    vec3 up = cross(fwd, rgt);
    vec3 rd = normalize(uv.x * rgt + uv.y * up + 1.2 * fwd);

    float d = (0.0 - ro.z) / rd.z;
    if (rd.z >= 0.0) d = 0.0;

    vec3 p3d = ro + rd * d;
    vec2 p = p3d.xy * 0.15;
    vec2 q, n = vec2(0.0);
    float s = 3.3, h = 1.0, e = 110.0;
    float L = dot(p, p);

    p *= R(t * 0.03);

    for(float i = 1.0; i < e; i++) {
        p *= R(1.17);
        n *= R(8.65);
        q = p * s + n + vec2(t * 0.91, -t * 1.00);
        h += dot(vec2(1.0), sin(q)) / s;
        n -= cos(q) * 1.00;
        s *= 1.04;
    }

    vec3 N = normalize(vec3(n * 0.08, 1.0));
    vec3 Ld = normalize(vec3(cos(t * 0.3), sin(t * 0.3), 1.0));
    vec3 V = -rd;
    vec3 H = normalize(Ld + V);

    float diff = max(dot(N, Ld), 0.0) + 0.2;
    float spec = pow(max(dot(N, H), 0.0), 107.1);
    float fres = pow(1.0 - max(dot(N, V), 0.0), 5.0);

    vec3 albedo = 0.18 + 1.00 * cos(h * -4.1 + vec3(0.7, -4.4, 9.5) + t * 1.0);
    vec3 col = albedo * (diff * 0.8) + vec3(spec * 0.7) + vec3(fres * 0.3);
    
    col *= smoothstep(20.0, 2.0, length(p3d.xy));
    col = mix(vec3(0.01, 0.02, 0.05), col, exp(-0.015 * d * d));

    fragColor = vec4(pow(max(col, 0.0), vec3(1.0)), 1.0);
}