// Etoile II
// Shadertoy ID: NfsXRX
// Description: Etoile
// Tags: star

#define MAX_STEPS 160
#define ITERATIONS 5
#define SURF_DIST 0.001
#define MAX_DIST 40.0

mat2 rot(float a) {
    float s = sin(a), c = cos(a);
    return mat2(c, -s, s, c);
}

float fractal(vec3 p, float pulse) {
    float s = 1.0;
    p.xz *= rot(iTime * 0.4);
    p.yz *= rot(iTime * 0.3);
    
    for(int i = 0; i < ITERATIONS; i++) {
        p = abs(p) - vec3(0.5, 1.2, 0.8) * (1.0 + 0.1 * pulse);
        if (p.x < p.y) p.xy = p.yx;
        if (p.x < p.z) p.xz = p.zx;
        if (p.y < p.z) p.yz = p.zy;
        
        float k = 1.8 / clamp(dot(p, p), 0.15, 1.0);
        p *= k;
        s *= k;
        p -= vec3(0.4, 0.5, 0.2);
    }
    return length(p.xyz) / s;
}

vec2 map(vec3 p, float pulse) {
    float f = fractal(p, pulse);
    float tube = length(p.xy) - 0.2;
    return vec2(max(f, -tube), 1.0);
}

vec3 getNormal(vec3 p, float pulse) {
    vec2 e = vec2(0.002, 0);
    return normalize(map(p, pulse).x - vec3(
        map(p - e.xyy, pulse).x,
        map(p - e.yxy, pulse).x,
        map(p - e.yyx, pulse).x
    ));
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    float t = iTime;
    float pulse = sin(t * 2.5) * 0.5 + 0.5;
    
    vec3 ro = vec3(0, 0, -4.0);
    vec3 rd = normalize(vec3(uv, 1.5));
    rd.xy *= rot(t * 0.1);

    float d = 0.0, glow = 0.0;
    vec2 res;
    
    for(int i = 0; i < MAX_STEPS; i++) {
        vec3 p = ro + rd * d;
        res = map(p, pulse);
        glow += exp(-res.x * 15.0) * (0.1 + 0.2 * pulse);
        if(abs(res.x) < SURF_DIST || d > MAX_DIST) break;
        d += res.x * 0.6;
    }

    vec3 c1 = 0.5 + 0.5 * cos(t * 0.4 + vec3(0, 2, 4));
    vec3 c2 = 0.5 + 0.5 * cos(t * 0.7 + vec3(3, 1, 5));
    vec3 col = mix(c1 * 0.1, c2 * 0.1, length(uv));

    if(d < MAX_DIST) {
        vec3 p = ro + rd * d;
        vec3 n = getNormal(p, pulse);
        vec3 sunDir = normalize(vec3(1, 2, -1));
        
        float diff = max(dot(n, sunDir), 0.0);
        float spec = pow(max(dot(reflect(rd, n), sunDir), 0.0), 32.0);
        float fres = pow(1.0 + dot(rd, n), 4.0);
        
        vec3 objCol = 0.5 + 0.5 * cos(p.z + vec3(0, 2, 4) + t);
        col = objCol * (diff + 0.2) + spec + fres * c1;
        col = mix(col, c2 * 0.2, 1.0 - exp(-0.05 * d));
    }

    col += glow * 0.04 * c1;
    col += (0.05 * pulse / length(uv)) * c2;
    
    fragColor = vec4(pow(clamp(col, 0.0, 1.0), vec3(0.4545)), 1.0);
}