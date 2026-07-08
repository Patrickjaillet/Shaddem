// Accumulation de glow basée sur : rm loop 
void mainImage(out vec4 fragColor, in vec2 fragCoord){
    vec2 uv = (fragCoord - 0.5*iResolution.xy) / iResolution.y;
    float glow = 0.0;
    vec3 col = vec3(0.0);
    
    for(float i = 0.0; i < 1.0; i += 0.2){
        float t = iTime * 0.1 + i;
        vec2 p = uv * (2.0 + sin(t));
        float d = length(p) - 0.5;
        glow += exp(-abs(d) * 4.0) * 0.2;
    }
    
    // Étoiles procédurales
    float stars = pow(fract(sin(dot(uv, vec2(12.9, 78.2))) * 43758.5), 15.0);
    col = vec3(0.1, 0.2, 0.5) * glow;
    col += stars * (sin(iTime*2.0) * 0.5 + 0.5);
    
    fragColor = vec4(col, 1.0);
}