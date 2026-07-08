/*
 * Fractale de type Mandelbulb/Julia en 3D par Raymarching.
 *
 * Le "raymarching" est une technique qui permet de rendre des scènes 3D
 * en "marchant" le long d'un rayon pour chaque pixel, jusqu'à toucher
 * une surface. C'est très efficace pour les formes mathématiques complexes.
*/

// 'DE' (Distance Estimator) : C'est la fonction mathématique qui définit la forme.
// Pour un point 'pos' dans l'espace 3D, elle retourne la distance la plus courte
// jusqu'à la surface de la fractale.
float DE(vec3 pos) {
    vec3 z = pos;
    float dr = 1.0;
    float r = 0.0;
    // La puissance de la fractale, contrôlée par l'uniform 'customShaderParam1' (défaut à 8.0 si non fourni ou trop bas)
    float power = (customShaderParam1 < 1.0) ? 8.0 : customShaderParam1;

    // C'est le cœur de la fractale : une boucle d'itération.
    for (int i = 0; i < 5; i++) {
        r = length(z);
        if (r > 2.0) break; // Si le point s'échappe, on arrête.

        // Sauve la dérivée pour l'estimation de distance
        dr = pow(r, power - 1.0) * power * dr + 1.0;

        // Conversion en coordonnées sphériques pour la transformation
        float theta = acos(z.z / r);
        float phi = atan2(z.y, z.x);

        // Transformation fractale
        float zr = pow(r, power);
        theta = theta * power;
        phi = phi * power;

        // Retour en coordonnées cartésiennes
        z = zr * vec3(sin(theta) * cos(phi), sin(phi) * sin(theta), cos(theta));
        z += pos; // L'ajout de 'pos' crée la variation de type "Julia"
    }
    // La formule finale pour estimer la distance
    return 0.5 * log(r) * r / dr;
}

// Calcule la "normale" (l'orientation de la surface) pour l'éclairage.
vec3 getNormal(vec3 p) {
    float d = 0.0001; // Une très petite distance
    return normalize(vec3(
        DE(p + vec3(d, 0, 0)) - DE(p - vec3(d, 0, 0)),
        DE(p + vec3(0, d, 0)) - DE(p - vec3(0, d, 0)),
        DE(p + vec3(0, 0, d)) - DE(p - vec3(0, 0, d))
    ));
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord.xy * 2.0 - iResolution.xy) / iResolution.y;

    // Caméra qui tourne lentement autour de l'origine
    vec3 ro = vec3(2.5 * cos(iTime * 0.2), 0.0, 2.5 * sin(iTime * 0.2));
    vec3 target = vec3(0.0, 0.0, 0.0);
    vec3 fwd = normalize(target - ro);
    vec3 right = normalize(cross(fwd, vec3(0.0, 1.0, 0.0)));
    vec3 up = normalize(cross(right, fwd));
    vec3 rd = normalize(uv.x * right + uv.y * up + 1.5 * fwd); // 1.5 = zoom

    // Raymarching : on avance le long du rayon
    float t = 0.0;
    for (int i = 0; i < 100; i++) {
        vec3 p = ro + rd * t;
        float d = DE(p);
        if (d < 0.001 || t > 20.0) break;
        t += d;
    }

    // Éclairage simple si on a touché la surface
    vec3 col = vec3(0.0);
    if (t < 20.0) {
        vec3 p = ro + rd * t;
        vec3 normal = getNormal(p);
        vec3 lightDir = normalize(vec3(1.0, 1.0, 0.8));
        float diff = max(dot(normal, lightDir), 0.1); // Lumière diffuse + ambiante
        
        // Couleur basée sur la position pour un effet psychédélique
        vec3 baseColor = 0.5 + 0.5 * cos(iTime * 0.5 + p.xyz * 2.0 + vec3(0, 2, 4));
        col = baseColor * diff;
    }

    fragColor = vec4(col, 1.0);
}