// Kaléidoscope fractal avec symétrie et mandala

float hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

// Rotation 2D
vec2 rotate(vec2 p, float a) {
    float c = cos(a);
    float s = sin(a);
    return vec2(p.x * c - p.y * s, p.x * s + p.y * c);
}

// Effet kaléidoscope
vec2 kaleidoscope(vec2 p, float segments) {
    float angle = atan(p.y, p.x);
    float radius = length(p);
    angle = mod(angle, 6.28318 / segments);
    if (mod(floor(atan(p.y, p.x) / (6.28318 / segments)), 2.0) < 0.5) {
        angle = 6.28318 / segments - angle;
    }
    return vec2(cos(angle), sin(angle)) * radius;
}

// Pattern fractal
float fractalPattern(vec2 p, float time) {
    float value = 0.0;
    float amplitude = 0.5;
    
    for (int i = 0; i < 6; i++) {
        p = kaleidoscope(p, 6.0);
        p = rotate(p, time * 0.2 + float(i) * 0.5);
        
        float r = length(p);
        float a = atan(p.y, p.x);
        
        value += sin(r * 10.0 - time + a * 3.0) * amplitude;
        value += sin(a * 12.0 + time * 0.5) * amplitude * 0.5;
        
        p *= 2.0;
        amplitude *= 0.5;
    }
    
    return value * 0.5 + 0.5;
}

// Mandala layers
float mandala(vec2 p, float time) {
    p = kaleidoscope(p, 8.0);
    
    float r = length(p);
    float a = atan(p.y, p.x);
    
    float pattern = 0.0;
    pattern += sin(a * 16.0 + time) * cos(r * 8.0 - time * 2.0);
    pattern += sin(a * 8.0 - time * 0.7) * sin(r * 12.0 + time);
    pattern += cos(a * 24.0 + r * 5.0);
    
    return pattern * 0.33 + 0.5;
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = fragCoord / iResolution.xy;
    vec2 p = (uv * 2.0 - 1.0) * vec2(iResolution.x / iResolution.y, 1.0);
    
    float time = iTime * 0.3;
    
    // Zoom pulsant
    float zoom = 1.5 + sin(time * 2.0) * 0.3;
    p *= zoom;
    
    // Rotation globale
    p = rotate(p, time * 0.5);
    
    // Multiple kaléidoscopes superposés
    vec2 k1 = kaleidoscope(p, 6.0);
    vec2 k2 = kaleidoscope(p, 8.0);
    vec2 k3 = kaleidoscope(p, 12.0);
    
    // Patterns fractals
    float frac1 = fractalPattern(k1, time);
    float frac2 = fractalPattern(k2 * 1.5, time * 1.3);
    float frac3 = fractalPattern(k3 * 0.7, time * 0.8);
    
    // Mandala
    float mand = mandala(p * 1.2, time);
    
    // Combiner
    float combined = frac1 * 0.3 + frac2 * 0.3 + frac3 * 0.2 + mand * 0.2;
    combined = fract(combined * 2.0);
    
    // Palette mystique
    vec3 col1 = vec3(0.5, 0.0, 1.0);  // Violet
    vec3 col2 = vec3(0.0, 0.8, 1.0);  // Cyan
    vec3 col3 = vec3(1.0, 0.8, 0.0);  // Or
    vec3 col4 = vec3(1.0, 0.0, 0.5);  // Magenta
    
    vec3 col = mix(col1, col2, sin(combined * 3.14159) * 0.5 + 0.5);
    col = mix(col, col3, sin(combined * 6.28318) * 0.5 + 0.5);
    col = mix(col, col4, pow(combined, 3.0));
    
    // Ajouter des étoiles aux intersections
    float stars = pow(frac1 * frac2 * frac3, 0.3);
    col += vec3(1.0) * stars * 1.5;
    
    // Glow au centre
    float centerGlow = 1.0 / (1.0 + length(p) * 1.5);
    col += vec3(0.8, 0.5, 1.0) * centerGlow * 0.5;
    
    // Contours brillants
    float edge = abs(sin(combined * 20.0));
    edge = smoothstep(0.9, 1.0, edge);
    col += vec3(1.0, 0.9, 0.8) * edge * 0.8;
    
    // Bloom
    float brightness = dot(col, vec3(0.299, 0.587, 0.114));
    if (brightness > 0.7) {
        col += (col - 0.7) * 0.6;
    }
    
    // Vignette douce
    float vignette = 1.0 - length(p) * 0.3;
    col *= clamp(vignette, 0.4, 1.0);
    
    // Gamma
    col = pow(col, vec3(0.4545));
    
    fragColor = vec4(col, 1.0);
}