// ======================================
// Infinite Organic Tunnel – Ultra Clean
// Raymarching / Minimal / Pastel
// ======================================

vec3 palette(float d){
    vec3 a = vec3(0.75, 0.80, 0.85);
    vec3 b = vec3(0.85, 0.65, 0.75);
    vec3 c = vec3(1.0);
    vec3 d2 = vec3(0.15, 0.35, 0.55);
    return a + b*cos(6.28318*(c*d+d2));
}

vec2 rotate(vec2 p, float a){
    float c = cos(a);
    float s = sin(a);
    return mat2(c,-s,s,c)*p;
}

// Smooth organic noise
float noise(vec3 p){
    return sin(p.x)*sin(p.y)*sin(p.z);
}

// Distance field
float map(vec3 p){
    float t = iTime * 0.3;

    // Infinite forward motion
    p.z += t * 2.0;

    // Organic swirl
    float ang = p.z * 0.25 + t;
    p.xy = rotate(p.xy, ang);

    // Organic surface modulation
    float n = noise(p*1.2 + t);
    float radius = 0.7 + n*0.08;

    // Clean cylindrical tunnel
    float tunnel = abs(length(p.xy) - radius);

    return tunnel - 0.01;
}

// Raymarch
vec4 rm(vec3 ro, vec3 rd){
    float t = 0.0;
    vec3 col = vec3(0.0);
    float glow = 0.0;

    for(int i = 0; i < 96; i++){
        vec3 pos = ro + rd * t;
        float d = map(pos);

        if(d < 0.001 || t > 40.0) break;

        glow += exp(-d*16.0)*0.02;
        t += d * 0.8;
    }

    float shade = exp(-t*0.05);
    col = palette(shade + glow*0.5);

    col += glow * vec3(1.0,0.95,1.05);
    col *= shade;

    return vec4(col,1.0);
}

// Main
void mainImage(out vec4 fragColor, in vec2 fragCoord){
    vec2 uv = (fragCoord - 0.5*iResolution.xy) / iResolution.y;

    // Centered camera inside tunnel
    vec3 ro = vec3(0.0, 0.0, 0.0);
    vec3 rd = normalize(vec3(uv, 1.5));

    // Very subtle drift
    rd.xy = rotate(rd.xy, sin(iTime*0.2)*0.05);

    vec4 col = rm(ro, rd);

    // Ultra clean vignette
    float v = smoothstep(1.0, 0.25, length(uv));
    col.rgb *= v;

    fragColor = col;
}

/** SHADERDATA
{
    "title": "Infinite Organic Tunnel",
    "description": "Ultra clean infinite raymarched tunnel with organic texture",
    "model": "car"
}
*/
