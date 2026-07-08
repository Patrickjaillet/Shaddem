void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = fragCoord / iResolution.xy;
    
    float time = iTime * 0.5;
    
    // Configuration de la grille
    float columns = 50.0;
    float rows = 20.0;
    // Ajustement du ratio pour des caractères rectangulaires
    vec2 grid = vec2(columns, rows * (iResolution.y / iResolution.x));
    vec2 ipos = floor(uv * grid);
    vec2 fpos = fract(uv * grid);
    
    // Aléatoire par colonne
    float random = fract(sin(dot(ipos, vec2(12.9898, 78.233))) * 43758.5453);
    float columnRandom = fract(sin(dot(vec2(ipos.x, 0.0), vec2(12.9898, 78.233))) * 43758.5453);
    
    // Vitesse de chute variable par colonne
    float speed = 0.5 + 0.5 * columnRandom;
    float y = fract(uv.y + time * speed + columnRandom);
    
    // Traînée (Trail)
    float trail = smoothstep(0.0, 1.0, 1.0 - y);
    trail = pow(trail, 3.0); // Rendre la traînée plus courte/nette
    
    // Tête brillante (le caractère du bas)
    float head = step(0.98, y);
    
    // Forme du caractère (simplifiée : points/croix aléatoires)
    vec2 center = fpos - 0.5;
    float charShape = step(length(center), 0.4);
    if (random > 0.5) charShape *= step(abs(center.x), 0.15) + step(abs(center.y), 0.15); // Croix
    
    // Clignotement
    float blink = step(0.1, fract(time * 5.0 + random));
    
    // Couleur Matrix (Vert)
    vec3 col = vec3(0.0, 1.0, 0.2) * trail * charShape * blink;
    col += vec3(0.8, 1.0, 0.8) * head * charShape; // Tête blanche/verte claire
    
    fragColor = vec4(col, 1.0);
}