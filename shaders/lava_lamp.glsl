/*
 * Simulation de lampe à lave avec des "metaballs" 2D.
 * Chaque "blob" est un point de chaleur dont l'intensité diminue
 * avec la distance. En additionnant les intensités, on obtient
 * des formes organiques qui fusionnent.
*/

float hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

float metaball(vec2 p, vec2 c, float r) {
    return r / dot(p - c, p - c);
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = fragCoord.xy / iResolution.xy;
    uv.x *= iResolution.x / iResolution.y;

    float total_intensity = 0.0;
    int num_blobs = 8;

    for (int i = 0; i < num_blobs; i++) {
        float id = float(i);
        
        float speed = hash(vec2(id, id)) * 0.2 + 0.1;
        float x_pos = hash(vec2(id, 0.0)) * 1.5 - 0.25;
        float y_pos = fract(iTime * speed + hash(vec2(0.0, id))) * 1.2 - 0.1;
        
        vec2 center = vec2(x_pos, y_pos);
        float radius = (sin(iTime * 0.5 + id * 2.0) * 0.5 + 0.5) * 0.02 + 0.01;
        
        total_intensity += metaball(uv, center, radius);
    }

    vec3 col = vec3(0.8, 0.0, 0.5); // Couleur du liquide
    if (total_intensity > 1.0) {
        col = vec3(1.0, 0.3, 0.0); // Couleur de la lave
    }
    
    col = mix(col, vec3(0.1), 1.0 - smoothstep(0.95, 1.05, total_intensity));

    fragColor = vec4(col, 1.0);
}