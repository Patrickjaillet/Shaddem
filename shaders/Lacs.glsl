// Lacs
// Shadertoy ID: N3jSzt
// Description: lacs
// Tags: lacs

mat2 rot(float a) {
    float c = cos(a), s = sin(a);
    return mat2(c, -s, s, c);
}

float hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453123);
}

float noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash(i + vec2(0.0, 0.0)), hash(i + vec2(1.0, 0.0)), u.x),
               mix(hash(i + vec2(0.0, 1.0)), hash(i + vec2(1.0, 1.0)), u.x), u.y);
}

float terrain(vec3 p, int octaves) {
    vec2 st = p.xz * 0.035;
    float h = 0.0;
    float weight = 7.5;
    float m = 0.0;
    mat2 r = mat2(1.6, 1.2, -1.2, 1.6);
    for(int j = 0; j < 15; j++) {
        if(j >= octaves) break;
        st *= r;
        float n = sin(st.x) * cos(st.y);
        h += n * weight;
        m += weight;
        weight *= 0.46 / (1.0 + h * h * 0.012);
    }
    return h - (p.y - 0.2) * 0.02;
}

float sea(vec3 p) {
    vec2 uv = p.xz * 0.4;
    float t = iTime * 1.5;
    mat2 r = mat2(1.6, 1.2, -1.2, 1.6);
    float h = 0.0;
    float w = 0.15;
    for(int i = 0; i < 5; i++) {
        vec2 d = vec2(sin(uv.x + t), cos(uv.y + t));
        uv += d * 0.1;
        h += (1.0 - abs(sin(uv.x + t) * cos(uv.y + t))) * w;
        uv *= r;
        w *= 0.45;
        t *= 1.3;
    }
    return h * 0.35 + 0.2;
}

