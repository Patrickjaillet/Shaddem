void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    
    // Animation Caméra (Vol avant + Légère oscillation)
    float time = iTime * 0.8;
    vec3 ro = vec3(2.0 * cos(time * 0.1), 3.0 + sin(time * 0.15), time * 4.0);
    vec3 ta = ro + vec3(sin(time * 0.1) * 0.5, -0.2, 1.0);
    
    // Matrice Caméra
    vec3 fwd = normalize(ta - ro);
    vec3 right = normalize(cross(fwd, vec3(0.0, 1.0, 0.0)));
    vec3 up = cross(right, fwd);
    vec3 rd = normalize(fwd + uv.x * right + uv.y * up);
    
    // Raymarching
    float t = 0.0;
    float d = 0.0;
    vec3 p = vec3(0.0);
    
    // Accumulation Volumétrique (Glow)
    vec3 glow = vec3(0.0);
    float glowIntensity = 0.0;
    
    int i = 0;
    for(i = 0; i < 100; i++) {
        p = ro + rd * t;
        
        // Génération de la ville (Modulation de l'espace)
        // On répète l'espace tous les 4.0 unités
        vec3 q = mod(p, 4.0) - 2.0;
        vec3 id = floor(p / 4.0);
        
        // Hauteur aléatoire des immeubles basée sur l'ID du bloc
        float rnd = fract(sin(dot(id.xz, vec2(12.9898, 78.233))) * 43758.5453);
        float height = 0.5 + rnd * 3.0;
        
        // SDF Boîte (Immeuble)
        float box = length(max(abs(q) - vec3(0.8, height, 0.8), 0.0));
        
        // Détails (Croix/Fenêtres pour effet Menger Sponge simplifié)
        float c = length(max(abs(q.xy) - vec2(0.2, 0.1), 0.0)); // Fenêtres horizontales
        c = min(c, length(max(abs(q.yz) - vec2(0.1, 0.2), 0.0)));
        
        d = max(box, -c); // Soustraction pour créer des fenêtres
        
        // Sol
        d = min(d, p.y + 2.0);
        
        // Accumulation de la lumière (Néons)
        // Couleur basée sur la position et le temps
        vec3 neonColor = 0.5 + 0.5 * cos(id.x * 0.5 + vec3(0.0, 2.0, 4.0) + iTime);
        
        // Plus on est proche de la surface, plus ça brille (Volumetric Glow)
        float distFactor = 1.0 / (1.0 + d * d * 20.0);
        glow += distFactor * neonColor * 0.05;
        
        if(d < 0.001 || t > 80.0) break;
        t += d * 0.5; // Pas plus petit pour éviter les artefacts sur les détails
    }
    
    vec3 col = vec3(0.0);
    
    if(t < 80.0) {
        // Couleur de base des bâtiments (Sombre)
        col = vec3(0.05, 0.05, 0.08);
        
        // Brouillard de distance (Fog)
        float fog = 1.0 - exp(-t * 0.03);
        vec3 fogCol = vec3(0.05, 0.02, 0.1); // Brouillard violet sombre
        col = mix(col, fogCol, fog);
    }
    
    // Ajout de la lumière volumétrique accumulée
    col += glow * 2.0;
    
    // Tone mapping et Gamma
    fragColor = vec4(pow(col, vec3(0.4545)), 1.0);
}