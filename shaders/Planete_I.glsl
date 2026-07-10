// Planete I
// Shadertoy ID: ffSSRc
// Description: Planete I
// Tags: planet

#define MAX_STEPS 160
#define SURFACE_DIST 0.0004
#define MAX_DIST 40.0
#define SHADOW_ITERATIONS 48

mat2 rot(float a) {
    float s = sin(a), c = cos(a);
    return mat2(c, -s, s, c);
}

float hash13(vec3 p3) {
    p3 = fract(p3 * .1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

float sdBox(vec3 p, vec3 b) {
    vec3 q = abs(p) - b;
    return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
}

vec3 rotateObject(vec3 p) {
    float t = iTime * 0.025;
    p.yz *= rot(t * 0.31);
    p.xz *= rot(t);
    return p;
}

float greeble(vec3 p) {
    float d = 0.0;
    float amp = 0.035;
    float freq = 6.0;
    for(int i = 0; i < 5; i++) {
        vec3 id = floor(p * freq);
        float h = hash13(id + float(i) * 168.41);
        vec3 f = fract(p * freq) - 0.5;
        float s = sdBox(f, vec3(0.12 + h * 0.35, 0.06, 0.12 + h * 0.35));
        d += smoothstep(0.015, -0.015, s) * amp * step(0.35, h);
        p.xy *= rot(0.785);
        p.zx *= rot(0.4);
        freq *= 1.85;
        amp *= 0.48;
    }
    return d;
}

float map(vec3 p) {
    p = rotateObject(p);
    
    float base = length(p) - 1.0;
    
    float p_norm = pow(pow(abs(p.x), 8.0) + pow(abs(p.y), 8.0) + pow(abs(p.z), 8.0), 1.0/8.0);
    float panels = sin(p_norm * 45.0) * 0.002;
    
    float equatorialTrench = max(base - 0.015, abs(p.y) - 0.004);
    float polarTrench = max(base - 0.015, abs(p.x) - 0.004);
    
    vec3 dishDir = normalize(vec3(0.5, 0.4, -0.6));
    float dToCenter = length(p - dishDir * 0.92);
    vec3 pD = p - dishDir * 1.04;
    float aD = length(pD - dot(pD, dishDir) * dishDir);
    float dishSDF = max(dot(pD, dishDir) + (aD * aD) * 4.2, dToCenter - 0.38);
    
    float d = max(base - panels, -equatorialTrench);
    d = max(d, -polarTrench);
    d = max(d, -dishSDF);
    
    return (d - greeble(p)) * 0.65;
}

vec3 getNormal(vec3 p) {
    vec2 e = vec2(0.0008, 0);
    return normalize(vec3(
        map(p + e.xyy) - map(p - e.xyy),
        map(p + e.yxy) - map(p - e.yxy),
        map(p + e.yyx) - map(p - e.yyx)
    ));
}

float softShadow(vec3 ro, vec3 rd, float k) {
    float res = 1.0;
    float t = 0.01;
    for(int i = 0; i < SHADOW_ITERATIONS; i++) {
        float h = map(ro + rd * t);
        res = min(res, k * h / t);
        t += clamp(h, 0.005, 0.15);
        if(res < 0.002 || t > 5.0) break;
    }
    return clamp(res, 0.0, 1.0);
}

float getAO(vec3 p, vec3 n) {
    float occ = 0.0;
    float sca = 1.0;
    for(int i = 0; i < 5; i++) {
        float h = 0.01 + 0.12 * float(i) / 4.0;
        float d = map(p + h * n);
        occ += (h - d) * sca;
        sca *= 0.95;
    }
    return clamp(1.0 - 3.0 * occ, 0.0, 1.0);
}

vec3 triplanar(sampler2D tex, vec3 p, vec3 n) {
    vec3 m = pow(abs(n), vec3(12.0));
    m /= (m.x + m.y + m.z);
    vec3 x = texture(tex, p.yz).rgb;
    vec3 y = texture(tex, p.zx).rgb;
    vec3 z = texture(tex, p.xy).rgb;
    return x * m.x + y * m.y + z * m.z;
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    vec3 ro = vec3(0, 0, -2.4);
    vec3 rd = normalize(vec3(uv, 2.5));
    
    float t = 0.0, d;
    for(int i = 0; i < MAX_STEPS; i++) {
        d = map(ro + rd * t);
        if(d < SURFACE_DIST || t > MAX_DIST) break;
        t += d;
    }
    
    vec3 bg = vec3(pow(hash13(floor(rd * 650.0)), 600.0));
    bg += vec3(0.02, 0.03, 0.05) * (1.0 - length(uv));
    vec3 col = bg;
    
    if(t < MAX_DIST) {
        vec3 p = ro + rd * t;
        vec3 n = getNormal(p);
        vec3 v = -rd;
        vec3 l = normalize(vec3(1.2, 0.9, -1.0));
        vec3 h = normalize(l + v);
        
        vec3 localP = rotateObject(p);
        vec3 localN = rotateObject(n);
        
        float sh = softShadow(p, l, 24.0);
        float ao = getAO(p, n);
        float dif = max(dot(n, l), 0.0);
        float spe = pow(max(dot(n, h), 0.0), 64.0) * dif * sh;
        float fre = pow(clamp(1.0 + dot(rd, n), 0.0, 1.0), 5.0);
        
        vec3 texCol = triplanar(iChannel0, localP * 0.8, localN);
        vec3 alb = mix(vec3(0.28, 0.3, 0.32), vec3(0.15, 0.16, 0.18), greeble(localP) * 15.0) * texCol;
        
        float cityMask = step(0.9982, hash13(floor(localP * 550.0))) * step(0.01, greeble(localP));
        vec3 emissive = vec3(0.4, 0.7, 1.0) * cityMask * 60.0 * (1.0 - dif);
        
        col = alb * dif * sh;
        col += alb * 0.04 * ao;
        col += vec3(0.8, 0.9, 1.0) * spe * 1.5;
        col += vec3(0.3, 0.4, 0.5) * fre * 0.6 * ao;
        col += emissive;
        
        col = mix(col, bg, smoothstep(5.0, MAX_DIST, t));
    }
    
    col = mix(col, vec3(dot(col, vec3(0.2126, 0.7152, 0.0722))), -0.1);
    col = pow(col, vec3(1.0 / 2.2));
    col = smoothstep(0.0, 1.0, col);
    col *= 1.0 - dot(uv, uv) * 0.25;
    
    fragColor = vec4(col, 1.0);
}