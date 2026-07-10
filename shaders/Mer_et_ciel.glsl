// Mer et ciel
// Shadertoy ID: sfSGzt
// Description: Mer et ciel
// Tags: water

vec3 scene(vec2 uv) {
float dis = sin(uv.x * 18. + iTime * 4.) * exp(-abs(uv.y) * 12.0);
uv.y += dis * 0.004;

vec3 r = normalize(vec3(uv, 1.1)), farge = vec3(0), p, 
     l = normalize(vec3(.5, .12, 1.));

float sol = pow(max(0., dot(r, l)), 400.),
      mie = pow(max(0., dot(r, l)), 15.) * .4;

if (r.y > 0.) {
    farge = mix(vec3(.05, .2, .4), vec3(.7, .8, .9), exp(-r.y * 5.));
    float sk = 0., s = 1.1;
    for (int j = 0; j < 7; j++) {
        p = r * (2.2 / r.y) * .2;
        p.z += iTime * .25;
        p.xz *= mat2(1.6, -1.2, 1.2, 1.6);
        sk += abs(dot(sin(p * s), cos(p.zxy * s))) / s;
        s *= 1.8;
    }
    farge = mix(farge, vec3(1), smoothstep(.5, 1.9, sk));
    farge += sol * 5. + mie;
    float stripe = max(0., 1. - abs((uv.y - l.y) * 180.)) * max(0., 1. - abs((uv.x - l.x) * .4));
    farge += stripe * vec3(.4, .7, 1.) * sol;
} else {
    float avstand = -2.1 / r.y;
    p = r * avstand; p.z += iTime * 1.8;
    vec2 g = p.xz;
    float h = 0., f = 1.2;
    for(int j = 0; j < 6; j++) {
        h += sin(g.x * f + iTime) * cos(g.y * f) / f;
        g *= 1.6; f *= 1.4;
    }
    vec3 n = normalize(vec3(-h, 30. - avstand * .15, -h));
    vec3 ref = reflect(r, n);
    float fro = pow(1. + dot(r, n), 5.);
    farge = mix(vec3(.005, .03, .07), vec3(.3, .5, .7) - ref.y * .3, .1 + fro);
    farge += pow(max(0., dot(ref, l)), 200.) * 3.0;
    farge = mix(farge, vec3(.5, .6, .7), 1. - exp(-avstand * .03));
}
return farge;
}

void mainImage(out vec4 o, vec2 i) {
vec3 farge = vec3(0);
vec2 forskyvning = vec2(0.25, 0.75) / iResolution.y;

farge += scene((i + forskyvning.xx - .5 * iResolution.xy) / iResolution.y);
farge += scene((i + forskyvning.xy - .5 * iResolution.xy) / iResolution.y);
farge += scene((i + forskyvning.yx - .5 * iResolution.xy) / iResolution.y);
farge += scene((i + forskyvning.yy - .5 * iResolution.xy) / iResolution.y);
farge /= 4.0;

farge *= 1.1 - length((i - .5 * iResolution.xy) / iResolution.y) * .25;
o = vec4(pow(max(farge, 0.), vec3(.4545)), 1);
}