void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    
    // Caméra
    vec3 ro = vec3(0.0, 2.0, -6.0);
    vec3 rd = normalize(vec3(uv, 1.5));
    
    // Rotation Caméra
    float t = iTime * 0.2;
    mat2 rot = mat2(cos(t), -sin(t), sin(t), cos(t));
    ro.xz *= rot;
    rd.xz *= rot;
    
    // Paramètres Trou Noir
    vec3 bhPos = vec3(0.0, 0.0, 0.0);
    float bhMass = 0.5;
    float eventHorizon = bhMass * 2.0;
    float accretionDiskRadius = eventHorizon * 3.0;
    
    vec3 col = vec3(0.0);
    vec3 p = ro;
    
    // Raymarching avec courbure (Lentille Gravitationnelle simplifiée)
    // Au lieu de tracer des géodésiques complexes, on déforme l'espace
    
    float dist = 0.0;
    vec3 glow = vec3(0.0);
    
    for(int i=0; i<100; i++) {
        float d = length(p - bhPos);
        
        // Gravitational bending (approximation)
        // La lumière est attirée vers le centre
        vec3 toBH = normalize(bhPos - p);
        float force = bhMass / (d * d);
        rd = normalize(rd + toBH * force * 0.5); // Bending factor
        
        p += rd * 0.1; // Petit pas pour simuler la courbe
        dist += 0.1;
        
        // Disque d'accrétion
        float diskH = abs(p.y);
        float diskD = length(p.xz);
        
        if (diskD > eventHorizon && diskD < accretionDiskRadius * 2.0) {
            // Densité du disque
            float density = exp(-diskH * 10.0) * exp(-abs(diskD - accretionDiskRadius) * 2.0);
            
            // Noise/Texture du disque (basé sur l'angle et le temps)
            float angle = atan(p.z, p.x);
            float speed = 10.0 / diskD; // Plus rapide près du centre
            float noise = 0.5 + 0.5 * sin(angle * 20.0 + iTime * speed);
            
            // Couleur (Chaud au centre, froid au bord)
            vec3 diskCol = mix(vec3(1.0, 0.5, 0.1), vec3(0.1, 0.2, 1.0), (diskD - eventHorizon) / accretionDiskRadius);
            
            glow += density * noise * 0.1 * diskCol;
        }
        
        // Horizon des événements (Noir absolu)
        if (d < eventHorizon) {
            col = vec3(0.0);
            break;
        }
        
        if (dist > 20.0) break; // Fond de l'espace
    }
    
    // Fond étoilé (si le rayon s'échappe)
    if (length(col) == 0.0 && length(glow) < 1.0) {
        float stars = fract(sin(dot(rd.xy, vec2(12.9898, 78.233))) * 43758.5453);
        col += vec3(pow(stars, 20.0));
    }
    
    col += glow;
    
    fragColor = vec4(col, 1.0);
}