// foret
// Shadertoy ID: sfBSRz
// Description: xcvxcv
// Tags: xcvxcv

// --- SHADERTOY AAA+ PRODUCTION : STOCHASTIC FOREST ---

#define MAX_STEPS 110
#define SURFACE_DIST .002
#define MAX_DIST 100.

// Paramètres tree.json
const vec3  BARK_TINT = vec3(0.808, 0.753, 0.024);
const vec3  LEAF_COL  = vec3(0.2, 0.4, 0.1);
const float LEAF_SIZE = 0.267;

// --- MATH UTILS ---
mat2 rot(float a) { float s=sin(a), c=cos(a); return mat2(c,-s,s,c); }
float hash12(vec2 p) {
	vec3 p3  = fract(vec3(p.xyx) * .1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

// --- CLOUDS FBM ---
float fbm(vec2 p) {
    float f = 0.5 * texture(iChannel1, p * 0.02).r;
    f += 0.25 * texture(iChannel1, p * 0.04).r;
    return f;
}

// --- GEOMETRY ---
float sdCapsule(vec3 p, vec3 a, vec3 b, float r) {
    vec3 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba)/dot(ba, ba), 0.0, 1.0);
    return length(pa - ba*h) - r * (1.0 - h * 0.3);
}

float sdLeafFlat(vec3 p, float size) {
    vec3 q = p;
    q.y += q.x * q.x * 0.4;
    float d = length(q - vec3(clamp(q.x, -size, size), 0, clamp(q.z, -size*0.4, size*0.4)));
    return d - 0.002;
}

// Logique d'un arbre unique (déplacé pour instanciation)
float getTreeSDF(vec3 p, float seed) {
    float d = 1e5;
    float t = iTime * 0.4 + seed * 10.0;
    
    // Tronc
    float trunk = sdCapsule(p, vec3(0, -0.2, 0), vec3(0, 4.5, 0), 0.22);
    d = min(d, trunk);

    // Branches & Feuilles
    for(int i=0; i<10; i++) {
        float fi = float(i);
        vec3 pb = p - vec3(0, 1.5 + fi * 0.4, 0);
        pb.xz *= rot(fi * 2.399 + seed); 
        pb.xy *= rot(0.8 + sin(t + fi) * 0.05); 
        
        float brLen = 2.5 - fi * 0.15;
        float branch = sdCapsule(pb, vec3(0), vec3(0, brLen, 0), 0.1 * (1.0 - fi/12.0));
        d = min(d, branch);

        if(fi > 1.0) {
            vec3 pl = pb - vec3(0, 1.5, 0);
            pl.xz *= rot(sin(t * 2.0 + fi));
            float leaves = sdLeafFlat(pl, LEAF_SIZE * 3.5);
            d = min(d, leaves);
        }
    }
    return d;
}

vec2 map(vec3 p) {
    // 1. Sol infini
    float ground = p.y + texture(iChannel1, p.xz * 0.05).r * 0.12;
    vec2 res = vec2(ground, 2.0);

    // 2. Forêt instanciée (Grille de 30+ arbres)
    vec2 gridPos = p.xz / 15.0; // Taille de la cellule
    vec2 id = floor(gridPos);
    vec3 pTree = p;
    
    // Jitter (placement aléatoire dans la cellule)
    float h = hash12(id);
    pTree.xz = (fract(gridPos) - 0.5) * 15.0;
    pTree.xz += (vec2(hash12(id), hash12(id+1.0)) - 0.5) * 8.0;
    pTree.xz *= rot(h * 6.28); // Rotation aléatoire de l'arbre

    float treeDist = getTreeSDF(pTree, h);
    if(treeDist < res.x) res = vec2(treeDist, 1.0);

    // 3. Herbe locale
    vec3 pg = p;
    pg.xz = (fract(pg.xz * 6.0) - 0.5);
    float grass = length(pg.xz) - 0.015 * (1.0 - p.y/0.3);
    grass = max(grass, p.y - 0.3);
    grass = max(grass, -p.y);
    if(grass < res.x) res = vec2(grass, 4.0);

    return res;
}

vec3 getNormal(vec3 p) {
    vec2 e = vec2(.001, 0);
    return normalize(vec3(map(p+e.xyy).x-map(p-e.xyy).x,
                          map(p+e.yxy).x-map(p-e.yxy).x,
                          map(p+e.yyx).x-map(p-e.yyx).x));
}

void mainImage( out vec4 fragColor, in vec2 fragCoord ) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    vec3 ro = vec3(0, 4.0, -25.0);
    vec3 rd = normalize(vec3(uv, 1.0));
    
    // Caméra orbitale
    float a = iMouse.z > 0. ? (iMouse.x/iResolution.x)*6.28 : iTime * 0.05;
    ro.xz *= rot(a); rd.xz *= rot(a);

    // Background Sky
    vec3 skyBase = vec3(0.6, 0.75, 0.9);
    float cloud = fbm(rd.xz / (rd.y + 0.1) + iTime * 0.2);
    vec3 col = mix(skyBase, vec3(1.0), smoothstep(0.4, 1.0, cloud));

    // Raymarching
    float d = 0.; vec2 m;
    for(int i=0; i<MAX_STEPS; i++) {
        m = map(ro + rd * d);
        if(m.x < SURFACE_DIST || d > MAX_DIST) break;
        d += m.x;
    }

    if(d < MAX_DIST) {
        vec3 p = ro + rd * d;
        vec3 n = getNormal(p);
        vec3 l = normalize(vec3(1, 5, -2));
        float diff = max(dot(n, l), 0.0);
        vec3 matCol = vec3(0);

        if(m.y == 1.0) { // Écorce/Feuilles (simplifié pour forêt)
            float isLeaf = map(p + n * 0.01).x > m.x ? 0.0 : 1.0; 
            matCol = mix(BARK_TINT, LEAF_COL, isLeaf);
            if(isLeaf > 0.5) diff += pow(max(dot(rd, -l), 0.0), 8.0) * 0.5;
        } 
        else if(m.y == 4.0) matCol = mix(vec3(0.05, 0.1, 0), vec3(0.2, 0.4, 0.1), p.y*3.0);
        else matCol = texture(iChannel1, p.xz * 0.1).rgb * 0.2;

        col = matCol * (diff + 0.15);
        col = mix(col, skyBase, 1.0 - exp(-0.0004 * d * d));
    }

    fragColor = vec4(pow(col, vec3(0.4545)), 1.0);
}