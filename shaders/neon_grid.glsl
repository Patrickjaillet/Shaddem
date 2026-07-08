/*
 * Grille de néon infinie en perspective.
 * Simule un déplacement de caméra au-dessus d'un plan infini
 * en utilisant la projection de rayons.
*/

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord.xy * 2.0 - iResolution.xy) / iResolution.y;

    // Caméra qui avance en modifiant sa position sur l'axe Z
    vec3 ro = vec3(0.0, 0.0, -iTime * 2.0);
    vec3 rd = normalize(vec3(uv, 1.0)); // Rayon partant de la caméra vers le pixel

    // Intersection du rayon avec un plan horizontal (le sol)
    float t = (-1.0 - ro.y) / rd.y;
    vec3 pos = ro + rd * t;

    vec3 col = vec3(0.0);
    if (t > 0.0) {
        vec2 grid = fract(pos.xz * 0.5);
        float line = min(grid.x, grid.y);
        line = min(line, 1.0 - grid.x);
        line = min(line, 1.0 - grid.y);

        col = vec3(0.1, 0.5, 1.0) * 0.02 / line;
        col *= smoothstep(20.0, 5.0, t); // Fondu au loin
    }

    fragColor = vec4(col, 1.0);
}