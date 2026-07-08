// Tunnel Wormhole
// Effet de torsion spatiale (Wormhole)

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    // Coordonnées centrées (-1 à 1)
    vec2 uv = (2.0 * fragCoord - iResolution.xy) / iResolution.y;

    // Polaires
    float r = length(uv);
    float a = atan2(uv.y, uv.x); // Utilise atan2 pour compatibilité via le moteur

    // Torsion (Twist) qui varie avec le temps
    // Plus on est proche du centre (r petit), plus la torsion est forte
    float twistStrength = 2.0 + sin(iTime) * 1.0;
    float angle = a + twistStrength / (r + 0.1);

    // Mapping tunnel : (Angle, Profondeur)
    // p.x = coordonnée angulaire (autour du tunnel)
    // p.y = coordonnée de profondeur (le long du tunnel) + mouvement
    vec2 p = vec2(angle / 3.14159, 1.0 / r + iTime * 0.5);

    // Motif de grille déformée
    float pattern = sin(p.x * 10.0) * sin(p.y * 10.0);
    
    // Couleurs basées sur la profondeur et l'angle (Palette cosinus)
    vec3 col = 0.5 + 0.5 * cos(iTime + p.y * 0.5 + vec3(0, 2, 4));
    
    // Ajout de contraste sur le motif pour marquer les lignes de force
    col += smoothstep(0.2, 0.8, pattern) * 0.5;
    
    // Glow central (assombrir le centre infini) et vignettage naturel
    col *= r * 1.5;

    fragColor = vec4(col, 1.0);
}