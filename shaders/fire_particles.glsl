// Fire Particles
// Simulation de feu avec des particules montantes (étincelles)

// --- Fonctions de Bruit ---
float hash(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

float noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash(i);
    float b = hash(i + vec2(1.0, 0.0));
    float c = hash(i + vec2(0.0, 1.0));
    float d = hash(i + vec2(1.0, 1.0));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

float fbm(vec2 p) {
    float v = 0.0;
    float a = 0.5;
    for (int i = 0; i < 5; i++) {
        v += a * noise(p);
        p *= 2.0;
        a *= 0.5;
    }
    return v;
}

// --- Particules (Étincelles) ---
float particles(vec2 uv) {
    float p = 0.0;
    // On simule 50 particules
    for (float i = 0.0; i < 50.0; i++) {
        // Random properties based on index
        float n = hash(vec2(i, i));
        float size = 0.005 + 0.01 * n;
        float speed = 0.2 + 0.5 * n;
        float x_offset = (n - 0.5) * 2.0; // -1 to 1
        
        // Position qui boucle
        float t = iTime * speed + n * 10.0;
        float y = fract(t); // 0 to 1 (bas vers haut)
        
        // Mouvement sinusoïdal en X
        float x = 0.5 + x_offset * 0.8 + sin(y * 10.0 + n * 100.0) * 0.05;
        
        // Forme de la particule (Glow)
        vec2 pos = vec2(x, y);
        float dist = length(uv - pos);
        
        // Fade in/out en bas et haut
        float fade = smoothstep(0.0, 0.2, y) * smoothstep(1.0, 0.5, y);
        
        p += (size / dist) * fade * 0.05; // Accumulation
    }
    return p;
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = fragCoord / iResolution.xy;
    
    // --- Fond Feu (Noise) ---
    vec2 q = uv;
    q.x *= 2.0; // Étirer bruit
    q.y -= iTime * 0.5; // Monter
    
    float fireNoise = fbm(q * 3.0);
    
    // Forme du feu (masque en bas de l'écran)
    float fireMask = smoothstep(1.0, 0.0, uv.y + abs(uv.x - 0.5) * 2.0 - fireNoise * 0.5);
    
    // Couleur du feu
    vec3 fireColor = vec3(1.5, 0.5, 0.1) * fireNoise * fireMask * 2.0;
    
    // --- Ajout des particules ---
    float sparks = particles(uv);
    vec3 sparkColor = vec3(1.0, 0.8, 0.2) * sparks;
    
    // --- Composition ---
    vec3 col = fireColor + sparkColor;
    
    // Fond sombre
    col += vec3(0.1, 0.0, 0.0) * (1.0 - uv.y);
    
    fragColor = vec4(col, 1.0);
}