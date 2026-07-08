// Raymarching Sphere
// Une sphère 3D simple rendue avec la technique du Raymarching.

// Fonction de distance signée (SDF) pour une sphère
float sdSphere(vec3 p, float s) {
    return length(p) - s;
}

// Carte de la scène
float map(vec3 p) {
    // Sphère au centre (0,0,0) avec rayon 1.0
    // Animation verticale avec le temps
    vec3 spherePos = vec3(0.0, sin(iTime) * 0.5, 0.0);
    return sdSphere(p - spherePos, 1.0);
}

// Calcul de la normale (gradient de la distance)
vec3 calcNormal(vec3 p) {
    const float h = 0.0001;
    const vec2 k = vec2(1, -1);
    return normalize(k.xyy * map(p + k.xyy * h) +
                     k.yyx * map(p + k.yyx * h) +
                     k.yxy * map(p + k.yxy * h) +
                     k.xxx * map(p + k.xxx * h));
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    // Coordonnées UV centrées (-1 à 1 sur Y)
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;

    // Configuration Caméra
    vec3 ro = vec3(0.0, 0.0, -3.0); // Origine du rayon (Caméra)
    vec3 rd = normalize(vec3(uv, 1.0)); // Direction du rayon

    // Boucle de Raymarching
    float t = 0.0; // Distance parcourue
    float d = 0.0; // Distance à la surface
    int i;
    for (i = 0; i < 80; i++) {
        vec3 p = ro + rd * t;
        d = map(p);
        t += d;
        // Si on est très proche de la surface ou trop loin
        if (d < 0.001 || t > 20.0) break;
    }

    // Couleur de fond (Dégradé)
    vec3 col = mix(vec3(0.1, 0.1, 0.2), vec3(0.0, 0.0, 0.05), uv.y + 0.5);

    // Si on touche un objet (distance très petite)
    if (d < 0.001) {
        vec3 p = ro + rd * t;
        vec3 n = calcNormal(p);
        
        // Éclairage simple
        vec3 lightPos = vec3(2.0, 4.0, -3.0);
        vec3 l = normalize(lightPos - p);
        
        // Diffuse (Lumière douce)
        float diff = max(dot(n, l), 0.0);
        
        // Specular (Reflet brillant - Phong)
        vec3 v = normalize(ro - p);
        vec3 r = reflect(-l, n);
        float spec = pow(max(dot(v, r), 0.0), 32.0);
        
        // Couleur de la sphère (Mélange avec la couleur personnalisée de l'interface)
        vec3 sphereColor = vec3(0.2, 0.6, 1.0); 
        sphereColor = mix(sphereColor, customColor.rgb, 0.5);

        col = sphereColor * (diff + 0.1) + vec3(1.0) * spec;
    }

    // Sortie
    fragColor = vec4(col, 1.0);
}