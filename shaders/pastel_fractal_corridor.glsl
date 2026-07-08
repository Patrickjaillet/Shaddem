// ===============================
// Pastel Fractal Corridor Shader
// Raymarching / Clean Abstract
// ===============================

vec3 palette(float d){
    vec3 a = vec3(0.65, 0.75, 0.85);
    vec3 b = vec3(0.95, 0.65, 0.85);
    vec3 c = vec3(1.0, 1.0, 1.0);
    vec3 d2 = vec3(0.00, 0.33, 0.67);
    return a + b*cos(6.28318*(c*d+d2));
}

vec2 rotate(vec2 p, float a){
    float c = cos(a);
    float s = sin(a);
    return mat2(c,-s,s,c)*p;
}

// Distance field
float map(vec3 p){
    float t = iTime * 0.25;

    // corridor twist
    p.xy = rotate(p.xy, t);
    p.xz = rotate(p.xz, t*0.7);

    float scale = 1.0;
    float d = 1e10;

    // fractal folding
    for(int i = 0; i < 7; i++){
        p = abs(p) - 0.45;
        p.xy = rotate(p.xy, 1.2 + t*0.2);
        p.xz = rotate(p.xz, 0.8);
        float di = length(p) / scale;
        d = min(d, di);
        p *= 1.35;
        scale *= 1.35;
    }

    return d - 0.02;
}

// Raymarch
vec4 rm(vec3 ro, vec3 rd){
    float t = 0.0;
    vec3 col = vec3(0.0);
    float glow = 0.0;

    for(int i = 0; i < 96; i++){
        vec3 pos = ro + rd * t;
        float d = map(pos);

        if(d < 0.001 || t > 25.0) break;

        glow += exp(-d*12.0)*0.02;
        t += d * 0.7;
    }

    float shade = exp(-t*0.08);
    col = palette(shade + glow);

    col += glow * vec3(1.0,0.9,1.1); // bloom feel
    col *= shade;

    return vec4(col,1.0);
}

// Main
void mainImage(out vec4 fragColor, in vec2 fragCoord){
    vec2 uv = (fragCoord - 0.5*iResolution.xy) / iResolution.y;

    vec3 ro = vec3(0.0, 0.0, -6.0 + sin(iTime*0.3));
    vec3 rd = normalize(vec3(uv, 1.8));

    // subtle camera drift
    rd.xy = rotate(rd.xy, sin(iTime*0.2)*0.1);

    vec4 col = rm(ro, rd);

    // soft fog
    col.rgb = mix(col.rgb, vec3(0.85,0.9,0.95), smoothstep(0.0,1.0,length(uv)));

    fragColor = col;
}

/** SHADERDATA
{
    "title": "Revolution Shift Corridor",
    "description": "Abstract pastel raymarched fractal corridor with motion and bloom",
    "model": "car"
}
*/
