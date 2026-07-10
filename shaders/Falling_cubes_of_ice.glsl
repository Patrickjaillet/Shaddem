// Falling cubes of ice
// Shadertoy ID: f3BXWt
// Description: Falling cubes of ice
// Tags: ice

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    vec3 ro = vec3(0.0, 3.5, -6.0);
    vec3 target = vec3(0.0, 0.5, 0.0);
    vec3 ww = normalize(target - ro);
    vec3 uu = normalize(cross(ww, vec3(0.0, 1.0, 0.0)));
    vec3 vv = normalize(cross(uu, ww));
    vec3 rd = normalize(uv.x * uu + uv.y * vv + 1.5 * ww);
    vec4 scene = vec4(0.0);
    float t = 0.0;
    int RAY_STEPS = int(max(32.0, floor(128.0 * customQualityScale)));

    for(int i = 0; i < RAY_STEPS; i++) {
        vec3 p = ro + rd * t;
        float dPlane = p.y;
        float dIce = 15.0;
        float mat = 0.0;
        
        for(float j = 0.0; j < 4.0; j++) {
            float timeSec = iTime * 1.2 + j * 2.1;
            float cycle = floor(timeSec / 4.0);
            float relTime = mod(timeSec, 4.0);
            float id = j + cycle * 7.33;
            
            vec3 h3 = fract(sin(vec3(id, id + 1.0, id + 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423));
            vec2 launchPos = (h3.xy - 0.5) * 5.0;
            vec3 iceP = p;
            
            if(relTime < 1.5) {
                float fallT = relTime / 1.5;
                float y = mix(6.0, 0.25, fallT * fallT);
                iceP -= vec3(launchPos.x, y, launchPos.y);
                
                float a1 = relTime * 4.0 + id;
                float s1 = sin(a1), c1 = cos(a1);
                iceP.xz = mat2(c1, -s1, s1, c1) * iceP.xz;
                
                float a2 = relTime * 2.0 - id;
                float s2 = sin(a2), c2 = cos(a2);
                iceP.xy = mat2(c2, -s2, s2, c2) * iceP.xy;
                
                vec3 q = abs(iceP) - vec3(0.25);
                float box = length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
                
                vec3 vp = iceP * 12.0;
                vec3 vi = floor(vp);
                vec3 vf = fract(vp);
                float minDist = 1.0;
                for(int vz = -1; vz <= 1; vz++) {
                    for(int vy = -1; vy <= 1; vy++) {
                        for(int vx = -1; vx <= 1; vx++) {
                            vec3 vg = vec3(float(vx), float(vy), float(vz));
                            vec3 vo = fract(sin(vec3(dot(vi + vg, vec3(127.1, 311.7, 74.7))) + vec3(0.0, 1.0, 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423));
                            vec3 vr = vg + vo - vf;
                            float vd = dot(vr, vr);
                            if(vd < minDist) minDist = vd;
                        }
                    }
                }
                box -= sqrt(minDist) * 0.03;
                
                if(box < dIce) {
                    dIce = box;
                    mat = 1.0;
                }
            } else {
                float crackT = relTime - 1.5;
                vec3 impactP = vec3(launchPos.x, 0.25, launchPos.y);
                vec3 localP = p - impactP;
                float explode = crackT * 4.0;
                float force = smoothstep(0.0, 0.5, crackT);
                vec3 cellI = floor(localP * 5.0);
                float cellHash = fract(sin(dot(cellI, vec3(12.9898, 78.233, 45.164))) * 43758.5453123);
                
                vec3 dir = fract(sin(vec3(cellHash, cellHash + 1.0, cellHash + 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423)) - 0.5;
                dir.y = abs(dir.y) * 1.5;
                dir = normalize(dir);
                
                localP -= dir * force * (0.1 + cellHash * 0.6);
                localP.y -= 0.25;
                
                float a3 = cellHash * crackT * 5.0;
                float s3 = sin(a3), c3 = cos(a3);
                localP.xz = mat2(c3, -s3, s3, c3) * localP.xz;
                
                float a4 = dir.z * crackT * 3.0;
                float s4 = sin(a4), c4 = cos(a4);
                localP.xy = mat2(c4, -s4, s4, c4) * localP.xy;
                
                localP.y += mix(0.25, -0.2, smoothstep(0.0, 1.0, crackT));
                
                vec3 q1 = abs(localP) - vec3(0.25);
                float box = length(max(q1, 0.0)) + min(max(q1.x, max(q1.y, q1.z)), 0.0);
                
                vec3 vp = localP * 12.0;
                vec3 vi = floor(vp);
                vec3 vf = fract(vp);
                float minDist = 1.0;
                for(int vz = -1; vz <= 1; vz++) {
                    for(int vy = -1; vy <= 1; vy++) {
                        for(int vx = -1; vx <= 1; vx++) {
                            vec3 vg = vec3(float(vx), float(vy), float(vz));
                            vec3 vo = fract(sin(vec3(dot(vi + vg, vec3(127.1, 311.7, 74.7))) + vec3(0.0, 1.0, 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423));
                            vec3 vr = vg + vo - vf;
                            float vd = dot(vr, vr);
                            if(vd < minDist) minDist = vd;
                        }
                    }
                }
                box -= sqrt(minDist) * 0.03;
                
                vec3 q2 = abs(p - impactP - vec3(0.0, explode * 0.2, 0.0)) - vec3(0.3 + explode * 1.5);
                float bounds = length(max(q2, 0.0)) + min(max(q2.x, max(q2.y, q2.z)), 0.0);
                box = max(box, bounds);
                
                if(box < dIce && crackT < 1.5) {
                    dIce = box;
                    mat = 1.0;
                }
            }
        }
        
        float d = min(dPlane, dIce);
        if (d == dIce) mat = 1.0;
        scene = vec4(d, mat, 0.0, 0.0);
        
        if(scene.x < 0.001 || t > 15.0) break;
        t += scene.x;
    }
    
    vec3 col = vec3(0.12, 0.15, 0.2) - rd.y * 0.1;
    
    if(t < 15.0) {
        vec3 p = ro + rd * t;
        vec3 n;
        {
            vec2 e = vec2(0.001, 0.0);
            vec4 s0 = vec4(0.0), s1 = vec4(0.0), s2 = vec4(0.0), s3 = vec4(0.0);
            vec3 p0 = p, p1 = p - e.xyy, p2 = p - e.yxy, p3 = p - e.yyx;
            
            float dPlane = p0.y; float dIce = 15.0; float mat = 0.0;
            for(float j = 0.0; j < 4.0; j++) {
                float timeSec = iTime * 1.2 + j * 2.1; float cycle = floor(timeSec / 4.0); float relTime = mod(timeSec, 4.0); float id = j + cycle * 7.33;
                vec3 h3 = fract(sin(vec3(id, id + 1.0, id + 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423)); vec2 launchPos = (h3.xy - 0.5) * 5.0; vec3 iceP = p0;
                if(relTime < 1.5) {
                    float fallT = relTime / 1.5; float y = mix(6.0, 0.25, fallT * fallT); iceP -= vec3(launchPos.x, y, launchPos.y);
                    float a1 = relTime * 4.0 + id; float s1 = sin(a1), c1 = cos(a1); iceP.xz = mat2(c1, -s1, s1, c1) * iceP.xz;
                    float a2 = relTime * 2.0 - id; float s2 = sin(a2), c2 = cos(a2); iceP.xy = mat2(c2, -s2, s2, c2) * iceP.xy;
                    vec3 q = abs(iceP) - vec3(0.25); float box = length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
                    vec3 vp = iceP * 12.0; vec3 vi = floor(vp); vec3 vf = fract(vp); float minDist = 1.0;
                    for(int vz = -1; vz <= 1; vz++) { for(int vy = -1; vy <= 1; vy++) { for(int vx = -1; vx <= 1; vx++) {
                        vec3 vg = vec3(float(vx), float(vy), float(vz)); vec3 vo = fract(sin(vec3(dot(vi + vg, vec3(127.1, 311.7, 74.7))) + vec3(0.0, 1.0, 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423));
                        vec3 vr = vg + vo - vf; float vd = dot(vr, vr); if(vd < minDist) minDist = vd;
                    } } }
                    box -= sqrt(minDist) * 0.03; if(box < dIce) { dIce = box; mat = 1.0; }
                } else {
                    float crackT = relTime - 1.5; vec3 impactP = vec3(launchPos.x, 0.25, launchPos.y); vec3 localP = p0 - impactP; float explode = crackT * 4.0; float force = smoothstep(0.0, 0.5, crackT); vec3 cellI = floor(localP * 5.0); float cellHash = fract(sin(dot(cellI, vec3(12.9898, 78.233, 45.164))) * 43758.5453123);
                    vec3 dir = fract(sin(vec3(cellHash, cellHash + 1.0, cellHash + 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423)) - 0.5; dir.y = abs(dir.y) * 1.5; dir = normalize(dir);
                    localP -= dir * force * (0.1 + cellHash * 0.6); localP.y -= 0.25; float a3 = cellHash * crackT * 5.0; float s3 = sin(a3), c3 = cos(a3); localP.xz = mat2(c3, -s3, s3, c3) * localP.xz; float a4 = dir.z * crackT * 3.0; float s4 = sin(a4), c4 = cos(a4); localP.xy = mat2(c4, -s4, s4, c4) * localP.xy; localP.y += mix(0.25, -0.2, smoothstep(0.0, 1.0, crackT));
                    vec3 q1 = abs(localP) - vec3(0.25); float box = length(max(q1, 0.0)) + min(max(q1.x, max(q1.y, q1.z)), 0.0);
                    vec3 vp = localP * 12.0; vec3 vi = floor(vp); vec3 vf = fract(vp); float minDist = 1.0;
                    for(int vz = -1; vz <= 1; vz++) { for(int vy = -1; vy <= 1; vy++) { for(int vx = -1; vx <= 1; vx++) {
                        vec3 vg = vec3(float(vx), float(vy), float(vz)); vec3 vo = fract(sin(vec3(dot(vi + vg, vec3(127.1, 311.7, 74.7))) + vec3(0.0, 1.0, 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423));
                        vec3 vr = vg + vo - vf; float vd = dot(vr, vr); if(vd < minDist) minDist = vd;
                    } } }
                    box -= sqrt(minDist) * 0.03; vec3 q2 = abs(p0 - impactP - vec3(0.0, explode * 0.2, 0.0)) - vec3(0.3 + explode * 1.5); float bounds = length(max(q2, 0.0)) + min(max(q2.x, max(q2.y, q2.z)), 0.0); box = max(box, bounds);
                    if(box < dIce && crackT < 1.5) { dIce = box; mat = 1.0; }
                }
            }
            s0.x = min(dPlane, dIce);
            
            dPlane = p1.y; dIce = 15.0;
            for(float j = 0.0; j < 4.0; j++) {
                float timeSec = iTime * 1.2 + j * 2.1; float cycle = floor(timeSec / 4.0); float relTime = mod(timeSec, 4.0); float id = j + cycle * 7.33;
                vec3 h3 = fract(sin(vec3(id, id + 1.0, id + 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423)); vec2 launchPos = (h3.xy - 0.5) * 5.0; vec3 iceP = p1;
                if(relTime < 1.5) {
                    float fallT = relTime / 1.5; float y = mix(6.0, 0.25, fallT * fallT); iceP -= vec3(launchPos.x, y, launchPos.y);
                    float a1 = relTime * 4.0 + id; float s1 = sin(a1), c1 = cos(a1); iceP.xz = mat2(c1, -s1, s1, c1) * iceP.xz;
                    float a2 = relTime * 2.0 - id; float s2 = sin(a2), c2 = cos(a2); iceP.xy = mat2(c2, -s2, s2, c2) * iceP.xy;
                    vec3 q = abs(iceP) - vec3(0.25); float box = length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
                    vec3 vp = iceP * 12.0; vec3 vi = floor(vp); vec3 vf = fract(vp); float minDist = 1.0;
                    for(int vz = -1; vz <= 1; vz++) { for(int vy = -1; vy <= 1; vy++) { for(int vx = -1; vx <= 1; vx++) {
                        vec3 vg = vec3(float(vx), float(vy), float(vz)); vec3 vo = fract(sin(vec3(dot(vi + vg, vec3(127.1, 311.7, 74.7))) + vec3(0.0, 1.0, 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423));
                        vec3 vr = vg + vo - vf; float vd = dot(vr, vr); if(vd < minDist) minDist = vd;
                    } } }
                    box -= sqrt(minDist) * 0.03; if(box < dIce) dIce = box;
                } else {
                    float crackT = relTime - 1.5; vec3 impactP = vec3(launchPos.x, 0.25, launchPos.y); vec3 localP = p1 - impactP; float explode = crackT * 4.0; float force = smoothstep(0.0, 0.5, crackT); vec3 cellI = floor(localP * 5.0); float cellHash = fract(sin(dot(cellI, vec3(12.9898, 78.233, 45.164))) * 43758.5453123);
                    vec3 dir = fract(sin(vec3(cellHash, cellHash + 1.0, cellHash + 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423)) - 0.5; dir.y = abs(dir.y) * 1.5; dir = normalize(dir);
                    localP -= dir * force * (0.1 + cellHash * 0.6); localP.y -= 0.25; float a3 = cellHash * crackT * 5.0; float s3 = sin(a3), c3 = cos(a3); localP.xz = mat2(c3, -s3, s3, c3) * localP.xz; float a4 = dir.z * crackT * 3.0; float s4 = sin(a4), c4 = cos(a4); localP.xy = mat2(c4, -s4, s4, c4) * localP.xy; localP.y += mix(0.25, -0.2, smoothstep(0.0, 1.0, crackT));
                    vec3 q1 = abs(localP) - vec3(0.25); float box = length(max(q1, 0.0)) + min(max(q1.x, max(q1.y, q1.z)), 0.0);
                    vec3 vp = localP * 12.0; vec3 vi = floor(vp); vec3 vf = fract(vp); float minDist = 1.0;
                    for(int vz = -1; vz <= 1; vz++) { for(int vy = -1; vy <= 1; vy++) { for(int vx = -1; vx <= 1; vx++) {
                        vec3 vg = vec3(float(vx), float(vy), float(vz)); vec3 vo = fract(sin(vec3(dot(vi + vg, vec3(127.1, 311.7, 74.7))) + vec3(0.0, 1.0, 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423));
                        vec3 vr = vg + vo - vf; float vd = dot(vr, vr); if(vd < minDist) minDist = vd;
                    } } }
                    box -= sqrt(minDist) * 0.03; vec3 q2 = abs(p1 - impactP - vec3(0.0, explode * 0.2, 0.0)) - vec3(0.3 + explode * 1.5); float bounds = length(max(q2, 0.0)) + min(max(q2.x, max(q2.y, q2.z)), 0.0); box = max(box, bounds);
                    if(box < dIce && crackT < 1.5) dIce = box;
                }
            }
            s1.x = min(dPlane, dIce);
            
            dPlane = p2.y; dIce = 15.0;
            for(float j = 0.0; j < 4.0; j++) {
                float timeSec = iTime * 1.2 + j * 2.1; float cycle = floor(timeSec / 4.0); float relTime = mod(timeSec, 4.0); float id = j + cycle * 7.33;
                vec3 h3 = fract(sin(vec3(id, id + 1.0, id + 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423)); vec2 launchPos = (h3.xy - 0.5) * 5.0; vec3 iceP = p2;
                if(relTime < 1.5) {
                    float fallT = relTime / 1.5; float y = mix(6.0, 0.25, fallT * fallT); iceP -= vec3(launchPos.x, y, launchPos.y);
                    float a1 = relTime * 4.0 + id; float s1 = sin(a1), c1 = cos(a1); iceP.xz = mat2(c1, -s1, s1, c1) * iceP.xz;
                    float a2 = relTime * 2.0 - id; float s2 = sin(a2), c2 = cos(a2); iceP.xy = mat2(c2, -s2, s2, c2) * iceP.xy;
                    vec3 q = abs(iceP) - vec3(0.25); float box = length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
                    vec3 vp = iceP * 12.0; vec3 vi = floor(vp); vec3 vf = fract(vp); float minDist = 1.0;
                    for(int vz = -1; vz <= 1; vz++) { for(int vy = -1; vy <= 1; vy++) { for(int vx = -1; vx <= 1; vx++) {
                        vec3 vg = vec3(float(vx), float(vy), float(vz)); vec3 vo = fract(sin(vec3(dot(vi + vg, vec3(127.1, 311.7, 74.7))) + vec3(0.0, 1.0, 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423));
                        vec3 vr = vg + vo - vf; float vd = dot(vr, vr); if(vd < minDist) minDist = vd;
                    } } }
                    box -= sqrt(minDist) * 0.03; if(box < dIce) dIce = box;
                } else {
                    float crackT = relTime - 1.5; vec3 impactP = vec3(launchPos.x, 0.25, launchPos.y); vec3 localP = p2 - impactP; float explode = crackT * 4.0; float force = smoothstep(0.0, 0.5, crackT); vec3 cellI = floor(localP * 5.0); float cellHash = fract(sin(dot(cellI, vec3(12.9898, 78.233, 45.164))) * 43758.5453123);
                    vec3 dir = fract(sin(vec3(cellHash, cellHash + 1.0, cellHash + 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423)) - 0.5; dir.y = abs(dir.y) * 1.5; dir = normalize(dir);
                    localP -= dir * force * (0.1 + cellHash * 0.6); localP.y -= 0.25; float a3 = cellHash * crackT * 5.0; float s3 = sin(a3), c3 = cos(a3); localP.xz = mat2(c3, -s3, s3, c3) * localP.xz; float a4 = dir.z * crackT * 3.0; float s4 = sin(a4), c4 = cos(a4); localP.xy = mat2(c4, -s4, s4, c4) * localP.xy; localP.y += mix(0.25, -0.2, smoothstep(0.0, 1.0, crackT));
                    vec3 q1 = abs(localP) - vec3(0.25); float box = length(max(q1, 0.0)) + min(max(q1.x, max(q1.y, q1.z)), 0.0);
                    vec3 vp = localP * 12.0; vec3 vi = floor(vp); vec3 vf = fract(vp); float minDist = 1.0;
                    for(int vz = -1; vz <= 1; vz++) { for(int vy = -1; vy <= 1; vy++) { for(int vx = -1; vx <= 1; vx++) {
                        vec3 vg = vec3(float(vx), float(vy), float(vz)); vec3 vo = fract(sin(vec3(dot(vi + vg, vec3(127.1, 311.7, 74.7))) + vec3(0.0, 1.0, 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423));
                        vec3 vr = vg + vo - vf; float vd = dot(vr, vr); if(vd < minDist) minDist = vd;
                    } } }
                    box -= sqrt(minDist) * 0.03; vec3 q2 = abs(p2 - impactP - vec3(0.0, explode * 0.2, 0.0)) - vec3(0.3 + explode * 1.5); float bounds = length(max(q2, 0.0)) + min(max(q2.x, max(q2.y, q2.z)), 0.0); box = max(box, bounds);
                    if(box < dIce && crackT < 1.5) dIce = box;
                }
            }
            s2.x = min(dPlane, dIce);
            
            dPlane = p3.y; dIce = 15.0;
            for(float j = 0.0; j < 4.0; j++) {
                float timeSec = iTime * 1.2 + j * 2.1; float cycle = floor(timeSec / 4.0); float relTime = mod(timeSec, 4.0); float id = j + cycle * 7.33;
                vec3 h3 = fract(sin(vec3(id, id + 1.0, id + 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423)); vec2 launchPos = (h3.xy - 0.5) * 5.0; vec3 iceP = p3;
                if(relTime < 1.5) {
                    float fallT = relTime / 1.5; float y = mix(6.0, 0.25, fallT * fallT); iceP -= vec3(launchPos.x, y, launchPos.y);
                    float a1 = relTime * 4.0 + id; float s1 = sin(a1), c1 = cos(a1); iceP.xz = mat2(c1, -s1, s1, c1) * iceP.xz;
                    float a2 = relTime * 2.0 - id; float s2 = sin(a2), c2 = cos(a2); iceP.xy = mat2(c2, -s2, s2, c2) * iceP.xy;
                    vec3 q = abs(iceP) - vec3(0.25); float box = length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
                    vec3 vp = iceP * 12.0; vec3 vi = floor(vp); vec3 vf = fract(vp); float minDist = 1.0;
                    for(int vz = -1; vz <= 1; vz++) { for(int vy = -1; vy <= 1; vy++) { for(int vx = -1; vx <= 1; vx++) {
                        vec3 vg = vec3(float(vx), float(vy), float(vz)); vec3 vo = fract(sin(vec3(dot(vi + vg, vec3(127.1, 311.7, 74.7))) + vec3(0.0, 1.0, 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423));
                        vec3 vr = vg + vo - vf; float vd = dot(vr, vr); if(vd < minDist) minDist = vd;
                    } } }
                    box -= sqrt(minDist) * 0.03; if(box < dIce) dIce = box;
                } else {
                    float crackT = relTime - 1.5; vec3 impactP = vec3(launchPos.x, 0.25, launchPos.y); vec3 localP = p3 - impactP; float explode = crackT * 4.0; float force = smoothstep(0.0, 0.5, crackT); vec3 cellI = floor(localP * 5.0); float cellHash = fract(sin(dot(cellI, vec3(12.9898, 78.233, 45.164))) * 43758.5453123);
                    vec3 dir = fract(sin(vec3(cellHash, cellHash + 1.0, cellHash + 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423)) - 0.5; dir.y = abs(dir.y) * 1.5; dir = normalize(dir);
                    localP -= dir * force * (0.1 + cellHash * 0.6); localP.y -= 0.25; float a3 = cellHash * crackT * 5.0; float s3 = sin(a3), c3 = cos(a3); localP.xz = mat2(c3, -s3, s3, c3) * localP.xz; float a4 = dir.z * crackT * 3.0; float s4 = sin(a4), c4 = cos(a4); localP.xy = mat2(c4, -s4, s4, c4) * localP.xy; localP.y += mix(0.25, -0.2, smoothstep(0.0, 1.0, crackT));
                    vec3 q1 = abs(localP) - vec3(0.25); float box = length(max(q1, 0.0)) + min(max(q1.x, max(q1.y, q1.z)), 0.0);
                    vec3 vp = localP * 12.0; vec3 vi = floor(vp); vec3 vf = fract(vp); float minDist = 1.0;
                    for(int vz = -1; vz <= 1; vz++) { for(int vy = -1; vy <= 1; vy++) { for(int vx = -1; vx <= 1; vx++) {
                        vec3 vg = vec3(float(vx), float(vy), float(vz)); vec3 vo = fract(sin(vec3(dot(vi + vg, vec3(127.1, 311.7, 74.7))) + vec3(0.0, 1.0, 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423));
                        vec3 vr = vg + vo - vf; float vd = dot(vr, vr); if(vd < minDist) minDist = vd;
                    } } }
                    box -= sqrt(minDist) * 0.03; vec3 q2 = abs(p3 - impactP - vec3(0.0, explode * 0.2, 0.0)) - vec3(0.3 + explode * 1.5); float bounds = length(max(q2, 0.0)) + min(max(q2.x, max(q2.y, q2.z)), 0.0); box = max(box, bounds);
                    if(box < dIce && crackT < 1.5) dIce = box;
                }
            }
            s3.x = min(dPlane, dIce);
            
            n = s0.x - vec3(s1.x, s2.x, s3.x);
        }
        n = normalize(n);
        
        vec3 lightPos = vec3(4.0, 8.0, -3.0);
        vec3 l = normalize(lightPos - p);
        vec3 v = normalize(ro - p);
        vec3 r = reflect(-l, n);
        
        float shadow = 1.0;
        {
            float st = 0.02;
            vec3 sro = p + n * 0.01;
            int SHADOW_RAY_STEPS = int(max(8.0, floor(32.0 * customQualityScale)));
            for(int i = 0; i < SHADOW_RAY_STEPS; i++) {
                vec3 sp = sro + l * st;
                float dPlane = sp.y; float dIce = 15.0;
                for(float j = 0.0; j < 4.0; j++) {
                    float timeSec = iTime * 1.2 + j * 2.1; float cycle = floor(timeSec / 4.0); float relTime = mod(timeSec, 4.0); float id = j + cycle * 7.33;
                    vec3 h3 = fract(sin(vec3(id, id + 1.0, id + 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423)); vec2 launchPos = (h3.xy - 0.5) * 5.0; vec3 iceP = sp;
                    if(relTime < 1.5) {
                        float fallT = relTime / 1.5; float y = mix(6.0, 0.25, fallT * fallT); iceP -= vec3(launchPos.x, y, launchPos.y);
                        float a1 = relTime * 4.0 + id; float s1 = sin(a1), c1 = cos(a1); iceP.xz = mat2(c1, -s1, s1, c1) * iceP.xz;
                        float a2 = relTime * 2.0 - id; float s2 = sin(a2), c2 = cos(a2); iceP.xy = mat2(c2, -s2, s2, c2) * iceP.xy;
                        vec3 q = abs(iceP) - vec3(0.25); float box = length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
                        vec3 vp = iceP * 12.0; vec3 vi = floor(vp); vec3 vf = fract(vp); float minDist = 1.0;
                        for(int vz = -1; vz <= 1; vz++) { for(int vy = -1; vy <= 1; vy++) { for(int vx = -1; vx <= 1; vx++) {
                            vec3 vg = vec3(float(vx), float(vy), float(vz)); vec3 vo = fract(sin(vec3(dot(vi + vg, vec3(127.1, 311.7, 74.7))) + vec3(0.0, 1.0, 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423));
                            vec3 vr = vg + vo - vf; float vd = dot(vr, vr); if(vd < minDist) minDist = vd;
                        } } }
                        box -= sqrt(minDist) * 0.03; if(box < dIce) dIce = box;
                    } else {
                        float crackT = relTime - 1.5; vec3 impactP = vec3(launchPos.x, 0.25, launchPos.y); vec3 localP = sp - impactP; float explode = crackT * 4.0; float force = smoothstep(0.0, 0.5, crackT); vec3 cellI = floor(localP * 5.0); float cellHash = fract(sin(dot(cellI, vec3(12.9898, 78.233, 45.164))) * 43758.5453123);
                        vec3 dir = fract(sin(vec3(cellHash, cellHash + 1.0, cellHash + 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423)) - 0.5; dir.y = abs(dir.y) * 1.5; dir = normalize(dir);
                        localP -= dir * force * (0.1 + cellHash * 0.6); localP.y -= 0.25; float a3 = cellHash * crackT * 5.0; float s3 = sin(a3), c3 = cos(a3); localP.xz = mat2(c3, -s3, s3, c3) * localP.xz; float a4 = dir.z * crackT * 3.0; float s4 = sin(a4), c4 = cos(a4); localP.xy = mat2(c4, -s4, s4, c4) * localP.xy; localP.y += mix(0.25, -0.2, smoothstep(0.0, 1.0, crackT));
                        vec3 q1 = abs(localP) - vec3(0.25); float box = length(max(q1, 0.0)) + min(max(q1.x, max(q1.y, q1.z)), 0.0);
                        vec3 vp = localP * 12.0; vec3 vi = floor(vp); vec3 vf = fract(vp); float minDist = 1.0;
                        for(int vz = -1; vz <= 1; vz++) { for(int vy = -1; vy <= 1; vy++) { for(int vx = -1; vx <= 1; vx++) {
                            vec3 vg = vec3(float(vx), float(vy), float(vz)); vec3 vo = fract(sin(vec3(dot(vi + vg, vec3(127.1, 311.7, 74.7))) + vec3(0.0, 1.0, 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423));
                            vec3 vr = vg + vo - vf; float vd = dot(vr, vr); if(vd < minDist) minDist = vd;
                        } } }
                        box -= sqrt(minDist) * 0.03; vec3 q2 = abs(sp - impactP - vec3(0.0, explode * 0.2, 0.0)) - vec3(0.3 + explode * 1.5); float bounds = length(max(q2, 0.0)) + min(max(q2.x, max(q2.y, q2.z)), 0.0); box = max(box, bounds);
                        if(box < dIce && crackT < 1.5) dIce = box;
                    }
                }
                float sh = min(dPlane, dIce);
                if(sh < 0.001) { shadow = 0.0; break; }
                shadow = min(shadow, 8.0 * sh / st);
                st += sh;
                if(st > 8.0) break;
            }
            shadow = clamp(shadow, 0.0, 1.0);
        }
        
        float diff = max(dot(n, l), 0.0) * shadow;
        float spec = pow(max(dot(r, v), 0.0), 32.0) * shadow;
        
        if(scene.y == 0.0) {
            float check = mod(floor(p.x * 2.0) + floor(p.z * 2.0), 2.0);
            vec3 floorCol = mix(vec3(0.15, 0.18, 0.22), vec3(0.1, 0.12, 0.14), check);
            col = floorCol * (diff + 0.3) + spec * 0.3;
        } 
        else if(scene.y == 1.0) {
            float fresnel = pow(1.0 - max(dot(n, v), 0.0), 5.0);
            vec3 refDir = reflect(rd, n);
            vec3 refSky = vec3(0.12, 0.15, 0.2) - refDir.y * 0.2;
            
            vec3 refrDir = refract(rd, n, 1.0 / 1.31);
            float internalTrans = max(dot(n, -refrDir), 0.0);
            vec3 iceInterior = vec3(0.7, 0.88, 0.98) * (internalTrans * 0.6 + 0.4);
            
            col = mix(iceInterior, refSky, fresnel);
            col += vec3(1.0) * spec * 1.2;
            col += vec3(0.8, 0.95, 1.0) * diff * 0.4;
            
            float depthNoise = 0.0;
            {
                vec3 vp = p * 5.0; vec3 vi = floor(vp); vec3 vf = fract(vp); float minDist = 1.0;
                for(int vz = -1; vz <= 1; vz++) { for(int vy = -1; vy <= 1; vy++) { for(int vx = -1; vx <= 1; vx++) {
                    vec3 vg = vec3(float(vx), float(vy), float(vz)); vec3 vo = fract(sin(vec3(dot(vi + vg, vec3(127.1, 311.7, 74.7))) + vec3(0.0, 1.0, 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423));
                    vec3 vr = vg + vo - vf; float vd = dot(vr, vr); if(vd < minDist) minDist = vd;
                } } }
                depthNoise = sqrt(minDist);
            }
            col += vec3(0.3, 0.5, 0.6) * (1.0 - depthNoise) * 0.25;
        }
        
        col = mix(col, vec3(0.12, 0.15, 0.2), 1.0 - exp(-0.01 * t * t));
    }
    
    col = pow(col, vec3(0.4545));
    fragColor = vec4(col, 1.0);
}