// Eclat tunnel
// Shadertoy ID: 7f2SWW
// Description: Eclat tunnel
// Tags: tunnel

#define ITERATIONS 7
#define MAX_STEPS 220
#define DIST_THRESHOLD 0.0008
#define MAX_DIST 60.0
#define PI 3.14159265359

mat2 rot(float a) {
    float s = sin(a), c = cos(a);
    return mat2(c, -s, s, c);
}

float map(vec3 p) {
    float twist = p.z * 0.15;
    p.xy *= rot(twist + iTime * 0.1);
    p.z += iTime * 1.2;
    
    vec3 p_grid = mod(p + 6.0, 12.0) - 6.0;
    float s = 1.0;
    
    for(int i = 0; i < ITERATIONS; i++) {
        p_grid = abs(p_grid) - vec3(0.18, 0.32, 0.22);
        if (p_grid.x < p_grid.y) p_grid.xy = p_grid.yx;
        if (p_grid.x < p_grid.z) p_grid.xz = p_grid.zx;
        if (p_grid.y < p_grid.z) p_grid.yz = p_grid.zy;
        
        float k = 1.78 / clamp(dot(p_grid, p_grid), 0.12, 1.1);
        p_grid *= k;
        s *= k;
        p_grid -= vec3(0.6, 1.4, 0.45);
    }
    
    float geometry = (length(p_grid.xz) - 0.15) / s;
    float core_tunnel = length(p.xy) - 1.4;
    return max(geometry, -core_tunnel);
}

vec3 ace(vec3 x) {
    return clamp((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14), 0.0, 1.0);
}

void mainImage(out vec4 O, vec2 C) {
    vec2 uv = (C - 0.5 * iResolution.xy) / iResolution.y;
    float noise = fract(sin(dot(uv, vec2(12.9898, 78.233) + iTime)) * 43758.5453);
    
    float cycle = mod(iTime, 32.0);
    vec3 ro, lookAt;
    float fov = 1.2;

    if(cycle < 8.0) {
        float t = cycle / 8.0;
        ro = vec3(0.4 * sin(t * PI), 0.4 * cos(t * PI), -3.0);
        lookAt = vec3(0.0, 0.0, 5.0);
    } else if(cycle < 16.0) {
        float t = (cycle - 8.0) / 8.0;
        ro = vec3(2.5 * sin(t * PI * 0.5), 1.5 * cos(t * PI * 0.5), -2.0 + t * 4.0);
        lookAt = vec3(0.0, 0.0, ro.z + 10.0);
        fov = 1.8;
    } else if(cycle < 24.0) {
        float t = (cycle - 16.0) / 8.0;
        ro = vec3(sin(iTime * 0.5) * 1.1, cos(iTime * 0.3) * 1.1, -1.0);
        lookAt = vec3(0.0, 0.0, 20.0);
        fov = 0.7;
    } else {
        float t = (cycle - 24.0) / 8.0;
        ro = vec3(0.0, 0.0, -5.0 + t * 15.0);
        lookAt = vec3(sin(iTime), cos(iTime), ro.z + 5.0);
    }

    vec3 f = normalize(lookAt - ro);
    vec3 r = normalize(cross(vec3(sin(iTime * 0.2), 1.0, 0.0), f));
    vec3 u = cross(f, r);
    vec3 rd = normalize(f * fov + uv.x * r + uv.y * u);
    
    float t_march = 0.0, vol = 0.0, beam = 0.0, bloom = 0.0;
    
    for(int i = 0; i < MAX_STEPS; i++) {
        vec3 p = ro + t_march * rd;
        float d = map(p);
        
        float weight = exp(-t_march * 0.08);
        vol += exp(-max(0.0, d) * 35.0) * weight;
        
        float shell = length(p.xy);
        beam += (0.005 / (0.005 + abs(shell - 1.1))) * weight;
        bloom += (0.002 / (0.002 + d * d)) * weight;
        
        t_march += max(DIST_THRESHOLD, d * 0.45);
        if(t_march > MAX_DIST) break;
    }

    vec3 color_a = vec3(0.1, 0.4, 1.0);
    vec3 color_b = vec3(1.0, 0.2, 0.6);
    vec3 color_c = vec3(0.4, 1.0, 0.5);
    
    float mix_val = sin(iTime * 0.4) * 0.5 + 0.5;
    vec3 current_palette = mix(color_a, mix(color_b, color_c, mix_val), mix_val);
    
    vec3 col = current_palette * vol * 0.18;
    col += vec3(0.3, 0.7, 1.0) * beam * 0.05;
    col += current_palette * bloom * 0.02;
    
    float r_dist = length(uv);
    float flare = 0.012 / (r_dist + 0.01);
    col += vec3(1.0, 0.9, 0.7) * pow(flare, 1.6) * (1.0 + 0.3 * sin(iTime * 20.0));
    
    col = ace(col * 2.8);
    col = pow(col, vec3(0.4545));
    
    col *= smoothstep(1.6, 0.3, r_dist);
    col *= 0.92 + 0.08 * noise;
    
    float transition = smoothstep(0.1, 0.0, abs(mod(cycle, 8.0)));
    col = mix(col, vec3(0.0), transition);
    
    O = vec4(col, 1.0);
}