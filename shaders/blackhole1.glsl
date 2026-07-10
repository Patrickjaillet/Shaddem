// blackhole1
// Shadertoy ID: w3cfWj
// Description: blackhole
// Tags: blackhole

#define R iResolution.xy
#define T iTime
#define L length
#define H(p) fract(sin(dot(p, vec3(12.9, 78.2, 43.7))) * 43758.5)

float N(vec3 p) {
    vec3 i = floor(p), f = fract(p);
    f *= f * f * (f * (f * 6. - 15.) + 10.);
    vec4 a = vec4(H(i), H(i+vec3(1,0,0)), H(i+vec3(0,1,0)), H(i+vec3(1,1,0))),
         b = vec4(H(i+vec3(0,0,1)), H(i+vec3(1,0,1)), H(i+vec3(0,1,1)), H(i+vec3(1,1,1)));
    return mix(mix(mix(a.x,a.y,f.x),mix(a.z,a.w,f.x),f.y), mix(mix(b.x,b.y,f.x),mix(b.z,b.w,f.x),f.y), f.z);
}

float F(vec3 p, float l) {
    float f = 0., a = .5;
    for(int i=0; i<6; i++) { 
        if(float(i)>l) break; 
        f += a*N(p); p *= 2.05; a *= .5; 
    }
    return f;
}

void mainImage(out vec4 o, vec2 c) {
    vec2 u = (c-.5*R)/R.y;
    float t = T*.1, tr = 1., h = .1, r, i=0.;
    vec3 ro = vec3(cos(t)*20., 4.+sin(t*.7)*2., sin(t)*20.),
         cw = normalize(-ro),
         cu = normalize(cross(cw, vec3(0,1,0))),
         rd = normalize(u.x*cu + u.y*cross(cu,cw) + 2.*cw),
         p = ro + rd * H(vec3(c, T))*.08, cl = vec3(0);

    for(; i<110.; i++) {
        r = L(p);
        if(r < 1.02) { tr = 0.; break; }
        rd = normalize(rd - 1.5 * p / (r*r*r) * h);
        p += rd * h;
        h = max(.02, .07 * r);

        if(abs(p.y) < .35 && r > 2.6 && r < 12.) {
            float v = dot(rd, normalize(cross(vec3(0,1,0), p))),
                  a = atan(p.z, p.x), s = 4.5/sqrt(r),
                  l = 6.-log2(1.+r*.25),
                  de = (F(vec3(r*.4, p.y*2.5, a*2.5-T*s), l) + F(vec3(r*.4, p.y*2.5, a*2.5-(T-.01)*s), l))*.5;
            de *= smoothstep(.35, 0., abs(p.y)) * smoothstep(12., 9.5, r);
            float al = de * .15 * (2./r);
            cl += mix(vec3(1,.25,.05), vec3(1,.95,.6), de) * (1.+v*.9) * de * 5.5 * exp(-.12*r) * al * tr;
            tr *= (1.-al);
            if(tr < .01) break;
        }
    }

    if(tr > .01) {
        vec2 sU = rd.xy / (abs(rd.z) + .15);
        for(float j=0.; j<3.; j++) {
            vec2 q = sU * (25.+j*15.), g = floor(q);
            float n = fract(sin(dot(g+j*33., vec2(12.9, 78.2)))*437.5);
            if(n < .005*(j+1.)) cl += smoothstep(.06, .01, L(fract(q)-.5)) * vec3(.8,.9,1) * smoothstep(-1.,1.,sin(T*2.+n*15.)) * (1.2/(j+1.)) * tr;
        }
    }

    vec3 bl = vec3(0), st = vec3(0);
    for(float j=-2.; j<=2.; j++) bl += cl * exp(-j*j); 
    for(float j=-10.; j<10.; j++) {
        vec3 sc = max(vec3(0), cl - 1.);
        st += sc * mix(vec3(.7,.9,1), vec3(0,.4,1), abs(j)/10.) * (1.-abs(j)/10.1);
    }
    
    cl = cl*.6 + bl*.05 + st*.1;
    cl = mix(vec3(dot(cl, vec3(.21, .71, .07))), cl, 1.25);
    cl = clamp((cl*(2.51*cl+.03))/(cl*(2.43*cl+.59)+.14), 0., 1.);
    
    o = vec4(pow(cl * (1.1 - .7*L(u)), vec3(1./2.2)), 1);
}