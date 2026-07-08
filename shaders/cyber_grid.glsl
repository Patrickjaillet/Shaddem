void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    
    // Paramètres depuis l'application
    vec3 gridColor = customColor.rgb;
    float speed = customSpeed;
    
    // Perspective
    float horizon = 0.1;
    
    vec3 col = vec3(0.0);
    
    if (uv.y < horizon) {
        // Sol 3D projeté
        float z = 0.5 / abs(uv.y - horizon);
        vec2 gridUV = uv * z;
        
        // Animation du sol
        gridUV.y += iTime * speed;
        
        // Dessin de la grille
        float gridVal = max(step(0.98, fract(gridUV.x)), step(0.98, fract(gridUV.y)));
        
        // Atténuation à l'horizon
        gridVal *= smoothstep(10.0, 0.0, z);
        
        col = mix(vec3(0.05, 0.0, 0.1), gridColor, gridVal);
        col += gridColor * gridVal * 0.5 * customIntensity; // Glow
    } else {
        // Ciel dégradé
        float t = uv.y - horizon;
        col = mix(vec3(0.05, 0.0, 0.1), vec3(0.0), t * 2.0);
        
        // Soleil Rétro
        float sun = length(uv - vec2(0.0, 0.3));
        if (sun < 0.2) col += vec3(1.0, 0.2, 0.5) * (1.0 - smoothstep(0.15, 0.2, sun));
    }

    fragColor = vec4(col, 1.0);
}
