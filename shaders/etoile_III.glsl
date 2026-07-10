// etoile III
// Shadertoy ID: t3KfDw
// Description: etoiles 
// Tags: procedural, 2d, waves, plasma, interference, minimalist, glowing, domainwarping, gold, silk

#define SAMPLES 4.0
#define ITERATIONS 20.0
#define SPEED 0.2
#define EXPOSURE 2.4
#define CA_STRENGTH 0.012
#define STAR_DENSITY 0.15
#define BLOOM_RADIUS 0.5
#define FILMIC_BENT 0.15

float hash(vec2 p) {
    uint x = floatBitsToUint(p.x);
    uint y = floatBitsToUint(p.y);
    uint h = (x * 1597334673U) ^ (y * 3812015801U);
    h = h * 1597334673U;
    return float(h) * (1.0 / 4294967296.0);
}

float starField(vec2 uv, float scale, float time) {
    vec2 id = floor(uv * scale);
    vec2 f = fract(uv * scale) - 0.5;
    float h = hash(id);
    float twinkle = sin(time * (h * 10.0) + h * 6.28) * 0.5 + 0.5;
    float d = length(f - (vec2(h, hash(id + 45.0)) - 0.5) * 0.8);
    return smoothstep(0.12 * twinkle, 0.0, d) * step(1.0 - STAR_DENSITY, h) * twinkle;
}

vec3 getSceneColor(vec2 uv, float time) {
    vec3 color = vec3(0.0);
    vec2 p = uv;
    
    float stars = starField(uv, 40.0, time) * 0.5;
    stars += starField(uv, 80.0, time * 0.6) * 0.25;
    stars += starField(uv, 150.0, time * 0.3) * 0.15;
    
    float intensity = 0.0;
    for(float i = 1.0; i < ITERATIONS; i++) {
        p += (0.06 + 0.015 * cos(time * 0.05)) * sin(p.yx * i + time * SPEED);
        float d = length(sin(p) * i);
        float pulse = 0.004 / d;
        intensity += pulse;
        color += pulse * (cos(i * 0.6 + vec3(0, 1.4, 2.8)) + 2.1);
    }
    
    vec3 nebula = color * vec3(1.2, 0.8, 0.6);
    vec3 starColor = stars * vec3(0.85, 0.95, 1.0);
    
    return mix(starColor, nebula + starColor, clamp(intensity * 1.5, 0.0, 1.0));
}

vec3 filmicToneMapping(vec3 x) {
    vec3 x_adj = max(vec3(0.0), x - 0.004);
    return (x_adj * (6.2 * x_adj + 0.5)) / (x_adj * (6.2 * x_adj + 1.7) + 0.06);
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 res = iResolution.xy;
    vec3 render = vec3(0.0);
    float t = iTime;
    float seed = hash(fragCoord + t);

    for(float m = 0.0; m < SAMPLES; m++) {
        for(float n = 0.0; n < SAMPLES; n++) {
            vec2 jitter = (vec2(m, n) + seed - 0.5) / SAMPLES;
            vec2 uv = (2.0 * (fragCoord + jitter) - res) / res.y;
            
            float r2 = dot(uv, uv);
            uv *= 1.0 + 0.05 * r2;

            vec3 col = vec3(0.0);
            float dist = length(uv);
            
            col.r = getSceneColor(uv * (1.0 + CA_STRENGTH * dist), t).r;
            col.g = getSceneColor(uv * (1.0 + (CA_STRENGTH * 0.5) * dist), t).g;
            col.b = getSceneColor(uv, t).b;
            
            render += col;
        }
    }
    
    render /= (SAMPLES * SAMPLES);
    render *= EXPOSURE;
    
    vec2 vuv = fragCoord / res;
    float vig = pow(vuv.x * (1.0 - vuv.x) * vuv.y * (1.0 - vuv.y) * 16.0, 0.25);
    render *= mix(0.3, 1.0, vig);
    
    render = filmicToneMapping(render);
    render = mix(render, vec3(dot(render, vec3(0.299, 0.587, 0.114))), -0.1); 
    render = pow(render, vec3(1.0 / 2.2));
    
    float noise = hash(fragCoord + t * 60.0);
    render += (noise - 0.5) * 0.04 * (1.0 - render);
    
    render += (hash(fragCoord) - 0.5) * (1.0 / 255.0);

    fragColor = vec4(render, 1.0);
}