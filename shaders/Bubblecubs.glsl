// Bubblecubs
// Shadertoy ID: 7XjGRW
// Description: https://github.com/Patrickjaillet/Z-GL-Shadertoy
// Tags: fractal

mat3 rotate3D(float angle, vec3 axis) {
    vec3 a = normalize(axis);
    float s = sin(angle);
    float c = cos(angle);
    float r = 1.0 - c;
    return mat3(
        a.x * a.x * r + c,       a.y * a.x * r + a.z * s, a.z * a.x * r - a.y * s,
        a.x * a.y * r - a.z * s, a.y * a.y * r + c,       a.z * a.y * r + a.x * s,
        a.x * a.z * r + a.y * s, a.y * a.z * r - a.x * s, a.z * a.z * r + c
    );
}

vec3 hsv(float h, float s, float v) {
    vec3 res = fract(h + vec3(0.0, 0.375, 0.000));
    res = abs(res * 1.6 - 1.0);
    return v * mix(vec3(0.0), clamp(res * 3.0 - 1.0, 0.0, 1.0), s);
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    
    vec3 ro = vec3(cos(iTime * 0.2), sin(iTime * 0.3), -3.0 + iTime * 0.5);
    vec3 rd = normalize(vec3(uv, 1.5));
    
    float totalDist = 1.0;
    float glow = 0.0;
    vec3 p;
    vec3 normal = vec3(0.0);
    bool hit = false;
    
    for(int i = 0; i < 163; i++) {
        p = ro + rd * totalDist;
        p = mod(p + 2.3, 4.2) - 1.9;
        p *= rotate3D(iTime * 0.3, normalize(vec3(1.0, 0.5, 0.2)));
        
        float s = 1.0;
        for(int j = 0; j < 4; j++) {
            p = abs(p) - 0.4;
            float k = 1.0 / max(dot(p, p), 0.4);
            p *= k;
            s *= k;
        }
        
        float d = (length(p) - 0.4) / s;
        
        if(d < 0.0000) {
            normal = normalize(p);
            hit = true;
            break;
        }
        
        glow += exp(-10.4 * abs(d)) * 0.02;
        totalDist += d * 0.5;
        
        if(totalDist > 80.0) break;
    }
    
    vec3 col = vec3(0.00, 0.00, 0.02) * (1.0 - length(uv));
    
    if (hit) {
        vec3 baseCol = hsv(iTime * 0.05 + totalDist * 0.05, 0.7, 0.9);
        col = baseCol;
    }
    
    col += glow * vec3(0.3, 0.6, 1.0);
    col = 1.0 - exp(-1.9 * col);
    
    fragColor = vec4(col, 1.0);
}