void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = fragCoord / iResolution.xy;
    float time = iTime * customSpeed * 0.5;
    
    // Génération de motifs plasma
    float v = 0.0;
    vec2 c = uv * 2.0 - 1.0;
    
    v += sin(c.x * 10.0 + time);
    v += sin((c.y * 10.0 + time) / 2.0);
    v += sin((c.x + c.y) * 10.0 + time);
    c += vec2(sin(time), cos(time));
    v += sin(length(c) * 10.0 + time);
    
    // Couleur basée sur le motif et la couleur personnalisée
    float r = sin(v * 3.14 + customColor.r);
    float g = sin(v * 3.14 + customColor.g + 2.0);
    float b = sin(v * 3.14 + customColor.b + 4.0);
    
    vec3 col = vec3(r, g, b) * 0.5 + 0.5;
    
    // Application de l'intensité globale
    fragColor = vec4(col * customIntensity, 1.0);
}