// demonflam
// Shadertoy ID: f3SSWR
// Description: demonflam
// Tags: fractal

vec3 rotateX(vec3 p, float a) {
    float c = cos(a), s = sin(a);
    return vec3(p.x, c * p.y - s * p.z, s * p.y + c * p.z);
}

vec3 rotateY(vec3 p, float a) {
    float c = cos(a), s = sin(a);
    return vec3(c * p.x + s * p.z, p.y, -s * p.x + c * p.z);
}

vec3 rotateZ(vec3 p, float a) {
    float c = cos(a), s = sin(a);
    return vec3(c * p.x - s * p.y, s * p.x + c * p.y, p.z);
}

float smin(float a, float b, float k) {
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    return mix(b, a, h) - k * h * (1.0 - h);
}

float hash(vec3 p) {
    p = fract(p * vec3(443.8975, 397.2973, 491.1871));
    p += dot(p.xyz, p.yzx + vec3(19.19));
    return fract(p.x * p.y * p.z);
}

float noise(vec3 p) {
    vec3 i = floor(p);
    vec3 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    return mix(mix(mix(hash(i + vec3(0.0, 0.0, 0.0)), hash(i + vec3(1.0, 0.0, 0.0)), f.x),
                   mix(hash(i + vec3(0.0, 1.0, 0.0)), hash(i + vec3(1.0, 1.0, 0.0)), f.x), f.y),
               mix(mix(hash(i + vec3(0.0, 0.0, 1.0)), hash(i + vec3(1.0, 0.0, 1.0)), f.x),
                   mix(hash(i + vec3(0.0, 1.0, 1.0)), hash(i + vec3(1.0, 1.0, 1.0)), f.x), f.y), f.z);
}

float fbm(vec3 p) {
    float v = 0.0;
    float a = 0.5;
    vec3 shift = vec3(100.0);
    mat3 m = mat3(0.00, 0.80, 0.60,
                 -0.80, 0.36, -0.48,
                 -0.60, -0.48, 0.64);
    for (int i = 0; i < 6; ++i) {
        v += a * noise(p);
        p = m * p * 2.15 + shift;
        a *= 0.45;
    }
    return v;
}

float sdCapsule(vec3 p, vec3 a, vec3 b, float ra, float rb) {
    vec3 ab = b - a;
    vec3 ap = p - a;
    float h = clamp(dot(ap, ab) / dot(ab, ab), 0.0, 1.0);
    float r = mix(ra, rb, h);
    return length(ap - ab * h) - r;
}

// Vraie corne de démon : courbée vers l'arrière et l'extérieur, base épaisse,
// pointe fine, légèrement torsadée. Construite par segments de capsule le long
// d'une trajectoire paramétrique (pas un simple cône droit).
float sdHorn(vec3 p, float side) {
    float d = 1e5;
    const int N = 7;
    vec3 prevPos = vec3(0.0);
    float prevR = 0.17;
    for (int i = 0; i <= N; i++) {
        float u = float(i) / float(N); // 0 = base, 1 = pointe
        // trajectoire : monte, part vers l'extérieur, puis se recourbe vers l'arrière
        float curl = u * u * 1.6;          // courbure qui s'accentue vers la pointe
        float outward = sin(u * 1.4) * 0.22;
        vec3 pos = vec3(
            side * outward,
            u * 0.85,
            -sin(curl) * 0.5 - u * 0.05     // recourbe vers l'arrière (-z)
        );
        // légère rotation autour de l'axe Y pour une torsion type corne de bélier
        float twist = u * 0.5 * side;
        pos.xz = mat2(cos(twist), -sin(twist), sin(twist), cos(twist)) * pos.xz;
        
        float r = mix(0.17, 0.012, pow(u, 0.85)); // base épaisse, pointe très fine
        
        if (i > 0) {
            d = min(d, sdCapsule(p, prevPos, pos, prevR, r));
        }
        prevPos = pos;
        prevR = r;
    }
    return d;
}

