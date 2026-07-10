// Rings II
// Shadertoy ID: 7cXXDN
// Description: Rings II
// Tags: rings

struct Material {
    vec3 albedo;
    float roughness;
    float metallic;
};

mat2 rot(float a) {
    float s = sin(a), c = cos(a);
    return mat2(c, -s, s, c);
}

float sdCylinder(vec3 p, vec2 h) {
    vec2 d = abs(vec2(length(p.xz), p.y)) - h;
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
}

float smin(float a, float b, float k) {
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    return mix(b, a, h) - k * h * (1.0 - h);
}

vec2 map(vec3 p, float PI) {
    float mId = 0.0;
    
    vec3 q = p;
    float cellSize = 12.0;
    q.xz = mod(p.xz + cellSize * 0.5, cellSize) - cellSize * 0.5;
    
    float t = iTime * 0.8;
    vec3 p1 = q;
    p1.xz *= rot(t * 0.5);
    p1.xy *= rot(t * 0.3);
    
    float d = length(vec2(length(p1.xz) - 1.8, p1.y)) - 0.08;
    
    float ang = atan(p1.z, p1.x);
    float sector = round(ang / (PI / 12.0));
    float a = sector * (PI / 12.0);
    vec3 pT = p1;
    pT.xz *= rot(-a);
    pT.x -= 1.8;
    float teeth = sdCylinder(pT.yxz, vec2(0.06, 0.08)) - 0.02;
    d = smin(d, teeth, 0.05);
    
    vec3 p2 = p1; p2.yz *= rot(t * 1.2);
    float ring2 = length(vec2(length(p2.xy) - 1.3, p2.z)) - 0.06;
    d = smin(d, ring2, 0.08);
    
    vec3 p3 = p2; p3.xz *= rot(t * 2.0);
    float ring3 = length(vec2(length(p3.yz) - 0.8, p3.x)) - 0.04;
    d = smin(d, ring3, 0.08);
    
    float core = length(p3) - 0.35;
    d = smin(d, core, 0.1);
    
    float ground = p.y + 2.0;
    if (ground < d) {
        d = ground;
        mId = 1.0;
    }
    
    return vec2(d, mId);
}

vec3 getNormal(vec3 p, float PI) {
    vec2 e = vec2(0.0005, 0.0);
    return normalize(vec3(
        map(p + e.xyy, PI).x - map(p - e.xyy, PI).x,
        map(p + e.yxy, PI).x - map(p - e.yxy, PI).x,
        map(p + e.yyx, PI).x - map(p - e.yyx, PI).x
    ));
}

float getShadow(vec3 ro, vec3 rd, float PI) {
    float res = 1.0;
    float t = 0.01;
    for (int i = 0; i < 80; i++) {
        float h = map(ro + rd * t, PI).x;
        res = min(res, 32.0 * h / t);
        t += clamp(h, 0.005, 0.5);
        if (res < 0.0001 || t > 25.0) break;
    }
    return clamp(res, 0.0, 1.0);
}

float D_GGX(float NoH, float roughness, float PI) {
    float alpha = roughness * roughness;
    float a2 = alpha * alpha;
    float d = (NoH * a2 - NoH) * NoH + 1.0;
    return a2 / (PI * d * d);
}

float G_SchlickGGX(float NoV, float NoL, float roughness) {
    float k = (roughness + 1.0);
    k = (k * k) / 8.0;
    float g1 = NoV / (NoV * (1.0 - k) + k);
    float g2 = NoL / (NoL * (1.0 - k) + k);
    return g1 * g2;
}

vec3 F_Schlick(float cosTheta, vec3 F0) {
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    vec4 settings = vec4(160.0, 80.0, 0.0008, 3.14159265359);
    float MAX_STEPS = settings.x;
    float MAX_DIST = settings.y;
    float SURF_DIST = settings.z;
    float PI = settings.w;
    
    float pathT = iTime * 0.15;
    vec3 ro = vec3(
        12.0 * sin(pathT), 
        3.5 + 2.5 * cos(pathT * 2.3), 
        12.0 * cos(pathT * 0.7)
    );
    
    float lookT = pathT + 0.2;
    vec3 ta = vec3(
        4.0 * sin(lookT * 1.5), 
        -0.5 + sin(iTime * 0.4), 
        4.0 * cos(lookT)
    );
    
    float roll = 0.2 * sin(pathT * 0.5);
    vec3 ww = normalize(ta - ro);
    vec3 cp = vec3(sin(roll), cos(roll), 0.0);
    vec3 uu = normalize(cross(ww, cp));
    vec3 vv = cross(uu, ww);
    vec3 rd = normalize(uv.x * uu + uv.y * vv + (1.5 + 0.4 * sin(iTime * 0.1)) * ww);

    float t = 0.0;
    vec2 res;
    for (int i = 0; i < int(MAX_STEPS); i++) {
        res = map(ro + rd * t, PI);
        if (abs(res.x) < (SURF_DIST * t) || t > MAX_DIST) break;
        t += res.x;
    }

    vec3 skyCol = vec3(0.005, 0.008, 0.015);
    vec3 col = skyCol;

    if (t < MAX_DIST) {
        vec3 pos = ro + rd * t;
        vec3 N = getNormal(pos, PI);
        vec3 V = -rd;
        
        Material mat;
        if (res.y < 0.5) {
            mat = Material(vec3(1.0, 0.78, 0.35), 0.12, 1.0);
        } else {
            mat = Material(vec3(0.04), 0.35, 0.0);
            float grid = smoothstep(0.015, 0.0, abs(sin(pos.x * 2.0) * sin(pos.z * 2.0)));
            mat.albedo += grid * 0.08;
        }

        vec3 L = normalize(vec3(2.5, 5.0, 1.5));
        vec3 H = normalize(V + L);
        float NoV = abs(dot(N, V)) + 1e-6;
        float NoL = clamp(dot(N, L), 0.0, 1.0);
        float NoH = clamp(dot(N, H), 0.0, 1.0);
        float LoH = clamp(dot(L, H), 0.0, 1.0);

        vec3 F0 = mix(vec3(0.04), mat.albedo, mat.metallic);
        vec3 F = F_Schlick(LoH, F0);
        float D = D_GGX(NoH, mat.roughness, PI);
        float G = G_SchlickGGX(NoV, NoL, mat.roughness);
        
        vec3 nominator = D * G * F;
        float denominator = 4.0 * NoV * NoL + 0.0001;
        vec3 specular = nominator / denominator;
        
        vec3 kS = F;
        vec3 kD = (vec3(1.0) - kS) * (1.0 - mat.metallic);
        
        float shadow = getShadow(pos + N * 0.005, L, PI);
        vec3 diffuse = kD * mat.albedo / PI;
        
        vec3 direct = (diffuse + specular) * NoL * shadow * vec3(1.6, 1.5, 1.3);
        vec3 ambient = vec3(0.02) * mat.albedo * (1.0 - mat.metallic);
        
        col = direct + ambient;
        
        float rim = pow(1.0 - NoV, 5.0);
        col += rim * skyCol * (1.0 - mat.roughness);
    }

    col = mix(col, skyCol, 1.0 - exp(-0.00005 * t * t * t));
    col = pow(col, vec3(0.4545));
    col = mix(col, vec3(dot(col, vec3(0.299, 0.587, 0.114))), -0.15);
    col = smoothstep(-0.07, 0.8, col);
    col *= 3.0 - 0.45 * dot(uv, uv);

    fragColor = vec4(col, 1.0);
}