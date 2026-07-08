// 3D Starfield
// Simule un déplacement dans un champ d'étoiles.

// --- Config ---
#define STAR_COUNT 1000
#define STAR_SPEED 2.0
#define STAR_SIZE 0.02

// --- Pseudo-random function ---
float random(vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}

// --- 3D Rotation ---
mat3 rotateY(float angle) {
    float s = sin(angle);
    float c = cos(angle);
    return mat3(
        c, 0.0, -s,
        0.0, 1.0, 0.0,
        s, 0.0, c
    );
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    // --- Coordinate Setup ---
    // Normalized pixel coordinates (-1 to 1, aspect corrected)
    vec2 uv = (2.0 * fragCoord - iResolution.xy) / iResolution.y;
    
    vec3 col = vec3(0.0);
    
    // --- Camera ---
    // Simple camera moving forward
    vec3 ro = vec3(0.0, 0.0, -iTime * STAR_SPEED);
    vec3 rd = normalize(vec3(uv, 1.0)); // Ray direction
    
    // Rotate camera for more dynamic movement
    mat3 rot = rotateY(iTime * 0.1);
    ro = rot * ro;
    rd = rot * rd;

    // --- Star Rendering Loop ---
    for (int i = 0; i < STAR_COUNT; i++) {
        float fi = float(i);
        // Generate a random 3D position for each star
        vec3 star_pos = vec3(
            random(vec2(fi, fi * 0.1)) * 2.0 - 1.0,
            random(vec2(fi * 0.2, fi * 0.3)) * 2.0 - 1.0,
            random(vec2(fi * 0.4, fi * 0.5))
        ) * 20.0; // Spread stars out
        
        // Make the starfield loop by wrapping the z-coordinate
        star_pos.z = fract(star_pos.z - ro.z / 20.0) * 20.0;
        
        vec3 p = star_pos - ro;
        float pz = max(0.1, p.z); // Avoid division by zero
        float d = length(uv - p.xy / pz);
        
        float size = STAR_SIZE / pz;
        float brightness = smoothstep(size, 0.0, d);
        
        vec3 star_color = vec3(0.8, 0.9, 1.0) * (0.8 + random(vec2(fi, fi)) * 0.4);
        
        col += star_color * brightness;
    }
    
    fragColor = vec4(col, 1.0);
}