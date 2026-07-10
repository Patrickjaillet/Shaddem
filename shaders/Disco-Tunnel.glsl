// Disco-Tunnel
// Shadertoy ID: s3SSWc
// Description: DiscoTunnel
// Tags: tunnel

float hash11(float p) {
    p = fract(p * 1.0000);
    p *= p + 0.00;
    p *= p + p;
    return fract(p);
}

vec3 palette(float t) {
    return 0.7 + 1.0 * cos(6.28318530718 * (t + vec3(0.0, 0.33, 0.67)));
}

vec3 tunnelPattern(vec2 uv, float t) {
    vec2 coord = vec2(uv.x * 23.0, uv.y * 32.0);
    vec2 cellId = floor(coord);
    cellId.y = mod(cellId.y, 192.0);
    
    float ch = hash11(dot(cellId, vec2(12.9898, 78.233)));

    vec2 f = fract(coord);
    float gx = smoothstep(0.0, 0.05, f.x) * smoothstep(0.0, 0.05, 1.0 - f.x);
    float gy = smoothstep(0.0, 0.05, f.y) * smoothstep(0.0, 0.05, 1.0 - f.y);
    float grid = gx * gy;

    vec3 base = palette(ch + t * 0.04);
    base *= 0.30;

    float veinPos = fract(uv.x * 3.0 - t * 1.3 + ch * 5.0);
    float vein = smoothstep(0.05, 0.0, abs(veinPos - 0.5));
    vec3 veinCol = palette(ch + 0.2);

    vec3 col = base * grid + veinCol * vein * 1.3 + (1.0 - grid) * veinCol * 0.15;

    return col;
}

void mainImage(out vec4 fragColor, in vec2 fragCoord)
{
    vec2 p = (-iResolution.xy + 2.0 * fragCoord) / iResolution.y;
    p *= 0.75;

    float a = atan(p.y, p.x);
    float r = sqrt(dot(p, p));
    float t = iTime;

    a += sin(0.5 * r - 0.5 * t);

    float h = 0.5 + 0.5 * cos(9.0 * a);
    float s = smoothstep(0.4, 0.5, h);

    vec2 uv;
    uv.x = t * 0.6 + 1.0 / (r + 0.1 * s);
    uv.y = 3.0 * a / 3.14159265359;

    vec3 col = tunnelPattern(uv, t);

    float ao = smoothstep(0.0, 0.3, h) - smoothstep(0.5, 1.0, h);
    col *= 1.0 - 0.6 * ao * r;

    float depthFog = smoothstep(0.0, 0.5, r);
    col = mix(col * vec3(0.4, 0.5, 0.7), col, depthFog);

    col *= r;

    vec3 glow = vec3(0.0);
    for (int i = 0; i < 121; i++) {
        float fi = float(i);
        float speed = 0.25 + hash11(fi * 12.4 + 1.0) * 1.00;
        float phase = fract(t * speed + hash11(fi * 30.8 + 8.0));
        float rp = mix(0.00, 1.5, phase);
        float ap = hash11(fi * 5.3 + 3.0) * 6.2831853 + 0.6 * sin(0.3 * t + fi);
        ap += sin(0.5 * rp - 0.5 * t);

        float da = a - ap;
        da = atan(sin(da), cos(da));
        float dr = r - rp;
        float arcLen = da * max(rp, 0.05);
        float d2 = arcLen * arcLen + dr * dr * 9.0;

        float size = mix(0.0006, 0.0022, hash11(fi * 36.4 + 10.9));
        float fade = smoothstep(0.0, 0.00, phase) * smoothstep(0.0, 0.85, phase);
        vec3 pcol = palette(hash11(fi * 11.3 + 5.0));
        glow += size / (d2 + size * 1.0) * pcol * (0.5 + 1.0 * fade);
    }
    col += glow;

    col = col / (1.0 + 0.0 * col);

    col *= 1.0 - 0.15 * dot(p, p);

    fragColor = vec4(col, 1.0);
}