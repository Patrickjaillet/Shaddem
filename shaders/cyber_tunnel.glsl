void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    
    float time = iTime * 2.0; // Vitesse de déplacement
    
    // Coordonnées polaires pour le tunnel
    // r = distance au centre, a = angle
    float r = length(uv);
    float a = atan(uv.y, uv.x);
    
    // Distortion 1/r pour créer la perspective du tunnel
    float dist = 1.0 / r;
    
    // Mapping UV du tunnel : x = profondeur, y = angle
    vec2 tunUV = vec2(dist + time, a / 3.14159);
    
    // Grille Néon
    // On multiplie pour répéter la texture/grille
    vec2 grid = fract(tunUV * vec2(1.0, 8.0)); // 1 segment profondeur, 8 segments radiaux
    float line = step(0.9, grid.x) + step(0.9, grid.y);
    
    // Couleur basée sur l'angle et la profondeur pour varier les teintes
    vec3 col = 0.5 + 0.5 * cos(time + tunUV.yxy * 3.0 + vec3(0, 2, 4));
    col *= line; // Appliquer la grille
    
    // Ajout de "lumières" qui passent (flashs blancs basés sur la profondeur)
    float light = pow(fract(dist * 0.5 + time * 0.5), 10.0);
    col += vec3(1.0) * light * 0.5;
    
    // Brouillard noir au centre (loin) pour masquer la singularité
    col *= r * 1.5;
    
    fragColor = vec4(col, 1.0);
}