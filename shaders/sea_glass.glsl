// Shader: Mer Liquide & Objet Verre Morphing

// Rotation 2D
float2x2 Rot(float a) {
    float s = sin(a);
    float c = cos(a);
    return float2x2(c, -s, s, c);
}

// SDF Primitives
float sdBox(float3 p, float3 b) {
    float3 q = abs(p) - b;
    return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
}

float sdSphere(float3 p, float s) {
    return length(p) - s;
}

float sdOctahedron(float3 p, float s) {
    p = abs(p);
    return (p.x + p.y + p.z - s) * 0.57735027;
}

// Bruit pour la mer et les nuages
float hash(float2 p) {
    p = 50.0 * frac(p * 0.3183099 + float2(0.71, 0.113));
    return -1.0 + 2.0 * frac(p.x * p.y * (p.x + p.y));
}

float noise(float2 p) {
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);
    return lerp(lerp(hash(i + float2(0.0, 0.0)), hash(i + float2(1.0, 0.0)), u.x),
                lerp(hash(i + float2(0.0, 1.0)), hash(i + float2(1.0, 1.0)), u.x), u.y);
}

// Map de la mer
float mapSea(float3 p) {
    float speed = iTime * customSpeed;
    float h = 0.0;
    float2 uv = p.xz * 0.2;
    
    // Superposition d'ondes
    h += noise(uv + speed * 0.5) * 0.5;
    h += noise(uv * 2.0 - speed * 0.3) * 0.25;
    h += noise(uv * 4.0 + speed * 0.1) * 0.125;
    
    // customWave contrôle l'amplitude des vagues
    return p.y + 2.0 - h * (customWave + 0.5); 
}

// Map de l'objet central
float mapObject(float3 p) {
    float3 p_obj = p;
    
    // Rotation de l'objet
    float rotSpeed = iTime * customRotationSpeed * 0.1;
    p_obj.xz = mul(p_obj.xz, Rot(iTime * 0.5 + rotSpeed));
    p_obj.xy = mul(p_obj.xy, Rot(iTime * 0.3));
    
    // Morphing basé sur le temps
    float t = iTime * customSpeed * 0.5;
    float state = (sin(t) * 0.5 + 0.5) * 2.0; // 0 -> 2
    
    float dBox = sdBox(p_obj, float3(1.0, 1.0, 1.0));
    float dSphere = sdSphere(p_obj, 1.3);
    float dOct = sdOctahedron(p_obj, 1.5);
    
    // Interpolation entre les formes
    float d = lerp(dBox, dSphere, smoothstep(0.0, 1.0, state));
    d = lerp(d, dOct, smoothstep(1.0, 2.0, state));
    
    return d;
}

// Map Globale
float GetDist(float3 p) {
    float dSea = mapSea(p);
    float dObj = mapObject(p);
    return min(dSea, dObj);
}

// Raymarching
float RayMarch(float3 ro, float3 rd) {
    float dO = 0.0;
    for(int i=0; i<100; i++) {
        float3 p = ro + rd * dO;
        float dS = GetDist(p);
        dO += dS;
        if(dO > 100.0 || dS < 0.001) break;
    }
    return dO;
}

// Calcul des normales
float3 GetNormal(float3 p) {
    float d = GetDist(p);
    float2 e = float2(0.001, 0.0);
    float3 n = d - float3(
        GetDist(p - e.xyy),
        GetDist(p - e.yxy),
        GetDist(p - e.yyx)
    );
    return normalize(n);
}

// Ciel et Nuages
float3 GetSky(float3 rd) {
    // Dégradé de ciel
    float3 col = lerp(float3(0.5, 0.7, 0.9), float3(0.1, 0.3, 0.6), rd.y * 0.5 + 0.5);
    
    // Nuages (bruit projeté sur le plan du ciel)
    if (rd.y > 0.0) {
        float2 uv = rd.xz / (rd.y + 0.2);
        float cl = noise(uv * 3.0 + iTime * 0.05);
        cl += noise(uv * 6.0 - iTime * 0.05) * 0.5;
        col = lerp(col, float3(1.0, 1.0, 1.0), smoothstep(0.4, 0.8, cl) * 0.6);
    }
    return col;
}

void mainImage(out float4 fragColor, in float2 fragCoord) {
    float2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    
    // Caméra
    float3 ro = float3(0.0, 0.0, -4.0);
    float3 rd = normalize(float3(uv.x, uv.y, 1.0));
    
    // Raymarching
    float d = RayMarch(ro, rd);
    
    float3 col = float3(0.0, 0.0, 0.0);
    
    if(d < 100.0) {
        float3 p = ro + rd * d;
        float3 n = GetNormal(p);
        float3 r = reflect(rd, n);
        
        float dObj = mapObject(p);
        float dSea = mapSea(p);
        
        // Matériaux
        if (dObj < dSea) {
            // OBJET : Verre / Miroir
            
            // Reflet de l'environnement (Ciel)
            float3 ref = GetSky(r);
            
            // Fresnel (plus brillant sur les bords)
            float fresnel = pow(1.0 + dot(rd, n), 3.0);
            
            // Couleur de base (teintée par customColor)
            float3 baseCol = customColor.rgb * 0.1;
            
            // Mélange base + reflet
            col = lerp(baseCol, ref, fresnel + 0.4);
            
            // Spéculaire (reflet du soleil fictif)
            float3 sunDir = normalize(float3(0.5, 0.8, -0.5));
            float spec = pow(max(dot(r, sunDir), 0.0), 32.0);
            col += spec * 0.8;
            
        } else {
            // MER : Liquide
            
            float3 seaCol = float3(0.0, 0.1, 0.2) + customColor.rgb * 0.2;
            
            // Eclairage diffus
            float3 sunDir = normalize(float3(0.5, 0.8, -0.5));
            float dif = max(dot(n, sunDir), 0.0);
            col = seaCol * (dif + 0.2);
            
            // Reflet du ciel sur l'eau
            float fresnel = pow(1.0 + dot(rd, n), 5.0);
            col += GetSky(r) * fresnel * 0.8;
            
            // Spéculaire sur les vagues
            float spec = pow(max(dot(r, sunDir), 0.0), 64.0);
            col += spec * 0.5;
        }
        
        // Brouillard atmosphérique
        col = lerp(col, GetSky(rd), 1.0 - exp(-d * 0.02));
        
    } else {
        // Fond (Ciel)
        col = GetSky(rd);
    }
    
    // Gamma correction
    col = pow(col, float3(0.4545, 0.4545, 0.4545));
    
    fragColor = float4(col, 1.0);
}