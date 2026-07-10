// Drone I
// Shadertoy ID: sfjXRD
// Description: Drone I
// Tags: drone

#define T iTime
#define rot(a) mat2(cos(a+vec4(0,33,11,0)))
#define MAX_DIST 40.0
#define SURF_DIST 0.001

float sdBo(vec3 p, vec3 b, float r) {
    vec3 q = abs(p) - b;
    return length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0) - r;
}

float map(vec3 p) {
    vec3 bP = vec3(0, 4.0 + sin(T*1.2)*0.5, 0);
    bP.xz += vec2(sin(T*0.5)*2.0, cos(T*0.4)*2.0);
    float d = length(p - bP) - 1.2;
    for(int i=0; i<6; i++) {
        float fi = float(i);
        vec3 oP = bP;
        float ang = T + fi * 1.047;
        oP += vec3(cos(ang)*2.5, sin(fi+T)*0.5, sin(ang)*2.5);
        vec3 pr = p - oP;
        pr.xz *= rot(ang);
        pr.xy *= rot(fi + T * 0.2);
        d = min(d, sdBo(pr, vec3(0.2, 0.8, 0.6), 0.1));
    }
    vec3 eP = bP + vec3(0, 0, 1.1);
    eP.xz *= rot(sin(T*0.5)*0.5);
    d = min(d, length(p - eP) - 0.4);
    return d;
}

vec3 getNormal(vec3 p) {
    vec2 e = vec2(SURF_DIST, 0);
    return normalize(vec3(
        map(p + e.xyy) - map(p - e.xyy),
        map(p + e.yxy) - map(p - e.yxy),
        map(p + e.yyx) - map(p - e.yyx)
    ));
}

vec3 getEnvironment(vec3 rd) {
    vec3 col = vec3(0);
    vec3 p = rd;
    float s = 0.5;
    for(int i = 0; i < 8; i++) {
        p = abs(p) / dot(p, p) - s;
        p.xy *= rot(T * 0.2 + float(i));
        p.yz *= rot(T * 0.15);
        
        vec3 colorShift = 0.5 + 0.5 * cos(vec3(0, 2, 4) + T * 0.5 + float(i) * 0.8);
        col += exp(-length(p) * 3.5) * colorShift * 0.4;
        col += exp(-length(p) * 7.0) * vec3(1.0, 0.5, 0.2) * 0.2;
        s += 0.04;
    }
    
    float aurora = pow(max(0.0, 1.0 - abs(rd.y - 0.2)), 10.0);
    col += aurora * vec3(0.1, 0.8, 0.4) * (0.5 + 0.5 * sin(T + rd.x * 3.0));
    
    return col * 0.5;
}

float rayMarch(vec3 ro, vec3 rd) {
    float dO = 0.0;
    for(int i=0; i<128; i++) {
        vec3 p = ro + rd * dO;
        float dS = map(p);
        dO += dS;
        if(dO > MAX_DIST || abs(dS) < SURF_DIST) break;
    }
    return dO;
}

vec3 shade(vec3 ro, vec3 rd) {
    float d = rayMarch(ro, rd);
    
    if(d > MAX_DIST) return getEnvironment(rd);

    vec3 p = ro + rd * d;
    vec3 n = getNormal(p);
    vec3 r = reflect(rd, n);
    
    float dRefl = rayMarch(p + n * SURF_DIST * 10.0, r);
    vec3 reflectionCol;
    
    if(dRefl < MAX_DIST) {
        vec3 p2 = p + r * dRefl;
        vec3 n2 = getNormal(p2);
        vec3 r2 = reflect(r, n2);
        reflectionCol = getEnvironment(r2) * 0.3;
    } else {
        reflectionCol = getEnvironment(r);
    }

    float fresnel = pow(clamp(1.0 + dot(rd, n), 0.0, 1.0), 5.0);
    float occ = clamp(map(p + n * 0.4) / 0.4, 0.0, 1.0);
    
    vec3 albedo = vec3(0.005, 0.005, 0.01);
    vec3 col = mix(albedo, reflectionCol, 0.3 + fresnel * 0.7);
    
    vec3 bP = vec3(0, 4.0 + sin(T*1.2)*0.5, 0);
    bP.xz += vec2(sin(T*0.5)*2.0, cos(T*0.4)*2.0);
    vec3 eyePos = bP + vec3(0, 0, 1.1);
    float eyeGlow = 0.08 / (length(p - eyePos) + 0.01);
    col += vec3(0.0, 0.9, 1.0) * eyeGlow;
    
    return col * occ;
}

void mainImage( out vec4 fragColor, in vec2 fragCoord ) {
    vec2 p = (fragCoord - iResolution.xy * 0.5) / iResolution.y;
    
    float r = length(p);
    float a = atan(p.y, p.x);
    float fish = pow(r, 1.5) * 1.2;
    vec2 uv = vec2(cos(a), sin(a)) * fish;

    vec3 ro = vec3(0, 5, -12);
    vec3 ta = vec3(0, 4, 0);
    vec3 cw = normalize(ta-ro);
    vec3 cu = normalize(cross(cw,vec3(0,1,0)));
    vec3 cv = cross(cu,cw);
    
    vec3 rd = mat3(cu, cv, cw) * normalize(vec3(uv, 2.0));

    vec3 col = shade(ro, rd);

    col = pow(col, vec3(0.4545));
    fragColor = vec4(tanh(col * 1.8), 1.0);
}