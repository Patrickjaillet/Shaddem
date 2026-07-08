// Tunnel 3D avec textures fractales et effets

float hash(float n) {
    return fract(sin(n) * 43758.5453123);
}

float hash2(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

// Bruit de Perlin
float noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float n = i.x + i.y * 57.0;
    return mix(mix(hash(n), hash(n + 1.0), f.x),
               mix(hash(n + 57.0), hash(n + 58.0), f.x), f.y);
}

// FBM (Fractional Brownian Motion)
float fbm(vec2 p) {
    float value = 0.0;
    float amplitude = 0.5;
    for (int i = 0; i < 5; i++) {
        value += amplitude * noise(p);
        p *= 2.0;
        amplitude *= 0.5;
    }
    return value;
}

// Voronoi pour texture cellulaire
vec2 voronoi(vec2 x) {
    vec2 n = floor(x);
    vec2 f = fract(x);
    
    float minDist = 8.0;
    vec2 minPoint;
    
    for (int j = -1; j <= 1; j++) {
        for (int i = -1; i <= 1; i++) {
            vec2 g = vec2(float(i), float(j));
            vec2 o = vec2(hash2(n + g), hash2(n + g + vec2(1.0, 0.0)));
            o = 0.5 + 0.5 * sin(iTime * 0.5 + 6.2831 * o);
            vec2 r = g + o - f;
            float d = dot(r, r);
            if (d < minDist) {
                minDist = d;
                minPoint = n + g + o;
            }
        }
    }
    
    return vec2(sqrt(minDist), 0.0);
}

// Génère un offset de virage
vec2 getTunnelOffset(float z) {
    float t = z * 0.15;
    float ox = sin(t * 0.9 + sin(t * 1.4) * 0.5) * 1.8;
    float oy = cos(t * 0.7 + cos(t * 1.1) * 0.6) * 1.5;
    return vec2(ox, oy);
}

// Distance au tunnel
float sdTunnel(vec3 p) {
    vec2 offset = getTunnelOffset(p.z);
    float radius = 2.2;
    float distToCenter = length(p.xy - offset);
    return distToCenter - radius;
}

// Raymarching
float trace(vec3 ro, vec3 rd) {
    float t = 0.0;
    for (int i = 0; i < 120; i++) {
        vec3 p = ro + rd * t;
        float d = sdTunnel(p);
        if (abs(d) < 0.0005) break;
        if (t > 60.0) break;
        t += d * 0.6;
    }
    return t;
}

// Normale
vec3 calcNormal(vec3 p) {
    vec2 e = vec2(0.001, 0.0);
    return normalize(vec3(
        sdTunnel(p + e.xyy) - sdTunnel(p - e.xyy),
        sdTunnel(p + e.yxy) - sdTunnel(p - e.yxy),
        sdTunnel(p + e.yyx) - sdTunnel(p - e.yyx)
    ));
}

// Texture mathématique complexe
vec3 getTextureColor(vec3 p, vec3 n) {
    vec2 offset = getTunnelOffset(p.z);
    
    // Coordonnées UV sur le cylindre
    float angle = atan(p.y - offset.y, p.x - offset.x);
    vec2 uv = vec2(angle * 2.0, p.z * 0.5 - iTime * 3.0);
    
    // FBM pour texture organique
    float pattern1 = fbm(uv * 3.0);
    float pattern2 = fbm(uv * 5.0 + vec2(10.0, 5.0));
    float pattern3 = fbm(uv * 8.0 - vec2(5.0, 10.0));
    
    // Voronoi pour motifs cellulaires
    vec2 vor = voronoi(uv * 4.0);
    float voronoiPattern = vor.x;
    
    // Lignes de flux
    float flow = sin(uv.x * 10.0 + uv.y * 5.0 + iTime * 2.0) * 0.5 + 0.5;
    flow = pow(flow, 3.0);
    
    // Turbulence
    float turb = abs(sin(pattern1 * 10.0 + iTime) * sin(pattern2 * 8.0));
    
    // Grille hexagonale
    vec2 hexUV = uv * 12.0;
    float hexPattern = abs(sin(hexUV.x * 1.732) * sin(hexUV.y + hexUV.x * 0.5));
    hexPattern = smoothstep(0.7, 0.8, hexPattern);
    
    // Combiner les patterns
    float finalPattern = pattern1 * 0.3 + pattern2 * 0.2 + pattern3 * 0.2;
    finalPattern += voronoiPattern * 0.5;
    finalPattern += turb * 0.4;
    finalPattern += flow * 0.3;
    finalPattern = fract(finalPattern);
    
    // Palette de couleurs dynamique
    float colorTime = p.z * 0.08 - iTime * 0.3;
    vec3 col1 = vec3(0.1, 0.4, 0.9);  // Bleu électrique
    vec3 col2 = vec3(0.9, 0.1, 0.5);  // Rose néon
    vec3 col3 = vec3(0.2, 0.9, 0.6);  // Cyan vert
    vec3 col4 = vec3(0.8, 0.3, 0.1);  // Orange
    
    vec3 baseColor = mix(col1, col2, sin(colorTime) * 0.5 + 0.5);
    baseColor = mix(baseColor, col3, sin(colorTime * 1.3) * 0.5 + 0.5);
    baseColor = mix(baseColor, col4, sin(colorTime * 0.7) * 0.5 + 0.5);
    
    // Moduler la couleur par les patterns
    vec3 finalColor = baseColor * (0.5 + finalPattern * 0.5);
    
    // Ajouter des highlights
    finalColor += vec3(flow) * baseColor * 1.5;
    finalColor += vec3(hexPattern) * vec3(1.0, 0.8, 0.5) * 0.7;
    finalColor += vec3(turb) * baseColor * 0.8;
    
    // Effet de profondeur dans la texture
    float depthEffect = smoothstep(0.4, 0.6, voronoiPattern);
    finalColor *= 0.7 + depthEffect * 0.6;
    
    return finalColor;
}

