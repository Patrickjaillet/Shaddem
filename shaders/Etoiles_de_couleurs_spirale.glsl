// Etoiles de couleurs spirale
// Shadertoy ID: N3XGzH
// Description: Etoiles de couleurs spirale
// Tags: stars

void mainImage(out vec4 o, vec2 u) {
    vec2 r = iResolution.xy;
    vec3 rd = normalize(vec3(u + u - r, r.y)), c = vec3(0), p, q;
    float d, s, t = iTime, i, l, w, k;
    for(k = 0.; k < 1.; k += .2) {
        float m = t + k * .02;
        for(i = d = 0.; i++ < 80. && d < 25.; d += max(s * .5, .001)) {
            p = vec3(0, 0, m * .8) + rd * d;
            p.xy *= mat2(cos(p.z * .2 + m * .1 + vec4(0, 33, 11, 0)));
            p.xz *= mat2(cos(m * .05 + vec4(0, 33, 11, 0)));
            q = mod(p, 2.) - 1.;
            w = 1.;
            for(int j = 0; j < 7; j++) {
                q = abs(q) - .5;
                q.xy *= mat2(.87, .5, -.5, .87);
                q *= l = 2.2 / clamp(dot(q, q), .05, 1.);
                w *= l;
            }
            s = length(q) / w;
            c += (.5 + .5 * cos(p.z * .5 + m + vec3(0, 2, 4))) * (.0008 / (.0008 + s * s)) * exp(-d * .15);
        }
    }
    o = vec4(tanh(pow(c * .12, vec3(.7)) * (1.2 + .8 * sin(t + vec3(0, 1, 2)))), 1);
}