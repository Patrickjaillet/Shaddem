void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    
    // Caméra et Rotation
    vec3 ro = vec3(0.0, 0.0, -2.5);
    vec3 rd = normalize(vec3(uv, 1.0));
    
    float t = iTime * 0.5;
    mat2 rot = mat2(cos(t), -sin(t), sin(t), cos(t));
    ro.xz *= rot;
    rd.xz *= rot;
    
    // Raymarching
    float d = 0.0;
    float t_march = 0.0;
    vec3 p = vec3(0.0);
    int steps = 0;
    
    // Pulsation Audio
    float kick = customAudioKick; // 0.0 à 1.0
    float pulse = 1.0 + kick * 0.3;
    
    for(int i=0; i<64; i++) {
        p = ro + rd * t_march;
        
        // Déformation Cœur
        vec3 q = p;
        q /= pulse; // Scale animation
        q.y -= 0.3; // Centrage vertical
        
        // Formule approximative d'un cœur 3D déformé depuis une sphère
        // y -= abs(x) * sqrt((20 - abs(x)) / 15)
        float h = abs(q.x);
        float y_shift = h * sqrt((20.0 - h) / 15.0) * 0.4;
        q.y -= y_shift;
        q.z *= 1.2; // Aplatir légèrement
        
        d = length(q) - 0.7;
        
        if(d < 0.01 || t_march > 10.0) break;
        t_march += d;
        steps = i;
    }
    
    vec3 col = vec3(0.0);
    
    if(d < 0.01) {
        // Effet Wireframe Néon
        // On utilise la position locale 'p' pour générer une grille
        float gridDensity = 15.0;
        vec3 grid = step(0.95, fract(p * gridDensity));
        float isWire = max(max(grid.x, grid.y), grid.z);
        
        // Couleur Néon (Rose/Rouge + Bleu sur les kicks)
        vec3 neonColor = mix(vec3(1.0, 0.0, 0.5), vec3(0.0, 0.8, 1.0), kick);
        
        // Glow basé sur le nombre d'étapes (Fake AO/Glow) et le wireframe
        col = neonColor * isWire * 2.0;
        col += neonColor * 0.2; // Ambient glow
    }
    
    fragColor = vec4(col, 1.0);
}