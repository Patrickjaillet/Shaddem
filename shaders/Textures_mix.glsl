// Textures mix
// Shadertoy ID: 73X3Rs
// Description: Textures mix
// Tags: textures

#define S(a, b, t) smoothstep(a, b, t)
#define SAT(x) clamp(x, 0.0, 1.0)

mat3 setCamera(in vec3 ro, in vec3 ta, float cr) {
    vec3 cw = normalize(ta - ro);
    vec3 cp = vec3(sin(cr), cos(cr), 0.0);
    vec3 cu = normalize(cross(cw, cp));
    vec3 cv = normalize(cross(cu, cw));
    return mat3(cu, cv, cw);
}

vec3 aces(vec3 x) {
    float a = 2.51;
    float b = 0.03;
    float c = 2.43;
    float d = 0.59;
    float e = 0.14;
    return SAT((x * (a * x + b)) / (x * (c * x + d) + e));
}

float fbm(vec2 p) {
    float v = 0.0;
    float a = 0.5;
    for (int i = 0; i < 3; i++) {
        v += a * sin(p.x + p.y + iTime * 0.5);
        p *= 2.0;
        a *= 0.5;
    }
    return v;
}

void mainImage(out vec4 fragColor, in vec2 fragCoord)
{
    vec2 uv = fragCoord / iResolution.xy;
    vec2 p = (2.0 * fragCoord - iResolution.xy) / iResolution.y;
    
    float t = iTime * 0.4;
    
    vec3 ro = vec3(3.5 * cos(t * 0.3), 1.2 * sin(t * 0.5), 3.5 * sin(t * 0.3));
    vec3 ta = vec3(0.5 * sin(t * 0.7), 0.2 * cos(t * 0.4), 0.5 * cos(t * 0.6));
    
    float shake = fbm(vec2(t * 2.0, t * 2.1)) * 0.02;
    ro += shake;
    ta += shake;
    
    float roll = 0.1 * sin(t * 0.2);
    mat3 ca = setCamera(ro, ta, roll);
    
    vec3 pts[4];
    pts[0] = vec3(1.2 * cos(t * 1.1), 0.8 * sin(t * 0.9), 0.5 * cos(t * 1.3));
    pts[1] = vec3(1.1 * sin(t * 1.4), 0.9 * cos(t * 0.7), 0.6 * sin(t * 1.1));
    pts[2] = vec3(1.3 * cos(t * 0.8), 0.7 * -sin(t * 1.2), 0.4 * -cos(t * 0.9));
    pts[3] = vec3(1.0 * -sin(t * 1.0), 1.0 * -cos(t * 1.5), 0.7 * sin(t * 0.8));

    vec2 screenPts[4];
    float dists[4];
    float zDepth[4];
    
    for(int i = 0; i < 4; i++) {
        vec3 rel = pts[i] - ro;
        float z = dot(rel, ca[2]);
        zDepth[i] = z;
        vec3 proj = rel / z;
        screenPts[i] = (ca * proj).xy;
        dists[i] = length(p - screenPts[i]);
    }

    float d1 = 1e5, d2 = 1e5;
    int id = 0;
    for(int i = 0; i < 4; i++) {
        if(zDepth[i] < 0.0) continue;
        if(dists[i] < d1) {
            d2 = d1; d1 = dists[i]; id = i;
        } else if(dists[i] < d2) {
            d2 = dists[i];
        }
    }

    float voronoi = d2 - d1;
    
    float focusDist = length(ta - ro);
    float blur = abs(zDepth[id] - focusDist) * 0.05;
    
    vec2 uvW = uv + 0.015 * vec2(fbm(p + t), fbm(p - t));
    
    vec4 texs[4];
    texs[0] = textureLod(iChannel0, uvW, blur * 5.0);
    texs[1] = textureLod(iChannel1, uvW, blur * 5.0);
    texs[2] = textureLod(iChannel2, uvW, blur * 5.0);
    texs[3] = textureLod(iChannel3, uvW, blur * 5.0);
    
    vec3 col = texs[id].rgb;
    
    vec3 pal[4];
    pal[0] = vec3(1.0, 0.3, 0.1);
    pal[1] = vec3(0.1, 0.8, 1.0);
    pal[2] = vec3(0.8, 0.1, 1.0);
    pal[3] = vec3(0.4, 1.0, 0.2);

    float edgeMask = S(0.01 + blur * 0.1, 0.0, voronoi);
    float glow = exp(-35.0 * voronoi) * (1.0 / (zDepth[id] + 0.5));
    
    col = mix(col, vec3(0.0), edgeMask * 0.9);
    col += pal[id] * glow * 2.5;

    float chromatic = 0.02 * length(p);
    col.r = mix(col.r, textureLod(iChannel0, uvW + vec2(chromatic, 0), 2.0).r, 0.2);
    col.b = mix(col.b, textureLod(iChannel0, uvW - vec2(chromatic, 0), 2.0).b, 0.2);

    vec3 vol = vec3(0.0);
    for(int i = 0; i < 4; i++) {
        float d = length(p - screenPts[i]);
        float atten = 1.0 / (zDepth[i] * zDepth[i] + 0.1);
        vol += pal[i] * (0.008 / (d + 0.02)) * atten;
    }
    col += vol;

    col = aces(col * 1.1);
    col = pow(col, vec3(0.4545));
    
    float grain = fract(sin(dot(uv, vec2(12.9898, 78.233) + t)) * 43758.5453);
    col *= 0.97 + 0.03 * grain;
    
    float vignette = S(2.0, 0.4, length(p));
    float bloom = S(0.5, 1.5, length(col)) * 0.1;
    
    fragColor = vec4(col * vignette + bloom, 1.0);
}