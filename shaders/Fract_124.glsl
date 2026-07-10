// Fract 124
// Shadertoy ID: 7XjSWK
// Description: Fract 123
// Tags: fractal

void mainImage(out vec4 O, vec2 I) {
    vec2 r = iResolution.xy, 
         u = (I - .5 * r) / r.y;
    vec3 o = vec3(0, 0, iTime * 1.8), d, p, c;
    o.xy += vec2(sin(o.z * .4), cos(o.z * .3)) * .4;
    
    vec3 f = normalize(o + vec3(sin((o.z + 1.) * .4), cos((o.z + 1.) * .3), 1) - o),
         s = normalize(cross(vec3(sin(iTime * .8) * .2, 1, 0), f));
    d = normalize(f + u.x * s + u.y * cross(f, s));
    
    float a = o.z * .15, C = cos(a), S = sin(a), t = .1, i = 0.;
    d.xy *= mat2(C, -S, S, C);
    
    for (O *= i; i++ < 100. && t < 25. && O.a < .95;) {
        p = o + d * t;
        a = p.z * .25 + iTime * .4;
        C = cos(a); S = sin(a);
        p.xy *= mat2(C, -S, S, C);
        
        vec3 h = vec3(abs(mod(atan(p.y, p.x) * 1.5915, .8) - .4), length(p.xy) - 1.5, p.z);
        float n = sin(h.x * 18. + h.z * 2.) * cos(h.y * 24. - iTime * 3.) * sin(h.z * 6. + h.x * 12.),
              g = exp(-abs(min(abs(h.y) - .08 + n * .04, length(h.xy) - .05 + n * .02)) * 65.) * .18,
              v = exp(-pow(min(abs(h.y) - .08 + n * .04, length(h.xy) - .05 + n * .02), 2.) * 400.) * .45 + (min(abs(h.y) - .08 + n * .04, length(h.xy) - .05 + n * .02) < .01 ? .5 : 0.),
              e = (v + g) * .35;
              
        O += vec4(((5. + 5. * cos(p.z * .35 + vec3(0, 2.1, 4.2) + iTime)) * v + (5. + 5. * sin(h.z * .6 + vec3(1, 3.5, 5))) * g * 2. + pow(max(0., n), 5.) * .6) * exp(-t * .08) * e, e) * (1. - O.a);
        t += max(.02, abs(min(abs(h.y) - .08 + n * .04, length(h.xy) - .05 + n * .02)) * .7);
    }
    O.rgb = mix(O.rgb + vec3(.02, .01, .04) * (1. - O.a), vec3(dot(O.rgb, vec3(.2126, .7152, .0722))), -.15);
    O.rgb = pow(O.rgb, vec3(.85));
    O.rgb = O.rgb * O.rgb * (3. - 2. * O.rgb);
}