// Rendu de la scène
vec3 renderScene(vec2 uv, float timeOffset) {
    vec2 p = (uv * 2.0 - 1.0) * vec2(iResolution.x / iResolution.y, 1.0);
    
    // Caméra
    float speed = 5.0;
    float time = iTime + timeOffset;
    vec3 ro = vec3(0.0, 0.0, time * speed);
    
    vec2 currentOffset = getTunnelOffset(ro.z);
    ro.xy += currentOffset;
    
    // Direction avec anticipation des virages
    vec2 lookAheadOffset = getTunnelOffset(ro.z + 3.0);
    vec2 lookOffset = lookAheadOffset - currentOffset;
    
    vec3 forward = normalize(vec3(lookOffset.x * 0.2, lookOffset.y * 0.2, 1.0));
    vec3 right = normalize(cross(vec3(0.0, 1.0, 0.0), forward));
    vec3 up = normalize(cross(forward, right));
    
    // Banking dans les virages
    float bankAngle = lookOffset.x * 0.25;
    float cb = cos(bankAngle);
    float sb = sin(bankAngle);
    vec3 rightBanked = right * cb - up * sb;
    vec3 upBanked = right * sb + up * cb;
    
    vec3 rd = normalize(forward + rightBanked * p.x * 0.7 + upBanked * p.y * 0.7);
    
    // Raymarching
    float t = trace(ro, rd);
    
    vec3 col = vec3(0.0);
    
    if (t < 60.0) {
        vec3 hitPos = ro + rd * t;
        vec3 normal = calcNormal(hitPos);
        
        // Couleur de la texture
        col = getTextureColor(hitPos, normal);
        
        // Éclairage
        vec3 lightDir = normalize(vec3(0.3, 0.2, 1.0));
        float diff = max(dot(normal, lightDir), 0.0);
        float spec = pow(max(dot(reflect(-lightDir, normal), -rd), 0.0), 32.0);
        
        col = col * (0.4 + diff * 0.6);
        col += vec3(1.0, 0.9, 0.8) * spec * 0.5;
        
        // Fresnel / rim light
        float fresnel = pow(1.0 - abs(dot(normal, -rd)), 3.0);
        col += fresnel * vec3(0.5, 0.7, 1.0) * 0.4;
        
        // Fog
        float fog = 1.0 - exp(-t * 0.04);
        col = mix(col, vec3(0.01, 0.02, 0.04), fog * 0.7);
        
    } else {
        // Fond spatial
        float stars = hash2(floor(p * 200.0));
        if (stars > 0.998) {
            col = vec3(1.0) * (stars - 0.998) * 500.0;
        }
        col += vec3(0.01, 0.02, 0.04);
    }
    
    return col;
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = fragCoord / iResolution.xy;
    
    // Rendu principal
    vec3 col = renderScene(uv, 0.0);
    
    // Motion blur léger
    vec3 blur = vec3(0.0);
    for (float i = -2.0; i <= 2.0; i += 1.0) {
        blur += renderScene(uv, i * 0.008);
    }
    blur /= 5.0;
    col = mix(col, blur, 0.25);
    
    // Bloom sur les zones lumineuses
    vec3 bloomCol = vec3(0.0);
    for (float x = -2.0; x <= 2.0; x++) {
        for (float y = -2.0; y <= 2.0; y++) {
            vec2 offset = vec2(x, y) * 0.004;
            vec3 sample = renderScene(uv + offset, 0.0);
            float brightness = dot(sample, vec3(0.299, 0.587, 0.114));
            if (brightness > 0.7) {
                bloomCol += sample;
            }
        }
    }
    bloomCol /= 25.0;
    col += bloomCol * 0.5;
    
    // Vignette
    vec2 vignetteUV = uv * 2.0 - 1.0;
    float vignette = 1.0 - dot(vignetteUV, vignetteUV) * 0.25;
    col *= vignette;
    
    // Aberration chromatique subtile
    float aberration = length(vignetteUV) * 0.015;
    
    // Gamma correction
    col = pow(col, vec3(0.4545));
    
    // Contraste
    col = (col - 0.5) * 1.15 + 0.5;
    
    // Saturation boost
    float luma = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(luma), col, 1.2);
    
    fragColor = vec4(col, 1.0);
}