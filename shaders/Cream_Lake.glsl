// Cream Lake
// Shadertoy ID: Nc3Gz8
// Description: https://github.com/Patrickjaillet   
// Tags: lake

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 res = iResolution.xy;
    vec2 uv = (fragCoord * 2.0 - res) / res.y;
    float t = iTime * 0.2;
// All my Soft for Windows
// https://github.com/Patrickjaillet   
    vec3 ro = vec3(0.0, 1.5, -2.5);
    vec3 ta = vec3(0.0, -0.2, 0.0);
    
    ro.xz *= mat2(cos(t * 0.5), -sin(t * 0.5), sin(t * 0.5), cos(t * 0.5));
    ro.y += sin(t) * 0.3;

    vec3 cw = normalize(ta - ro);
    vec3 cp = vec3(0.0, 1.0, 0.0);
    vec3 cu = normalize(cross(cw, cp));
    vec3 cv = cross(cu, cw);
    vec3 rd = normalize(uv.x * cu + uv.y * cv + 1.8 * cw);

    float tm = 0.0;
    float tmax = 12.0;
    float hit = -1.0;
    
    for (int i = 0; i < 252; i++) {
        vec3 p = ro + rd * tm;
        
        vec2 pTerrain = p.xz;
        vec2 state = vec2(0.0);
        float accum = 0.0;
        float freq = 2.2;
        const float iter = 6.9;
        for (float j = 0.0; j < iter; j++) {
            pTerrain *= mat2(cos(0.83), -sin(0.83), sin(0.83), cos(0.83));
            state = state * (mat2(cos(1.1), -sin(1.1), sin(1.1), cos(1.1)) + mat2(cos(t), -sin(t), sin(t), cos(t)) * 0.05) + sin(pTerrain * freq + t);
            vec2 q = pTerrain * freq + state;
            accum += (dot(sin(q), cos(q.yx)) / freq) * 3.5;
            state += cos(q + vec2(1.3, 0.7));
            freq *= 1.06;
        }
        float h = p.y - (accum * 0.04);

        if (abs(h) < 0.001 * tm) {
            hit = tm;
            break;
        }
        if (tm > tmax) break;
        tm += h * 0.5;
    }

    vec3 col = vec3(0.01, 0.005, 0.02) * (1.0 - vec3(uv.y * 0.5));
    
    if (hit > 0.0) {
        vec3 p = ro + rd * hit;
        
        vec2 pTerrainMain = p.xz;
        vec2 stateMain = vec2(0.0);
        float accumMain = 0.0;
        float freqMain = 2.2;
        for (float j = 0.0; j < 6.9; j++) {
            pTerrainMain *= mat2(cos(0.83), -sin(0.83), sin(0.83), cos(0.83));
            stateMain = stateMain * (mat2(cos(1.1), -sin(1.1), sin(1.1), cos(1.1)) + mat2(cos(t), -sin(t), sin(t), cos(t)) * 0.05) + sin(pTerrainMain * freqMain + t);
            vec2 q = pTerrainMain * freqMain + stateMain;
            accumMain += (dot(sin(q), cos(q.yx)) / freqMain) * 3.5;
            stateMain += cos(q + vec2(1.3, 0.7));
            freqMain *= 1.06;
        }
        float rawH = accumMain * 0.04;

        vec2 e = vec2(0.002, 0.0);
        
        vec2 pTerrainN1 = p.xz - e.xy;
        vec2 stateN1 = vec2(0.0);
        float accumN1 = 0.0;
        float freqN1 = 2.2;
        for (float j = 0.0; j < 6.9; j++) {
            pTerrainN1 *= mat2(cos(0.83), -sin(0.83), sin(0.83), cos(0.83));
            stateN1 = stateN1 * (mat2(cos(1.1), -sin(1.1), sin(1.1), cos(1.1)) + mat2(cos(t), -sin(t), sin(t), cos(t)) * 0.05) + sin(pTerrainN1 * freqN1 + t);
            vec2 q = pTerrainN1 * freqN1 + stateN1;
            accumN1 += (dot(sin(q), cos(q.yx)) / freqN1) * 3.5;
            stateN1 += cos(q + vec2(1.3, 0.7));
            freqN1 *= 1.06;
        }
        
        vec2 pTerrainN2 = p.xz + e.xy;
        vec2 stateN2 = vec2(0.0);
        float accumN2 = 0.0;
        float freqN2 = 2.2;
        for (float j = 0.0; j < 6.9; j++) {
            pTerrainN2 *= mat2(cos(0.83), -sin(0.83), sin(0.83), cos(0.83));
            stateN2 = stateN2 * (mat2(cos(1.1), -sin(1.1), sin(1.1), cos(1.1)) + mat2(cos(t), -sin(t), sin(t), cos(t)) * 0.05) + sin(pTerrainN2 * freqN2 + t);
            vec2 q = pTerrainN2 * freqN2 + stateN2;
            accumN2 += (dot(sin(q), cos(q.yx)) / freqN2) * 3.5;
            stateN2 += cos(q + vec2(1.3, 0.7));
            freqN2 *= 1.06;
        }
        
        vec2 pTerrainN3 = p.xz - e.yx;
        vec2 stateN3 = vec2(0.0);
        float accumN3 = 0.0;
        float freqN3 = 2.2;
        for (float j = 0.0; j < 6.9; j++) {
            pTerrainN3 *= mat2(cos(0.83), -sin(0.83), sin(0.83), cos(0.83));
            stateN3 = stateN3 * (mat2(cos(1.1), -sin(1.1), sin(1.1), cos(1.1)) + mat2(cos(t), -sin(t), sin(t), cos(t)) * 0.05) + sin(pTerrainN3 * freqN3 + t);
            vec2 q = pTerrainN3 * freqN3 + stateN3;
            accumN3 += (dot(sin(q), cos(q.yx)) / freqN3) * 3.5;
            stateN3 += cos(q + vec2(1.3, 0.7));
            freqN3 *= 1.06;
        }
        
        vec2 pTerrainN4 = p.xz + e.yx;
        vec2 stateN4 = vec2(0.0);
        float accumN4 = 0.0;
        float freqN4 = 2.2;
        for (float j = 0.0; j < 6.9; j++) {
            pTerrainN4 *= mat2(cos(0.83), -sin(0.83), sin(0.83), cos(0.83));
            stateN4 = stateN4 * (mat2(cos(1.1), -sin(1.1), sin(1.1), cos(1.1)) + mat2(cos(t), -sin(t), sin(t), cos(t)) * 0.05) + sin(pTerrainN4 * freqN4 + t);
            vec2 q = pTerrainN4 * freqN4 + stateN4;
            accumN4 += (dot(sin(q), cos(q.yx)) / freqN4) * 3.5;
            stateN4 += cos(q + vec2(1.3, 0.7));
            freqN4 *= 1.06;
        }
        
        vec3 n = normalize(vec3(
            (accumN1 * 0.04) - (accumN2 * 0.04),
            2.0 * e.x,
            (accumN3 * 0.04) - (accumN4 * 0.04)
        ));
        
        vec3 r = reflect(rd, n);
        
        vec3 lPos = vec3(2.0, 5.0, -3.0);
        lPos.xz *= mat2(cos(t), -sin(t), sin(t), cos(t));
        vec3 lDir = normalize(lPos - p);
        
        float diff = clamp(dot(n, lDir), 0.0, 1.0);
        float spec = pow(clamp(dot(r, lDir), 0.0, 1.0), 32.0);
        float fre = pow(clamp(1.0 + dot(n, rd), 0.0, 1.0), 4.0);
        
        float sha = 1.0;
        float mint = 0.02;
        float maxt = 4.0;
        float ph = 1e20;
        vec3 roSha = p + n * 0.01;
        for (float i = mint; i < maxt; ) {
            vec3 pSha = roSha + lDir * i;
            vec2 pTerrainSha = pSha.xz;
            vec2 stateSha = vec2(0.0);
            float accumSha = 0.0;
            float freqSha = 2.2;
            for (float j = 0.0; j < 6.9; j++) {
                pTerrainSha *= mat2(cos(0.83), -sin(0.83), sin(0.83), cos(0.83));
                stateSha = stateSha * (mat2(cos(1.1), -sin(1.1), sin(1.1), cos(1.1)) + mat2(cos(t), -sin(t), sin(t), cos(t)) * 0.05) + sin(pTerrainSha * freqSha + t);
                vec2 q = pTerrainSha * freqSha + stateSha;
                accumSha += (dot(sin(q), cos(q.yx)) / freqSha) * 3.5;
                stateSha += cos(q + vec2(1.3, 0.7));
                freqSha *= 1.06;
            }
            float hSha = pSha.y - (accumSha * 0.04);
            
            if (hSha < 0.001) {
                sha = 0.0;
                break;
            }
            float y = hSha * hSha / (2.0 * ph);
            float d = sqrt(hSha * hSha - y * y);
            sha = min(sha, 16.0 * d / max(0.0, i - y));
            ph = hSha;
            i += max(0.01, hSha);
        }
        sha = clamp(sha, 0.0, 1.0);
        
        float occ = 0.0;
        float sca = 1.0;
        for (float i = 0.0; i < 1.0; i++) {
            float hr = 0.01 + 0.12 * i / 4.0;
            vec3 aopos = n * hr + p;
            
            vec2 pTerrainAO = aopos.xz;
            vec2 stateAO = vec2(0.0);
            float accumAO = 0.0;
            float freqAO = 2.2;
            for (float j = 0.0; j < 6.9; j++) {
                pTerrainAO *= mat2(cos(0.83), -sin(0.83), sin(0.83), cos(0.83));
                stateAO = stateAO * (mat2(cos(1.1), -sin(1.1), sin(1.1), cos(1.1)) + mat2(cos(t), -sin(t), sin(t), cos(t)) * 0.05) + sin(pTerrainAO * freqAO + t);
                vec2 q = pTerrainAO * freqAO + stateAO;
                accumAO += (dot(sin(q), cos(q.yx)) / freqAO) * 3.5;
                stateAO += cos(q + vec2(1.3, 0.7));
                freqAO *= 1.06;
            }
            float dd = aopos.y - (accumAO * 0.04);
            
            occ += -(dd - hr) * sca;
            sca *= 0.95;
        }
        float ao = clamp(1.0 - 3.0 * occ, 0.0, 1.0);

        vec3 matCol = vec3(0.0);
        matCol.r = sin(rawH * 10.0 + 0.0) * 0.5 + 0.5;
        matCol.g = sin(rawH * 7.5 + 1.5) * 0.5 + 0.5;
        matCol.b = sin(rawH * 5.0 + 3.0) * 0.5 + 0.5;

        vec3 amb = vec3(0.08, 0.04, 0.1) * ao;
        vec3 dir = vec3(1.2, 0.9, 0.7) * diff * sha;
        
        col = matCol * (dir + amb);
        col += vec3(0.5, 0.3, 0.6) * spec * sha;
        col += vec3(0.2, 0.4, 0.7) * fre * ao;
        
        col = mix(col, vec3(0.02, 0.01, 0.03), 1.0 - exp(-0.04 * hit * hit));
    }

    col += vec3(0.2, 0.03, 0.25) * (1.0 / (1.0 + dot(uv, uv) * 2.0));
    col = pow(col, vec3(0.8));
    col = clamp(col, 0.0, 1.0);
    
    fragColor = vec4(col, 1.0);
}