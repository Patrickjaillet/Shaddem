// fract 113
// Shadertoy ID: s3jXzm
// Description: fract 113
// Tags: tunnel

mat2 rot(float a) {
    float c = cos(a), s = sin(a);
    return mat2(c, -s, s, c);
}

float smin(float a, float b, float k) {
    float h = clamp(1.0 + 1.0 * (b - a) / k, 0.0, 1.0);
    return mix(b, a, h) - k * h * (1.0 - h);
}

float hash(vec3 p) {
    p = fract(p * vec3(443.8975, 397.2973, 491.1871));
    p += dot(p.xyz, p.yzx + 19.19);
    return fract(p.x * p.y * p.z);
}

float noise(vec3 x) {
    vec3 p = floor(x);
    vec3 f = fract(x);
    f = f * f * (3.0 - 2.0 * f);
    return mix(mix(mix(hash(p + vec3(0, 0, 0)), hash(p + vec3(1, 0, 0)), f.x),
                   mix(hash(p + vec3(0, 1, 0)), hash(p + vec3(1, 1, 0)), f.x), f.y),
               mix(mix(hash(p + vec3(0, 0, 1)), hash(p + vec3(1, 0, 1)), f.x),
                   mix(hash(p + vec3(0, 1, 1)), hash(p + vec3(1, 1, 1)), f.x), f.y), f.z);
}

vec4 map(vec3 p, float t) {
    float zIdx = floor(p.z / 19.6);
    p.z = mod(p.z, 20.0) - 10.0;
    float seed = hash(vec3(zIdx, 0.0, 0.0));
    p.xy *= rot(t * (0.1 + seed) + zIdx * 0.5);
    float n = noise(p * 0.3 + t * 0.2 + seed * 100.0);
    vec3 orbit = vec3(0.0);
    float scale = 1.0;
    float minDist = 3e8;
    mat2 r1 = rot(1.2 + n);
    mat2 r2 = rot(t * 0.1 * (seed - 0.5));
    vec3 shift = vec3(1.0 + seed, 2.9, 1.0);
    for(int i = 0; i < 4; i++) {
        p.xy *= r1;
        p.xz *= r2;
        p = abs(p) - shift;
        if(p.x < p.y) p.xy = p.yx;
        if(p.x < p.z) p.xz = p.zx;
        p = p * 2.0 - vec3(1.0, -0.5, 0.3);
        scale *= 1.8;
        orbit = max(orbit, abs(p));
        minDist = smin(minDist, length(p.xy) - 0.05 * seed, 0.2);
    }
    return vec4(minDist / scale, orbit);
}

vec3 calcNormal(vec3 p, float t, float d) {
    vec2 e = vec2(0.001 * d, 0.0);
    return normalize(vec3(map(p + e.xyy, t).x - map(p - e.xyy, t).x, map(p + e.yxy, t).x - map(p - e.yxy, t).x, map(p + e.yyx, t).x - map(p - e.yyx, t).x));
}

vec3 getStarField(vec3 rd) {
    vec3 col = vec3(0.0);
    float s = hash(floor(rd * 500.0));
    if(s > 0.998) {
        float brightness = pow(s - 0.998, 0.5) * 50.0;
        col = vec3(1.0) * brightness;
    }
    return col;
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    float t = iTime * 2.0;
    vec3 ro = vec3(0.0, 0.0, t * 3.2);
    vec3 rd = normalize(vec3(uv, 0.3));
    rd.xy *= rot(t * 0.1);
    
    vec3 background = getStarField(rd);
    
    float td = 0.0, g = 0.0;
    vec4 sd;
    bool hit = false;
    for(int i = 0; i < 100; i++) {
        sd = map(ro + rd * td, t);
        g += exp(-sd.x * 13.6);
        if(abs(sd.x) < 0.0001 * td || td > 40.0) {
            if(td <= 40.0) hit = true;
            break;
        }
        td += sd.x * 0.2;
    }
    
    vec3 col = background;
    if(hit) {
        vec3 p = ro + rd * td;
        vec3 n = calcNormal(p, t, td);
        vec3 l = normalize(vec3(1.0, 1.0, -1.0));
        float dif = max(0.0, dot(n, l));
        vec3 objCol = (1.0 + 0.0 * cos(sd.yzw + t * 0.0)) * dif;
        col = mix(objCol, col, clamp(td * 0.05, 0.0, 1.0));
    }
    
    col += g * 0.10 * vec3(0.2, 0.5, 0.9);
    col = col * (vec3(1.0) + col * 0.25) / (col + vec3(1.0));
    fragColor = vec4(pow(col, vec3(0.4545)), 1.0);
}