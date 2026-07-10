// ciel bleu
// Shadertoy ID: 7ffSz8
// Description: dfg
// Tags: dfg

float aleatoire(vec2 p) {
    return fract(sin(dot(p.xy, vec2(11.9898, 78.233))) * 434248.5453123);
}

float bruit(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    float a = aleatoire(i);
    float b = aleatoire(i + vec2(1.0, 0.0));
    float c = aleatoire(i + vec2(0.0, 1.0));
    float d = aleatoire(i + vec2(1.0, 1.0));
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
}

float fbm(vec2 p) {
    float valeur = 0.0;
    float amplitude = 0.5;
    for (int i = 0; i < 6; i++) {
        valeur += amplitude * bruit(p);
        p *= 2.0;
        amplitude *= 0.5;
    }
    return valeur;
}

void mainImage(out vec4 couleurFrag, in vec2 coordFrag) {
    int taillePixel = 5;
    vec2 sp;
    
    sp.x = float(int(coordFrag.x) - (int(coordFrag.x) % taillePixel));
    sp.y = float(int(coordFrag.y) - (int(coordFrag.y) % taillePixel));
    
    vec2 st = sp.xy / iResolution.xy;

    vec3 couleur3 = vec3(61.0, 101.0, 145.0) / 255.0;
    vec3 couleur4 = vec3(90.0, 128.0, 151.0) / 255.0;

    float nuages = smoothstep(0.4, 1.0, st.y) * 0.8;
    vec2 cp = st + vec2(iTime / 10.0, 0.0);
    float rnd = bruit(cp * 7.5);
    float fbmValeur = fbm((st + vec2(iTime / 30.0, 0.0)) * 6.0);

    vec3 couleurFinale = (vec3(rnd) * nuages * fbmValeur) + mix(couleur3, couleur4, st.y);
    
    couleurFrag = vec4(couleurFinale, 1.0);
}