float sdDemon(vec3 p, out float eyeGlow, out float hornD) {
    vec3 hp = p;
    hp.y -= 0.1;
    float head = length(hp / vec3(0.68, 0.78, 0.63)) - 0.75;
    
    vec3 ep = hp;
    ep.x = abs(ep.x);
    float eyes = length(ep - vec3(0.24, 0.18, 0.42)) - 0.12;
    head = max(head, -eyes);
    // distance "pleine" à la pupille, pour une lueur ponctuelle au fond de l'orbite
    float eyeCore = length(ep - vec3(0.24, 0.18, 0.46)) - 0.05;
    eyeGlow = eyeCore;
    
    vec3 mp = hp;
    float mouth = length(mp - vec3(0.0, -0.28, 0.42)) - 0.22;
    head = max(head, -mouth);
    
    vec3 cp = p - vec3(0.0, 0.55, -0.05);
    float side = sign(cp.x) == 0.0 ? 1.0 : sign(cp.x);
    cp.x = abs(cp.x) - 0.28; // écarte les deux cornes de l'axe central
    cp = rotateX(cp, -0.35); // incline la base vers l'arrière
    float horn = sdHorn(cp, side);
    hornD = horn;
    
    return smin(head, horn, 0.08);
}

float map(vec3 p, float t, out float dota, out float isDemon, out float eyeGlow, out float hornD) {
    vec3 q = p;
    q.y -= t * 2.8;
    float n = fbm(q * 1.5);
    
    float dDem = sdDemon(p, eyeGlow, hornD);
    
    vec3 pf = p;
    pf.xz = rotateY(vec3(pf.xz, 0.0), t * 0.2).xy;
    
    float scale = 1.0;
    dota = 0.0;
    
    for(int i = 0; i < 6; i++) {
        pf = abs(pf) - vec3(0.12, 0.45, 0.12);
        float r2 = dot(pf, pf);
        dota += r2;
        if (r2 < 0.01) {
            pf /= 0.01;
            scale /= 0.01;
        } else if (r2 < 1.0) {
            pf /= r2;
            scale /= r2;
        }
        pf = rotateX(pf, 0.35 + sin(t * 0.25 + float(i)) * 0.06);
        pf = rotateZ(pf, 0.18);
        pf = pf * 1.4 - vec3(0.04, 0.35, 0.04);
        scale *= 1.4;
    }
    
    float dFract = (length(pf.xz) - 0.05 * (1.1 - smoothstep(-3.0, 4.0, pf.y))) / scale;
    dFract -= n * 0.4 * (1.0 - smoothstep(1.5, 5.0, q.y));

    // --- MODIF 1 ---
    // On élargit fortement la marge de priorité du démon (était 0.08 * n)
    // pour qu'il "perce" la masse fractale au lieu d'être noyé dedans.
    if (dDem - 0.35 < dFract) {
        isDemon = 1.0;
        // On ne réduit presque plus le pas pour le démon (était * 0.65)
        // -> silhouette beaucoup plus nette et solide.
        return dDem * 0.9;
    } else {
        isDemon = 0.0;
        return dFract * 0.45;
    }
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    float t = iTime;
    
    vec3 ro = vec3(0.0, 0.8, 3.2);
    vec3 rd = normalize(vec3(uv, -1.6));
    
    ro = rotateY(ro, sin(t * 0.15) * 0.15);
    rd = rotateY(rd, sin(t * 0.15) * 0.15);
    ro = rotateX(ro, 0.02);
    rd = rotateX(rd, 0.02);
    
    float d = 0.0;
    float t_max = 8.0;
    vec4 col = vec4(0.0);
    
    for (int i = 0; i < 140; i++) {
        if (d > t_max || col.a >= 0.99) break;
        
        vec3 pos = ro + rd * d;
        float dota, isDemon, eyeGlow, hornD;
        float h = map(pos, t, dota, isDemon, eyeGlow, hornD);
        
        if (h < 0.25) {
            float density = (0.25 - h) * step(h, 0.25);
            float fbm_noise = fbm(pos * 4.0 - vec3(0.0, t * 2.5, 0.0));
            
            float heat = density * (0.3 + 0.7 * fbm_noise);
            
            if (isDemon > 0.5) {
                // --- MODIF 2 ---
                // Heat du démon poussé plus haut et moins dilué par mix(heat*0.5,...)
                // pour qu'il reste lumineux/distinct même loin du coeur (h proche de 0.25).
                heat = mix(heat * 0.9, 0.95 + 0.15 * sin(t * 5.0 + pos.y * 4.0 + pos.x * 2.0), smoothstep(0.3, -0.02, h));
            }
            
            heat *= smoothstep(4.0, -2.0, pos.y);
            
            vec3 c_blue   = vec3(0.1, 0.6, 2.0);
            vec3 c_white  = vec3(1.8, 1.6, 1.2);
            vec3 c_gold   = vec3(1.0, 0.5, 0.0);
            vec3 c_red    = vec3(0.75, 0.01, 0.005);
            vec3 c_dark   = vec3(0.02, 0.005, 0.01);
            
            vec3 f_col = vec3(0.0);
            
            if (heat < 0.15) {
                f_col = mix(c_dark, c_red, smoothstep(0.02, 0.15, heat));
            } else if (heat < 0.4) {
                f_col = mix(c_red, c_gold, smoothstep(0.15, 0.4, heat));
            } else if (heat < 0.7) {
                f_col = mix(c_gold, c_white, smoothstep(0.4, 0.7, heat));
            } else {
                f_col = mix(c_white, c_blue, smoothstep(0.7, 1.0, heat));
            }
            
            if (isDemon > 0.5) {
                vec3 demonCoreColor = mix(vec3(0.9, 0.05, 0.01), vec3(2.5, 1.8, 0.5), sin(t * 8.0 + pos.z * 5.0) * 0.5 + 0.5);
                f_col = mix(f_col, demonCoreColor, smoothstep(0.3, 0.0, h));
                f_col += vec3(0.8, 0.1, 0.0) * smoothstep(0.2, 0.0, h) * (1.0 + fbm_noise);

                // --- MODIF 3 ---
                // Un fin contour incandescent (rim) autour de la silhouette du démon
                // pour le détacher visuellement de la masse de flammes derrière lui.
                float rim = smoothstep(0.08, 0.0, abs(h)) ;
                f_col += vec3(1.2, 0.6, 0.1) * rim * 1.5;

                // --- CORNES ---
                // Kératine sombre quasi noire, légèrement irisée d'ambre près de la pointe,
                // pour qu'elles se détachent nettement de la peau incandescente du crâne.
                float hornMask = smoothstep(0.10, -0.02, hornD);
                vec3 hornColor = mix(vec3(0.05, 0.02, 0.01), vec3(0.4, 0.15, 0.03), 0.5 + 0.5 * fbm_noise);
                f_col = mix(f_col, hornColor, hornMask * smoothstep(0.25, 0.0, h));
                f_col += vec3(0.6, 0.25, 0.05) * hornMask * smoothstep(0.04, 0.0, abs(h)) * 0.8;

                // --- YEUX ---
                // Lueur perçante au fond des orbites, indépendante du heat ambiant,
                // pour donner un regard incandescent qui transperce la silhouette.
                float eyeMask = smoothstep(0.06, -0.01, eyeGlow);
                vec3 eyeColor = vec3(3.0, 2.2, 0.3) + vec3(0.0, 0.0, 1.0) * (sin(t * 6.0) * 0.5 + 0.5) * 0.4;
                f_col = mix(f_col, eyeColor, eyeMask);
                f_col += eyeColor * eyeMask * 1.5;
            } else {
                f_col += vec3(0.04, 0.12, 0.35) * (dota * 0.004) * step(0.6, heat);
            }
            
            // --- MODIF 4 ---
            // Alpha du démon nettement augmenté (0.65 -> 0.92) et seuil de heat
            // abaissé pour que sa silhouette soit quasi opaque, contrairement
            // au fractal qui reste translucide (0.18 inchangé).
            float alpha = smoothstep(-0.05, 0.05, heat) * (isDemon > 0.5 ? 0.92 : 0.18);
            col.rgb += (1.0 - col.a) * f_col * alpha * 6.5;
            col.a += (1.0 - col.a) * alpha;
        }
        
        d += max(h * 0.18, 0.008);
    }
    
    col.rgb = vec3(1.0) - exp(-col.rgb * 1.5);
    col.rgb = pow(col.rgb, vec3(1.0 / 2.2));
    
    float vignette = length(uv);
    col.rgb = mix(col.rgb, vec3(0.002, 0.001, 0.004), smoothstep(0.35, 0.95, vignette));
    
    fragColor = vec4(col.rgb, 1.0);
}
