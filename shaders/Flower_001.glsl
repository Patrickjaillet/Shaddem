// Flower 001
// Shadertoy ID: 732SDK
// Description: Flower IV
// Tags: fractal

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    float t = iTime * 0.4;
    vec3 col = vec3(0.0);
    for (float i = 0.0; i < 32.0; i++) {
        float scale = pow(0.98, i + fract(t));
        float angle = i * 0.15 + t * 0.1;
        vec2 p = (uv / scale) * mat2(cos(angle), -sin(angle), sin(angle), cos(angle));
        p += vec2(sin(p.y * 2.6 + i * 1.2 + t), cos(p.x * 3.4 + t)) * 0.02;
        float petC = 3.0 + mod(i, 2.9);
        float a = atan(p.y, p.x);
        float r = length(p);
        float petVal = mix(abs(sin(a)) * 0.5, sin((petC + 1.0) * a + t * 4.0), 1.0) * (1.0 - r);
        float edge = smoothstep(0.2, 0.16, abs(petVal - 0.5));
        vec3 layerCol = mix(vec3(1.0, 0.2, 0.4), vec3(1.0, 0.7, 0.8), sin(i + t + r * 8.0));
        col += edge * layerCol * smoothstep(0.0, 0.0, 0.8 - r) * pow(0.98, i) * 1.5;
    }
    fragColor = vec4(tanh(col * 0.3), 0.0);
}