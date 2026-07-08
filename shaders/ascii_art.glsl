void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = fragCoord / iResolution.xy;
    
    // Paramètres de la grille (Densité des caractères)
    float charsX = 80.0; 
    // Correction du ratio pour avoir des cellules rectangulaires (comme des lettres)
    float charsY = charsX * (iResolution.y / iResolution.x) * 1.8; 
    
    vec2 cellUV = fract(uv * vec2(charsX, charsY));
    vec2 cellID = floor(uv * vec2(charsX, charsY));
    
    // Échantillonnage de la couleur au centre de la cellule (Pixelisation)
    vec2 sampleUV = (cellID + 0.5) / vec2(charsX, charsY);
    vec3 col = texture(iChannel0, sampleUV).rgb;
    
    // Calcul de la luminance (Gris)
    float gray = dot(col, vec3(0.299, 0.587, 0.114));
    
    // Dessin du caractère
    float d = 0.0;
    
    // Coordonnées locales centrées (-1 à 1)
    vec2 p = cellUV * 2.0 - 1.0;
    
    // Mapper le gris vers des niveaux discrets (0 à 5)
    int level = int(gray * 6.0);
    
    if (level == 0) {
        // Vide
        d = 0.0;
    } else if (level == 1) {
        // Point (.)
        d = step(length(p), 0.2);
    } else if (level == 2) {
        // Deux points (:)
        d = step(length(p - vec2(0.0, 0.5)), 0.2) + step(length(p + vec2(0.0, 0.5)), 0.2);
    } else if (level == 3) {
        // Plus (+)
        d = step(abs(p.x), 0.15) * step(abs(p.y), 0.8) + step(abs(p.y), 0.15) * step(abs(p.x), 0.8);
    } else if (level == 4) {
        // Croix (x)
        d = step(abs(p.x - p.y), 0.2) + step(abs(p.x + p.y), 0.2);
    } else {
        // Bloc (#)
        d = step(max(abs(p.x), abs(p.y)), 0.8);
    }
    
    // Couleur finale : on garde la couleur d'origine masquée par le caractère
    vec3 finalCol = col * d;
    
    fragColor = vec4(finalCol, 1.0);
}