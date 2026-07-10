// rzerzerr
// Shadertoy ID: 7cSXWz
// Description: zerzerzer
// Tags: zerzerzer

// --- UTILS ---
vec3 mod289(vec3 x) {
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec4 mod289(vec4 x) {
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}

vec4 mod7(vec4 x) {
    return x - floor(x * (1.0 / 7.0)) * 7.0;
}

vec3 permute(vec3 x) {
    return mod289((34.0 * x + 10.0) * x);
}

vec4 permute(vec4 x) {
    return mod289((34.0 * x + 10.0) * x);
}

vec2 cellular2x2x2(vec3 P) {
    const float K = 0.142857142857;
    const float Ko = 0.428571428571;
    const float K2 = 0.020408163265306;
    const float Kz = 0.166666666667;
    const float Kzo = 0.416666666667;
    const float jitter = 0.8;

    vec3 Pi = mod289(floor(P));
    vec3 Pf = fract(P);
    vec4 Pfx = Pf.x + vec4(0.0, -1.0, 0.0, -1.0);
    vec4 Pfy = Pf.y + vec4(0.0, 0.0, -1.0, -1.0);
    vec4 p = permute(Pi.x + vec4(0.0, 1.0, 0.0, 1.0));
    p = permute(p + Pi.y + vec4(0.0, 0.0, 1.0, 1.0));
    vec4 p1 = permute(p + Pi.z); 
    vec4 p2 = permute(p + Pi.z + vec4(1.0)); 
    
    vec4 ox1 = fract(p1 * K) - Ko;
    vec4 oy1 = mod7(floor(p1 * K)) * K - Ko;
    vec4 oz1 = floor(p1 * K2) * Kz - Kzo;
    vec4 ox2 = fract(p2 * K) - Ko;
    vec4 oy2 = mod7(floor(p2 * K)) * K - Ko;
    vec4 oz2 = floor(p2 * K2) * Kz - Kzo;
    
    vec4 dx1 = Pfx + jitter * ox1;
    vec4 dy1 = Pfy + jitter * oy1;
    vec4 dz1 = Pf.z + jitter * oz1;
    vec4 dx2 = Pfx + jitter * ox2;
    vec4 dy2 = Pfy + jitter * oy2;
    vec4 dz2 = Pf.z - 1.0 + jitter * oz2;
    
    vec4 d1 = dx1 * dx1 + dy1 * dy1 + dz1 * dz1; 
    vec4 d2 = dx2 * dx2 + dy2 * dy2 + dz2 * dz2;

    vec4 d = min(d1, d2); 
    d2 = max(d1, d2); 
    
    if(d.x > d.y) d.xy = d.yx;
    if(d.x > d.z) d.xz = d.zx;
    if(d.x > d.w) d.xw = d.wx;
    
    d.yzw = min(d.yzw, d2.yzw);
    d.y = min(d.y, d.z);
    d.y = min(d.y, d.w);
    d.y = min(d.y, d2.x);
    
    return sqrt(d.xy);
}

// --- VOLUME ENGINE ---
void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    vec3 ro = vec3(0.0, 0.0, -3.0);
    vec3 rd = normalize(vec3(uv, 1.5));

    // Rotation de la caméra pour dynamique XXL
    float ang = iTime * 0.2;
    mat2 rot = mat2(cos(ang), sin(ang), -sin(ang), cos(ang));
    ro.xz *= rot;
    rd.xz *= rot;

    vec3 col = vec3(0.0);
    float t = 0.0;
    
    for(int i = 0; i < 40; i++) {
        vec3 p = ro + rd * t;
        // Échantillonnage 3D
        vec2 noise = cellular2x2x2(p * 1.5 + vec3(0, 0, iTime * 0.5));
        
        // Densité basée sur F2 - F1 (aspect organique/mousse)
        float density = smoothstep(0.0, 0.5, noise.y - noise.x);
        density *= smoothstep(1.5, 0.5, length(p)); // Sphère de confinement
        
        if(density > 0.1) {
            vec3 glow = 0.5 + 0.5 * cos(iTime + p.yzz + vec3(0, 2, 4));
            col += glow * density * 0.12; // Accumulation de lumière
        }
        
        t += 0.1;
    }

    // Post-processing AAA
    col = smoothstep(0.0, 1.0, col);
    col = pow(col, vec3(0.4545)); // Correction Gamma
    
    fragColor = vec4(col, 1.0);
}