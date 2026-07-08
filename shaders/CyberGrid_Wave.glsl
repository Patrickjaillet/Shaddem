// Structure basée sur : dark_chrome_mandelbulb_julia.hlsl [cite: 1]
#define GRID_SIZE 1.0

vec3 palette(float d){
    return vec3(0.1, 0.0, 0.2) + vec3(0.5, 0.0, 0.5)*cos(6.28*(vec3(1.0)*d+vec3(0.0, 0.33, 0.67)));
}

float map(vec3 p){
    float wave = sin(p.x * 0.5 + iTime) * 0.2 + cos(p.z * 0.5 + iTime) * 0.2;
    return p.y + 1.0 + wave; // Plan infini avec déformation
}

void mainImage(out vec4 fragColor, in vec2 fragCoord){
    vec2 uv = (fragCoord - 0.5*iResolution.xy) / iResolution.y;
    vec3 ro = vec3(0.0, 0.5, -5.0);
    vec3 rd = normalize(vec3(uv, 1.0));
    
    float t = 0.0;
    for(int i = 0; i < 64; i++){
        float d = map(ro + rd * t);
        if(d < 0.001 || t > 20.0) break;
        t += d;
    }
    
    vec3 pos = ro + rd * t;
    float line = smoothstep(0.02, 0.0, abs(sin(pos.x * 2.0)) * abs(sin(pos.z * 2.0)));
    vec3 col = palette(t * 0.1) * line;
    col += vec3(1.0, 0.2, 0.8) * exp(-t * 0.2) * 0.5; // Glow horizon
    
    fragColor = vec4(col, 1.0);
}