vec3 skyColor(vec3 rd, vec3 lig) {
    float d = max(dot(rd, lig), 0.0);
    vec3 sky = vec3(0.3, 0.5, 0.85) - rd.y * 0.4 * vec3(1.0, 0.7, 0.4);
    sky = mix(sky, vec3(0.7, 0.82, 0.95), pow(1.0 - max(rd.y, 0.0), 5.0));
    sky += vec3(1.0, 0.85, 0.6) * pow(d, 350.0) * 3.0;
    sky += vec3(1.0, 0.7, 0.4) * pow(d, 12.0) * 0.6;
    return max(sky, 0.0);
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    vec3 ro = vec3(iTime * 4.5, 18.0 + sin(iTime * 0.2) * 5.0, iTime * 3.0);
    vec3 rd = normalize(vec3(uv.x, uv.y - 0.15, 1.1));
    rd.xz *= rot(sin(iTime * 0.05) * 0.15);
    rd.xy *= rot(cos(iTime * 0.03) * 0.05);
    vec3 lig = normalize(vec3(0.8, 0.4, 0.5));
    
    float t = 0.1;
    float maxD = 180.0;
    bool hit = false;
    vec3 p;
    float h = 0.0;
    float wh = 0.0;
    bool isWater = false;
    
    for(int i = 0; i < 320; i++) {
        p = ro + rd * t;
        h = terrain(p, 15);
        wh = sea(p);
        float d = p.y - max(h, wh);
        if(abs(d) < 0.0004 * t) {
            hit = true;
            isWater = (wh > h);
            break;
        }
        if(t > maxD) break;
        t += d * 0.45;
    }
    
    vec3 col = skyColor(rd, lig);
    float cloud = noise(rd.xz / (rd.y + 0.01) * 0.1 + vec2(iTime * 0.02));
    col = mix(col, vec3(0.95, 0.9, 0.85), smoothstep(0.4, 0.8, cloud) * max(rd.y, 0.0) * 0.5);
    
    if(hit) {
        vec3 nor;
        if(isWater) {
            vec3 eps = vec3(0.005, 0.0, 0.0);
            float h0 = sea(p);
            float hx = sea(p + eps.xyz);
            float hz = sea(p + eps.zyx);
            nor = normalize(vec3(h0 - hx, eps.x, h0 - hz));
        } else {
            vec3 eps = vec3(0.001 * t, 0.0, 0.0);
            float h0 = terrain(p, 15);
            float hx = terrain(p + eps.xyz, 15);
            float hz = terrain(p + eps.zyx, 15);
            nor = normalize(vec3(h0 - hx, eps.x, h0 - hz));
        }
        
        float sh = 1.0;
        float shT = 0.1;
        for(int i = 0; i < 45; i++) {
            vec3 sp = p + lig * shT;
            float sph = terrain(sp, 10);
            float swh = sea(sp);
            float sd = sp.y - max(sph, swh);
            sh = min(sh, 12.0 * max(sd, 0.0) / shT);
            shT += clamp(sd, 0.2, 5.0);
            if(sh < 0.001 || shT > 60.0) break;
        }
        sh = clamp(sh, 0.0, 1.0);
        
        float ao = 1.0;
        float aoStep = 0.5;
        float aoSum = 0.0;
        for(int i = 1; i <= 4; i++) {
            float dist = float(i) * aoStep;
            aoSum += (dist - (p.y - terrain(p + nor * dist, 8))) / dist;
        }
        ao = clamp(1.0 - aoSum * 0.2, 0.0, 1.0);
        
        float dif = clamp(dot(nor, lig), 0.0, 1.0);
        float amb = clamp(0.5 + 0.5 * nor.y, 0.0, 1.0);
        float bac = clamp(dot(nor, normalize(vec3(-lig.x, 0.0, -lig.z))), 0.0, 1.0) * clamp(1.0 - p.y / 40.0, 0.0, 1.0);
        
        vec3 mCol;
        float slope = nor.y;
        
        if(isWater) {
            float fre = pow(clamp(1.0 + dot(nor, rd), 0.0, 1.0), 5.0);
            vec3 waterBase = vec3(0.02, 0.08, 0.12);
            vec3 skyRefl = skyColor(reflect(rd, nor), lig);
            mCol = mix(waterBase, skyRefl, fre * 0.9 + 0.06);
            
            float spec = pow(max(dot(reflect(rd, nor), lig), 0.0), 160.0);
            mCol += vec3(1.0, 0.9, 0.7) * spec * 2.0 * sh;
            
            float depth = clamp((wh - h) * 0.3, 0.0, 1.0);
            mCol = mix(mCol, vec3(0.01, 0.04, 0.05), depth);
        } else {
            vec3 rock = vec3(0.15, 0.14, 0.13) * (0.4 + 0.6 * noise(p.xz * 0.2));
            vec3 grass = vec3(0.08, 0.11, 0.06) * (0.5 + 0.5 * noise(p.xz * 0.8));
            vec3 snow = vec3(0.95, 0.96, 0.98);
            
            mCol = mix(rock, grass, smoothstep(0.7, 0.9, slope) * (1.0 - smoothstep(12.0, 28.0, p.y + noise(p.xz) * 3.0)));
            
            float snowThresh = 15.0 + noise(p.xz * 0.05) * 8.0 - slope * 6.0;
            mCol = mix(mCol, snow, smoothstep(snowThresh, snowThresh + 4.0, p.y) * smoothstep(0.5, 0.7, slope));
            
            vec3 microN = vec3(noise(p.xz * 15.0), noise(p.zx * 18.0), noise(p.xz * 12.0 + 5.0)) * 2.0 - 1.0;
            nor = normalize(nor + microN * 0.04 * (1.0 - smoothstep(15.0, 35.0, t)));
            dif = clamp(dot(nor, lig), 0.0, 1.0);
        }
        
        if(!isWater) {
            vec3 lin = vec3(0.0);
            lin += dif * vec3(1.6, 1.3, 1.0) * sh;
            lin += amb * vec3(0.4, 0.55, 0.7) * ao;
            lin += bac * vec3(0.25, 0.2, 0.15) * ao;
            col = mCol * lin;
            
            float spec = pow(max(dot(reflect(rd, nor), lig), 0.0), 12.0);
            col += vec3(1.0, 0.9, 0.8) * spec * 0.08 * sh * (1.0 - slope * 0.3);
        }
        
        float fog = 1.0 - exp(-pow(t * 0.0085, 1.5));
        vec3 fogCol = skyColor(rd, lig);
        col = mix(col, fogCol, fog);
    }
    
    col = mix(col, vec3(dot(col, vec3(0.2126, 0.7152, 0.0722))), -0.08);
    col = col * 1.15 / (col + vec3(1.0));
    col = pow(col, vec3(0.4545));
    
    vec2 screenUV = fragCoord / iResolution.xy;
    col *= 0.5 + 0.5 * pow(16.0 * screenUV.x * screenUV.y * (1.0 - screenUV.x) * (1.0 - screenUV.y), 0.25);
    
    fragColor = vec4(col, 1.0);
}