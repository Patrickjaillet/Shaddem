// Electric Tunnel
// Shadertoy ID: 7csXWS
// Description: Electric Tunnel
// Tags: tunnel

#define R(a) mat2(cos(a),sin(a),-sin(a),cos(a))
#define N(p) dot(sin(p),cos((p).yzx))
#define H(p) fract(sin(dot(p,vec2(12.9,78.2)))*4e4)
#define SAMPLES 4.0
#define VOLUMETRIC_STEPS 80.0
#define EXPOSURE 1.8

float getStars(vec3 p) {
    vec3 id = floor(p * 20.0);
    vec3 f = fract(p * 20.0) - 0.5;
    float h = H(id.xy + id.z);
    return smoothstep(0.1 * h, 0.0, length(f)) * step(0.97, h);
}

vec3 renderScene(vec2 uv, float iTime, vec2 fragCoord) {
    vec2 r = iResolution.xy;
    vec3 D = normalize(vec3(uv, 1.2));
    vec3 col = vec3(0), p, q, pa, c;
    
    float time = iTime * 8.0;
    float seed = H(fragCoord + iTime);
    float t = seed * 0.22; 
    float w = 1.0;
    
    D.xy *= R(iTime * 0.15);
    float f = H(vec2(floor(time * 2.0))) * 0.8;
    if(f < 0.3) time += H(vec2(time)) * 0.2;

    for(float i = 0.0; i < VOLUMETRIC_STEPS; i++) {
        p = vec3(0, 0, time) + D * t;
        q = p; 
        float d = 0.0;
        q.xy *= R(q.z * 0.1);
        
        for(float a = 0.0; a < 12.0; a++) {
            pa = q; 
            float s = iTime * 1.5 + a;
            pa.xy *= R(pa.z * (0.05 + a * 0.03) + s * 0.4);
            pa.x += sin(pa.z * 0.4 + s + a);
            pa.y += cos(pa.z * 0.3 + s - a);
            d = max(d, exp(-length(pa.xy) * (7.0 + N(pa * 3.0) * 4.0)));
        }
        
        d *= smoothstep(0.5, -0.8, length(q.xy) - (2.5 + N(q * 0.2)));
        c = mix(vec3(0.0, 0.1, 0.4), vec3(0.5, 0.8, 1.0), d);
        
        float stars = getStars(p * 0.1) * (1.0 - d);
        c += stars * vec3(0.8, 0.9, 1.0);
        
        float flash = step(0.5, H(vec2(floor(time * 4.0), i))) * 2.0; 
        col += c * d * d * 22.0 * w * 0.02 * (1.0 + flash);
        
        w *= (1.0 - d * 0.25);
        if(w < 0.001) break;
        t += 0.22;
    }
    
    return col * f;
}

vec3 aces(vec3 x) {
    return clamp((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14), 0.0, 1.0);
}

void mainImage(out vec4 O, vec2 U) {
    vec2 r = iResolution.xy;
    vec3 finalCol = vec3(0);
    float dither = H(U + iTime);

    for(float m = 0.0; m < SAMPLES; m++) {
        vec2 offset = vec2(H(U + m), H(U.yx + m)) - 0.5;
        vec2 uv = (U + offset - 0.5 * r) / r.y;
        finalCol += renderScene(uv, iTime, U);
    }
    finalCol /= SAMPLES;
    finalCol *= EXPOSURE;

    float bloom = max(0.0, finalCol.g - 0.7) * 0.5;
    finalCol.rb += bloom * vec2(1.2, 0.8);

    finalCol = aces(finalCol);
    
    finalCol = pow(finalCol, vec3(0.4545)); 
    finalCol = mix(finalCol, vec3(dot(finalCol, vec3(0.299, 0.587, 0.114))), -0.1);
    finalCol += vec3(0.0, 0.02, 0.07) * (1.0 - exp(-length(U/r.y)));

    float grain = (H(U + iTime * 60.0) - 0.5) * 0.03;
    O = vec4(finalCol + grain, 1.0);
}