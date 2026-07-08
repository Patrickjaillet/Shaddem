// =====================================================
// Dark Chrome Mandelbulb / Julia Fractal
// Raymarching / Glossy / Cinematic
// =====================================================

// ===== PARAMETERS =====
#define ITER 8
#define POWER 8.0

// ===== COLOR PALETTE =====
vec3 palette(float d){
    vec3 a = vec3(0.04, 0.045, 0.05);
    vec3 b = vec3(0.35, 0.38, 0.42);
    vec3 c = vec3(1.0);
    vec3 d2 = vec3(0.2, 0.35, 0.55);
    return a + b*cos(6.28318*(c*d+d2));
}

vec2 rotate(vec2 p, float a){
    float c = cos(a);
    float s = sin(a);
    return mat2(c,-s,s,c)*p;
}

// ===== MANDELBULB / JULIA DE =====
float mandelbulb(vec3 p){
    vec3 z = p;
    float dr = 1.0;
    float r = 0.0;

    for(int i = 0; i < ITER; i++){
        r = length(z);
        if(r > 4.0) break;

        // Spherical coordinates
        float theta = acos(z.z / r);
        float phi = atan(z.y, z.x);

        dr = pow(r, POWER-1.0)*POWER*dr + 1.0;

        // Mandelbulb power
        float zr = pow(r, POWER);
        theta *= POWER;
        phi *= POWER;

        // Julia constant drift
        z = zr * vec3(
            sin(theta)*cos(phi),
            sin(phi)*sin(theta),
            cos(theta)
        ) + p*0.35 + vec3(0.15,0.1,0.0);
    }

    return 0.5 * log(r) * r / dr;
}

// ===== DISTANCE FIELD =====
float map(vec3 p){
    float t = iTime * 0.2;

    // Slow rotation
    p.xy = rotate(p.xy, t*0.4);
    p.xz = rotate(p.xz, t*0.3);

    // Tunnel-like stretch
    p.z *= 0.7;

    return mandelbulb(p) - 0.02;
}

// ===== NORMAL =====
vec3 normal(vec3 p){
    vec2 e = vec2(0.001,0.0);
    return normalize(vec3(
        map(p+e.xyy) - map(p-e.xyy),
        map(p+e.yxy) - map(p-e.yxy),
        map(p+e.yyx) - map(p-e.yyx)
    ));
}

// ===== FAKE ENVIRONMENT MAP =====
vec3 envMap(vec3 r){
    float h = clamp(r.y*0.5 + 0.5, 0.0, 1.0);
    vec3 sky = vec3(0.45,0.5,0.55);
    vec3 ground = vec3(0.02,0.02,0.025);
    return mix(ground, sky, h);
}

// ===== RAYMARCH =====
vec4 rm(vec3 ro, vec3 rd){
    float t = 0.0;
    vec3 col = vec3(0.0);
    float glow = 0.0;
    vec3 hitPos;
    bool hit = false;

    for(int i = 0; i < 120; i++){
        vec3 pos = ro + rd * t;
        float d = map(pos);

        if(d < 0.001){
            hitPos = pos;
            hit = true;
            break;
        }
        if(t > 40.0) break;

        glow += exp(-d*18.0)*0.025;
        t += d * 0.75;
    }

    float shade = exp(-t*0.08);
    col = palette(shade);

    // ===== DARK CHROME SHADING =====
    if(hit){
        vec3 n = normal(hitPos);
        vec3 v = normalize(-rd);

        vec3 r = reflect(rd, n);
        vec3 env = envMap(r);

        float fres = pow(1.0 - max(dot(n,v),0.0), 5.0);

        // Chrome base
        col = mix(col, env, 0.55);
        col += fres * vec3(0.5,0.6,0.7);

        // Specular highlight
        vec3 l = normalize(vec3(0.3,0.4,1.0));
        float spec = pow(max(dot(reflect(-l,n),v),0.0), 100.0);
        col += spec * vec3(1.5,1.6,1.7);
    }

    col += glow * vec3(0.6,0.8,1.1);
    col *= shade;

    return vec4(col,1.0);
}

// ===== MAIN =====
void mainImage(out vec4 fragColor, in vec2 fragCoord){
    vec2 uv = (fragCoord - 0.5*iResolution.xy) / iResolution.y;

    vec3 ro = vec3(0.0, 0.0, -4.0 + sin(iTime*0.2));
    vec3 rd = normalize(vec3(uv, 1.6));

    rd.xy = rotate(rd.xy, sin(iTime*0.25)*0.05);

    vec4 col = rm(ro, rd);

    // Cinematic vignette
    float v = smoothstep(1.3, 0.3, length(uv));
    col.rgb *= v;

    fragColor = col;
}

/** SHADERDATA
{
    "title": "Dark Chrome Mandelbulb Julia",
    "description": "Cinematic mandelbulb/julia fractal with dark chrome glossy shading",
    "model": "car"
}
*/
