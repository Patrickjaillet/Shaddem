/*
 * Sphère 3D en Raymarching qui pulse sur le kick de la musique.
 * Le rayon de la sphère est modulé par l'uniforme 'customAudioKick'.
*/

// 'DE' (Distance Estimator) pour une sphère.
float DE(vec3 p) {
    // Le rayon de base est 1.0. On ajoute l'effet du kick.
    // customAudioKick est une valeur entre 0.0 et 1.0 fournie par le moteur.
    float radius = 1.0 + customAudioKick * 0.5;
    return length(p) - radius;
}

// Calcule la normale (orientation de la surface) pour l'éclairage.
vec3 getNormal(vec3 p) {
    float d = 0.0001;
    return normalize(vec3(
        DE(p + vec3(d, 0, 0)) - DE(p - vec3(d, 0, 0)),
        DE(p + vec3(0, d, 0)) - DE(p - vec3(0, d, 0)),
        DE(p + vec3(0, 0, d)) - DE(p - vec3(0, 0, d))
    ));
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord.xy * 2.0 - iResolution.xy) / iResolution.y;

    // Caméra simple qui tourne lentement
    vec3 ro = vec3(2.5 * cos(iTime * 0.2), 0.0, 2.5 * sin(iTime * 0.2));
    vec3 target = vec3(0.0, 0.0, 0.0);
    vec3 fwd = normalize(target - ro);
    vec3 right = normalize(cross(fwd, vec3(0.0, 1.0, 0.0)));
    vec3 up = normalize(cross(right, fwd));
    vec3 rd = normalize(uv.x * right + uv.y * up + 1.5 * fwd);

    // Raymarching
    float t = 0.0;
    for (int i = 0; i < 80; i++) {
        vec3 p = ro + rd * t;
        float d = DE(p);
        if (d < 0.001 || t > 20.0) break;
        t += d;
    }

    // Coloriage et éclairage
    vec3 col = vec3(0.0);
    if (t < 20.0) {
        vec3 p = ro + rd * t;
        vec3 normal = getNormal(p);
        vec3 lightDir = normalize(vec3(0.8, 0.5, -0.5));
        float diff = max(dot(normal, lightDir), 0.1); // Diffus + Ambiant
        
        // La couleur de base change aussi avec le kick
        vec3 baseColor = mix(vec3(0.1, 0.2, 0.8), vec3(1.0, 0.2, 0.5), customAudioKick);
        col = baseColor * diff;
        
        // Effet Fresnel pour illuminer les bords
        float fresnel = pow(1.0 - dot(normalize(-rd), normal), 3.0);
        col += vec3(1.0, 0.8, 0.5) * fresnel * 0.5;
    }

    fragColor = vec4(col, 1.0);
}