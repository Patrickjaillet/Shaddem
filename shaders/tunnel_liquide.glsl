void mainImage(out float4 fragColor, in float2 fragCoord) {
    // Normalisation des coordonnées : centre (0,0), Y de -1 à 1
    float2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    
    // Coordonnées polaires
    float r = length(uv);
    // Angle (-PI à PI)
    float a = atan2(uv.y, uv.x);
    
    // Vitesse globale
    float speed = iTime * customSpeed;
    
    // Distortion "Liquide"
    // On perturbe l'angle et la distance en fonction du temps et de la position
    // Cela crée l'effet de surface mouvante
    float wave = sin(r * 8.0 - speed * 2.0) * 0.5;
    wave += sin(a * 4.0 + speed) * 0.2;
    
    // Mapping UV cylindrique (Tunnel)
    // x = angle / PI (répétition circulaire)
    // y = 1/r (profondeur inverse)
    float2 tun = float2(a / 3.14159, 1.0 / (r + 0.05 * sin(speed)));
    
    // Application de la distortion aux UVs du tunnel
    tun.x += wave * 0.1;
    tun.y += speed;
    
    // Génération de motif procédural (plasma/liquide)
    float f = sin(tun.x * 10.0 + sin(tun.y * 5.0 + iTime));
    f += sin(tun.y * 8.0 - iTime * 0.5);
    f = sin(f * 2.0); // Augmente le contraste des vagues
    
    // Couleur de base (fond sombre)
    float3 col = float3(0.02, 0.0, 0.05);
    
    // Couleur du liquide (basée sur le paramètre customColor)
    float3 liquid = customColor.rgb;
    
    // Calcul du Bloom / Glow
    // On utilise l'inverse de la valeur du motif pour créer des pics d'intensité très lumineux
    float glow = 0.02 / abs(f * 0.1 + 0.01);
    
    // Atténuation avec la distance (brouillard noir au centre du tunnel)
    glow *= smoothstep(0.0, 0.8, r);
    
    // Composition finale
    // Le glow est multiplié par l'intensité pour saturer les blancs (effet bloom)
    col += liquid * glow * customIntensity * 2.0;
    col += float3(liquid.b, liquid.r, liquid.g) * glow * 0.5; // Reflets secondaires

    fragColor = float4(col, 1.0);
}