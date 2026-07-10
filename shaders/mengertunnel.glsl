// mengertunnel
// Shadertoy ID: 7cfXWr
// Description: tunnel
// Tags: tunnel

#define ITERATIONS 6
#define MAX_STEPS 100
#define SURF_DIST 0.001
#define MAX_DIST 40.0
#define COLOR_A vec3(4.0, 0.1, 0.8)
#define COLOR_B vec3(0.1, 3.0, 4.0)

float g_glow = 0.0;

mat2 rot(float a) {
    float s = sin(a), c = cos(a);
    return mat2(c, -s, s, c);
}

float sdMenger(vec3 p) {
    float d = length(max(abs(p) - vec3(12.0, 12.0, 50.0), 0.0));
    float s = 1.0;
    for(int m = 0; m < ITERATIONS; m++) {
        vec3 a = mod(p * s, 2.0) - 1.0;
        s *= 3.0;
        vec3 r = abs(1.0 - 3.0 * abs(a));
        float da = max(r.x, r.y);
        float db = max(r.y, r.z);
        float dc = max(r.z, r.x);
        float c = (min(da, min(db, dc)) - 1.0) / s;
        d = max(d, c);
    }
    g_glow += 0.012 / (0.01 + d*d*400.0);
    return d;
}

float sdCross(vec3 p) {
    p = abs(mod(p, 2.0) - 1.0);
    float d = max(p.x, max(p.y, p.z)) - 0.75;
    float d2 = min(max(p.x, p.y), min(max(p.y, p.z), max(p.x, p.z))) - 0.12;
    float res = max(d, -d2);
    g_glow += 0.008 / (0.01 + res*res*200.0);
    return res;
}

float map(vec3 p, float t) {
    p.xy *= rot(p.z * 0.12 + iTime * 0.3);
    float zRep = 4.0;
    vec3 pFract = p;
    pFract.z = mod(p.z, zRep) - zRep * 0.5;
    g_glow = 0.0;
    return mix(sdMenger(pFract), sdCross(pFract), t);
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    vec3 ro = vec3(0.0, 0.0, iTime * 4.0);
    vec3 rd = normalize(vec3(uv, 1.0));
    
    float t = smoothstep(0.15, 0.85, abs(sin(iTime * (3.14159 / 4.0))));
    float morphFlash = pow(sin(t * 3.14159), 2.0) * 1.8;
    
    float dO = 0.0;
    float current_glow = 0.0;
    
    for(int i = 0; i < MAX_STEPS; i++) {
        float dS = map(ro + rd * dO, t);
        dO += dS;
        current_glow += g_glow; 
        if(dO > MAX_DIST || abs(dS) < SURF_DIST) break;
    }
    
    vec3 glowColor = mix(COLOR_A, COLOR_B, sin(iTime * 0.5) * 0.5 + 0.5);
    glowColor *= (1.0 + morphFlash);
    
    vec3 col = (dO < MAX_DIST) ? glowColor * 0.01 : vec3(0.0);
    col += glowColor * current_glow * (0.0006 + morphFlash * 0.0004);
    col *= exp(-0.06 * dO);
    
    col += pow(col, vec3(2.2)) * (0.6 + morphFlash);
    col = smoothstep(-0.05, 1.2, col);
    col = pow(col, vec3(0.4545));
    col *= 1.0 - smoothstep(0.4, 1.6, length(uv));

    fragColor = vec4(col, 1.0);
}