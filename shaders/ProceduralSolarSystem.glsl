// =====================================================
// Procedural Solar System - Raymarching
// Structure basée sur : dark_chrome_mandelbulb_julia.hlsl
// =====================================================

// ===== PARAMETERS =====
#define PI 3.14159265
#define SATURN_RINGS 1

// ===== COLOR PALETTE [cite: 1] =====
vec3 palette(float d){
    vec3 a = vec3(0.05, 0.02, 0.1); // [cite: 1]
    vec3 b = vec3(0.5, 0.4, 0.3);   // [cite: 2]
    vec3 c = vec3(1.0, 1.0, 1.0);   // [cite: 2]
    vec3 d2 = vec3(0.1, 0.2, 0.3);  // [cite: 2]
    return a + b*cos(6.28318*(c*d+d2)); // [cite: 3]
}

// ===== ROTATION [cite: 3] =====
vec2 rotate(vec2 p, float a){
    float c = cos(a);
    float s = sin(a);
    return mat2(c,-s,s,c)*p; // [cite: 4]
}

// ===== SDF SHAPES =====
float sdSphere(vec3 p, float s) {
    return length(p) - s;
}

// ===== DISTANCE FIELD (SOLAR SYSTEM) [cite: 11] =====
float map(vec3 p){
    float t = iTime * 0.5; // [cite: 11]
    
    // Le Soleil (Centre)
    float d = sdSphere(p, 1.2);
    
    // Planète 1 (Mercure-like - Rapide et proche)
    vec3 p1 = p;
    p1.xz = rotate(p1.xz, t * 2.0);
    p1.x -= 2.0;
    d = min(d, sdSphere(p1, 0.2));
    
    // Planète 2 (Terre-like)
    vec3 p2 = p;
    p2.xz = rotate(p2.xz, t * 0.8);
    p2.x -= 3.5;
    d = min(d, sdSphere(p2, 0.4));
    
    // Planète 3 (Géante avec anneau)
    vec3 p3 = p;
    p3.xz = rotate(p3.xz, t * 0.4);
    p3.x -= 6.0;
    float planet3 = sdSphere(p3, 0.7);
    
    // Anneaux (SDF simplifié)
    float rings = max(length(p3.xz) - 1.5, -(length(p3.xz) - 0.9));
    rings = max(rings, abs(p3.y) - 0.02);
    
    d = min(d, min(planet3, rings));

    return d; // [cite: 13]
}

// ===== NORMAL [cite: 14] =====
vec3 normal(vec3 p){
    vec2 e = vec2(0.001,0.0); // [cite: 14]
    return normalize(vec3( // [cite: 15]
        map(p+e.xyy) - map(p-e.xyy),
        map(p+e.yxy) - map(p-e.yxy),
        map(p+e.yyx) - map(p-e.yyx)
    )); // [cite: 16]
}

// ===== RAYMARCH [cite: 18] =====
vec4 rm(vec3 ro, vec3 rd){
    float t = 0.0;
    vec3 col = vec3(0.0);
    float glow = 0.0; // [cite: 19]
    vec3 hitPos;
    bool hit = false; // [cite: 19]

    for(int i = 0; i < 100; i++){ // [cite: 20]
        vec3 pos = ro + rd * t;
        float d = map(pos); // [cite: 21]

        if(d < 0.001){
            hitPos = pos; // [cite: 21]
            hit = true; // [cite: 22]
            break;
        }
        if(t > 50.0) break; // [cite: 22]

        // Glow intense pour le Soleil [cite: 23]
        glow += exp(-length(pos)*0.5) * 0.15; 
        t += d * 0.8; // [cite: 23]
    }

    float shade = exp(-t*0.05); // [cite: 24]
    col = palette(shade); // [cite: 24]

    if(hit){
        vec3 n = normal(hitPos); // [cite: 24]
        vec3 v = normalize(-rd); // [cite: 25]
        vec3 l = normalize(vec3(0.0,0.0,0.0) - hitPos); // Lumière venant du Soleil
        
        float diff = max(dot(n, l), 0.1);
        col *= diff; // Ombrage planétaire
        
        // Reflet spéculaire style chrome [cite: 27]
        vec3 r = reflect(rd, n);
        float spec = pow(max(dot(r, l), 0.0), 30.0);
        col += spec * 0.5; // [cite: 28]
    }

    // Ajout du fond étoilé procédural
    float stars = pow(fract(sin(dot(rd.xy, vec2(12.9898, 78.233))) * 43758.5453), 20.0);
    col += stars * (sin(iTime + stars * 10.0) * 0.5 + 0.5);

    col += glow * vec3(1.0, 0.6, 0.2); // Glow solaire [cite: 29]
    return vec4(col * shade, 1.0); // [cite: 29]
}

// ===== MAIN [cite: 29] =====
void mainImage(out vec4 fragColor, in vec2 fragCoord){
    vec2 uv = (fragCoord - 0.5*iResolution.xy) / iResolution.y; // [cite: 30]
    
    // Caméra orbitale cinématographique
    float camDist = -12.0 + sin(iTime*0.1)*2.0;
    vec3 ro = vec3(0.0, 4.0, camDist); // [cite: 30]
    ro.xz = rotate(ro.xz, iTime * 0.1);
    
    vec3 lookAt = vec3(0.0, 0.0, 0.0);
    vec3 f = normalize(lookAt - ro);
    vec3 r = normalize(cross(vec3(0,1,0), f));
    vec3 u = cross(f, r);
    vec3 rd = normalize(uv.x*r + uv.y*u + 1.5*f); // [cite: 30]

    vec4 col = rm(ro, rd); // [cite: 31]

    // Vignette cinématographique [cite: 31]
    float v = smoothstep(1.5, 0.4, length(uv));
    col.rgb *= v; // [cite: 32]
    
    fragColor = col; // [cite: 32]
}