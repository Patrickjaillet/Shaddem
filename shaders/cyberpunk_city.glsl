// Cyberpunk City - Neon Rain
// Une ville procédurale avec effet de parallaxe, néons et pluie.

float hash(float n) { return fract(sin(n) * 43758.5453); }

float noise(vec2 x) {
    vec2 p = floor(x);
    vec2 f = fract(x);
    f = f * f * (3.0 - 2.0 * f);
    float n = p.x + p.y * 57.0;
    return mix(mix(hash(n + 0.0), hash(n + 1.0), f.x),
               mix(hash(n + 57.0), hash(n + 58.0), f.x), f.y);
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    float t = iTime * 0.5;
    
    vec3 col = vec3(0.0);
    
    // Ciel / Fond (Dégradé sombre)
    col = mix(vec3(0.05, 0.02, 0.1), vec3(0.0, 0.0, 0.05), uv.y + 0.5);
    
    // Lune / Soleil Cyberpunk
    float moon = smoothstep(0.5, 0.49, length(uv - vec2(0.6, 0.3)));
    col += vec3(0.9, 0.8, 1.0) * moon * 0.1; // Lueur
    col += vec3(1.0) * smoothstep(0.1, 0.09, length(uv - vec2(0.6, 0.3)));
    
    // Couches de la ville (Parallax)
    // On dessine de l'arrière vers l'avant
    for (float i = 0.0; i < 4.0; i++) {
        float z = 1.0 - i / 4.0; // Profondeur (1.0 = devant, 0.0 = fond)
        float scale = 4.0 + i * 2.0; // Échelle des bâtiments
        float speed = (1.0 - z) * 0.2 + 0.05; // Vitesse de défilement
        
        vec2 layerUV = uv;
        layerUV.x += t * speed;
        layerUV.x *= scale;
        
        vec2 id = floor(layerUV);
        vec2 gv = fract(layerUV) - 0.5;
        
        // Hauteur procédurale des bâtiments
        float h = noise(vec2(id.x, i * 10.0)) * 0.6 + 0.1;
        
        // Masque du bâtiment
        float mask = step(gv.y + 0.5, h);
        
        if (mask > 0.0) {
            vec3 buildCol = vec3(0.0);
            
            // Fenêtres / Néons
            vec2 winUV = layerUV * vec2(1.0, 5.0); // Fenêtres verticales
            vec2 winId = floor(winUV);
            
            // Probabilité d'avoir une fenêtre allumée
            if (hash(dot(winId, vec2(12.34, 56.78))) > 0.6) {
                // Couleur néon basée sur la position et le temps
                float hue = hash(winId.x) + t * 0.1;
                vec3 neon = 0.5 + 0.5 * cos(hue * 6.28 + vec3(0, 2, 4));
                
                // Effet de clignotement
                float blink = step(0.1, sin(t * 5.0 + hash(winId.y) * 10.0));
                
                buildCol += neon * 2.0 * blink;
            }
            
            // Couleur de base du bâtiment (sombre)
            buildCol = mix(vec3(0.01, 0.01, 0.02), buildCol, 0.5);
            
            // Brouillard atmosphérique (Distance fade)
            col = mix(col, buildCol, z * z);
        }
    }
    
    // Pluie
    float rain = 0.0;
    for (float i = 0.0; i < 2.0; i++) {
        vec2 rainUV = uv * vec2(1.0, 20.0) * (1.5 + i); // Gouttes allongées
        rainUV.y += t * (15.0 + i * 5.0); // Vitesse de chute
        rainUV.x += noise(vec2(rainUV.y * 0.01, i)) * 0.5; // Vent léger
        
        float n = noise(rainUV);
        if (n > 0.9) {
            rain += (n - 0.9) * 10.0 * (1.0 - i * 0.5);
        }
    }
    col += vec3(0.7, 0.8, 1.0) * rain * 0.3; // Couleur bleutée de la pluie
    
    // Color Grading Final
    col = pow(col, vec3(0.8)); // Contraste
    col *= vec3(1.0, 0.95, 1.1); // Teinte froide
    
    fragColor = vec4(col, 1.0);
}