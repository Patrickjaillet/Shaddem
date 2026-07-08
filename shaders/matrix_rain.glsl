// Matrix Rain Effect
// Creates a falling character effect inspired by The Matrix.

// --- Configuration ---
// You can tweak these values to change the appearance.
#define FONT_SIZE vec2(10.0, 18.0) // Size of each character cell
#define RAIN_SPEED 0.3             // How fast the rain falls
#define FADE_SPEED 0.15            // How quickly the tail fades
#define CHARACTER_FLICKER_SPEED 15.0 // How fast the characters change


// --- Pseudo-random function ---
// Generates a random-looking number from a 2D vector.
float random(vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}


void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    // --- Grid setup ---
    // Calculate integer grid coordinates for the current fragment (which cell we are in)
    vec2 st = floor(fragCoord / FONT_SIZE);
    // Calculate the UV coordinates within that cell (from 0.0 to 1.0)
    vec2 uv_char = fract(fragCoord / FONT_SIZE);

    // --- Raindrop logic ---
    // Get a single random value for each column. This makes all cells in a column behave together.
    float col_rand = random(vec2(st.x, 0.0));
    
    // Calculate the Y position of the drop's "head".
    // It moves from top to bottom based on time and the column's random value.
    // `* 1.5 - 0.5` makes it start and end off-screen for a smoother loop.
    float head_y = fract(col_rand - iTime * RAIN_SPEED * (0.5 + col_rand)) * 1.5 - 0.5;
    
    // Get the current cell's Y position, normalized from 0.0 (top) to 1.0 (bottom).
    float cell_y = st.y * FONT_SIZE.y / iResolution.y;

    // Calculate the distance from the cell to the drop's head.
    float dist = cell_y - head_y;

    // --- Brightness calculation ---
    // The tail is only visible if the cell is "below" the head (dist > 0).
    float intensity = 0.0;
    if (dist > 0.0 && dist < 1.0) { // Only draw tails up to 1.0 screen height long
        // Use exponential decay to make the tail fade out naturally.
        intensity = exp(-dist * FADE_SPEED * iResolution.y / FONT_SIZE.y);
    }
    
    // --- Character rendering ---
    // Get a random "character code" that flickers rapidly over time.
    float char_code = random(st + floor(iTime * CHARACTER_FLICKER_SPEED));
    
    // Use a noise texture (iChannel1) as a procedural font. We sample a tiny, unique part of it for each character.
    float glyph = texture(iChannel1, (uv_char * 0.1 + vec2(char_code, char_code) * 0.2)).r;
    glyph = step(0.6, glyph); // Make the character binary (on/off pixels).

    // --- Final color calculation ---
    // The head of the drop is bright white/light green, fading to Matrix green.
    vec3 color = mix(vec3(0.0, 1.0, 0.2), vec3(0.8, 1.0, 0.8), max(0.0, 1.0 - dist * 20.0));
    
    // The final pixel color is the character shape, multiplied by the tail's intensity and color.
    fragColor = vec4(glyph * intensity * color, 1.0);
}