// gdgfg
// Shadertoy ID: WXyBRG
// Description: dfgdfg
// Tags: fdfg

// Techno Singularity – Abstract Live Visual
// hypnotic / minimal / club-ready

#define N normalize
#define PI 3.14159265359

mat2 rot(float a){
    float s = sin(a), c = cos(a);
    return mat2(c,-s,s,c);
}

float pattern(vec3 p){
    p.xy *= rot(p.z*.4 + iTime*.6);
    float r = length(p.xy);
    float a = atan(p.y,p.x);

    float waves = sin(r*14. - iTime*4.);
    float spokes = sin(a*10. + iTime*2.);

    return abs(waves*spokes)*.6 + r - 1.2;
}

vec3 palette(float t){
    return .5 + .5*cos(PI*2.*(vec3(.6,.3,.1)*t + vec3(0,.5,.8)));
}

void mainImage(out vec4 fragColor, in vec2 fragCoord){
    vec2 uv = (fragCoord - .5*iResolution.xy) / iResolution.y;

    vec3 ro = vec3(0,0,-3);
    vec3 rd = N(vec3(uv,1.));

    float t = 0.;
    vec3 col = vec3(0);
    float acc = 0.;

    for(int i=0;i<90;i++){
        vec3 p = ro + rd*t;
        float d = pattern(p);

        float hit = exp(-abs(d)*6.);
        float pulse = sin(iTime*2. + length(p)*4.)*.5+.5;

        vec3 c = palette(pulse + length(p)*.2);

        col += c * hit * .06;
        acc += hit;

        t += clamp(d*.5,.02,.18);
    }

    // hard contrast for club visibility
    col *= acc*1.3;
    col = pow(col, vec3(.65));

    // central strobe
    float center = smoothstep(.6,0.,length(uv));
    col += center * vec3(1.,.9,.7) * (.3 + .3*sin(iTime*6.));

    fragColor = vec4(col,1);
}