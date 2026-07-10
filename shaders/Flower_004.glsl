// Flower 004
// Shadertoy ID: NXBSD3
// Description: Flower 004
// Tags: fractal

mat2 rot(float a) {
    float c = cos(a), s = sin(a);
    return mat2(c, -s, s, c);
}

float hash12(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * vec3(0.1031, 0.1136, 0.1377));
    p3 += dot(p3, p3.yzx + 19.19);
    return fract((p3.x + p3.y) * p3.z);
}

float noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash12(i + vec2(0.0, 0.0)), hash12(i + vec2(1.0, 0.0)), u.x),
               mix(hash12(i + vec2(0.0, 1.0)), hash12(i + vec2(1.0, 1.0)), u.x), u.y);
}

float pShape(float theta, float r, float numPetals, float sharpness, float waveAmp, float waveFreq, out float localX, float fbmWarp) {
    float angleStep = 6.2831853 / numPetals;
    float localAngle = mod(theta + angleStep * 0.5, angleStep) - angleStep * 0.5;
    localX = localAngle / (angleStep * 0.5);
    float profile = cos(localX * 1.5707963);
    float baseRadius = pow(max(0.0, profile), sharpness);
    float waves = sin(r * waveFreq - localAngle * 12.0 + fbmWarp * 4.0) * waveAmp * (1.0 - abs(localX)) * (0.2 + 0.8 * r);
    return baseRadius + waves;
}

vec4 renderLayer(float theta, float r, float numPetals, float size, float sharpness, vec3 innerCol, vec3 outerCol, float seed, float t, float h, float nVal) {
    float localX;
    float wFreq = 35.0 + h * 20.0;
    float wAmp = 0.04 + 0.02 * sin(t + seed);
    float currentSize = size * (1.0 + 0.04 * sin(t * 1.8 + seed * 6.28));

    float rWarped = r + nVal * 0.02 * (1.0 - smoothstep(currentSize, currentSize * 1.5, r));
    float petalR = pShape(theta + seed + sin(rWarped * 5.0 - t * 1.2) * 0.03, rWarped, numPetals, sharpness, wAmp, wFreq, localX, nVal);
    float normR = rWarped / (currentSize * petalR);

    float edgeWidth = fwidth(normR) * 1.6;
    float mask = smoothstep(1.0, 1.0 - edgeWidth, normR) * smoothstep(-edgeWidth, edgeWidth, normR);
    if (mask <= 0.0005) return vec4(0.0);

    float midrib = cos(localX * 1.570796) * (1.0 - normR) * (0.4 + 0.6 * sin(normR * 12.0));
    float vNoise = hash12(vec2(floor(localX * 60.0), floor(normR * 120.0)));
    float veins = sin(normR * 90.0 + localX * 30.0 + vNoise * 1.5 + nVal * 2.0) * 0.045 * (1.0 - abs(localX)) * normR;

    vec3 col = mix(innerCol, outerCol, smoothstep(0.15, 0.85, normR));
    col = mix(col, col * 1.3 + vec3(0.1, 0.0, 0.15) * normR * nVal, 0.2);
    col += vec3(0.8, 0.25, 0.9) * midrib * 0.6 + vec3(0.95, 0.85, 1.0) * veins;

    col += vec3(1.0, 0.9, 0.95) * smoothstep(0.975, 1.0, normR) * smoothstep(1.0, 0.996, normR) * (0.5 + 0.5 * sin(theta * 2.0 + t));
    col *= (0.3 + 0.7 * smoothstep(-0.8, 0.8, cos(localX * 3.141592))) * (0.45 + 0.55 * smoothstep(0.0, 0.4, normR)) * mix(0.3, 1.0, smoothstep(0.0, 0.3, normR));
    col += vec3(1.0) * pow(max(0.0, cos(localX * 1.2) * (1.0 - normR * 0.5)), 8.0) * 0.15;

    return vec4(col, mask * (1.0 - smoothstep(0.6, 1.0, normR) * 0.4));
}

vec3 getScene(vec2 uv, float t, float nVal, float scaleProj) {
    float theta = atan(uv.y, uv.x);
    float r = length(uv);

    vec3 cIn[5]; vec3 cOut[5]; float pars[5];
    cIn[0] = vec3(0.28, 0.0,  0.25); cOut[0] = vec3(0.05, 0.0,  0.15); pars[0] = 0.33;
    cIn[1] = vec3(0.55, 0.0,  0.35); cOut[1] = vec3(0.15, 0.0,  0.28); pars[1] = 1.74;
    cIn[2] = vec3(0.85, 0.05, 0.40); cOut[2] = vec3(0.30, 0.0,  0.32); pars[2] = 3.45;
    cIn[3] = vec3(1.0,  0.20, 0.38); cOut[3] = vec3(0.50, 0.02, 0.25); pars[3] = 5.12;
    cIn[4] = vec3(1.0,  0.50, 0.20); cOut[4] = vec3(0.80, 0.05, 0.15); pars[4] = 2.19;

    float steps[5]; float sharps[5];
    steps[0] = 15.0; sharps[0] = 0.45;
    steps[1] = 12.0; sharps[1] = 0.65;
    steps[2] = 10.0; sharps[2] = 0.90;
    steps[3] = 8.0;  sharps[3] = 1.20;
    steps[4] = 6.0;  sharps[4] = 1.55;

    float sizes[5];
    sizes[0] = 0.94; sizes[1] = 0.80; sizes[2] = 0.66; sizes[3] = 0.52; sizes[4] = 0.38;

    vec3 finalCol = vec3(0.0);
    for (int i = 0; i < 5; i++) {
        vec4 layer = renderLayer(theta, r * scaleProj, steps[i], sizes[i], sharps[i], cIn[i], cOut[i], pars[i], t, float(i) * 0.25, nVal);
        finalCol = mix(finalCol, layer.rgb, layer.a);
    }

    float stamenLimit = 0.185 * (1.0 + 0.03 * sin(t * 4.0));
    if (r * scaleProj < stamenLimit) {
        float stamenMask = smoothstep(stamenLimit, stamenLimit - 0.015, r * scaleProj);
        float sTheta = theta + sin(r * scaleProj * 36.0 - t * 2.5) * 0.3;
        float dots = smoothstep(0.3, 0.93, sin(sTheta * 54.0 + t * 1.2) * sin(r * scaleProj * 210.0) * sin(sTheta * 36.0 - t * 1.7) * sin(r * scaleProj * 150.0) + nVal * 0.2);
        vec3 stamenCol = mix(vec3(0.9, 0.28, 0.05), vec3(1.0, 0.92, 0.2), r * scaleProj / stamenLimit) + vec3(1.0, 1.0, 0.8) * dots * 0.85;
        stamenCol = mix(stamenCol, vec3(0.08, 0.32, 0.08), smoothstep(0.045, 0.0, r * scaleProj)) + vec3(0.95, 0.95, 0.6) * smoothstep(0.008, 0.0, r * scaleProj);
        finalCol = mix(finalCol, stamenCol, stamenMask);
    }

    return finalCol;
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec3 tot = vec3(0.0);
    float t = iTime * 0.4;

    vec2 offsets[4];
    offsets[0] = vec2( 0.25,  0.25); offsets[1] = vec2(-0.25, -0.25);
    offsets[2] = vec2(-0.25,  0.25); offsets[3] = vec2( 0.25, -0.25);

    vec2 baseUV = (fragCoord * 2.0 - iResolution.xy) / iResolution.y;
    float nVal = noise(baseUV * 2.0 + t * 0.2);

    float camT = t * 0.5;
    vec3 camPos = vec3(sin(camT) * 1.6, 0.6 + cos(camT * 0.7) * 0.4, cos(camT) * 1.6);
    vec3 target = vec3(0.0, -0.1, 0.0);

    vec3 cz = normalize(target - camPos);
    vec3 upAnim = vec3(sin(camT * 0.3) * 0.2, 1.0, cos(camT * 0.5) * 0.1);
    vec3 cx = normalize(cross(cz, upAnim));
    vec3 cy = cross(cx, cz);

    for (int m = 0; m < 4; m++) {
        vec2 p = ((fragCoord + offsets[m]) * 2.0 - iResolution.xy) / iResolution.y;
        vec3 rd = normalize(p.x * cx + p.y * cy + 1.8 * cz);
        float distZ = -camPos.y / rd.y;

        if (distZ > 0.0 && camPos.y > 0.0) {
            vec3 intersect = camPos + rd * distZ;
            vec2 uv = intersect.xz;
            float scaleProj = mix(1.0, 1.3, smoothstep(0.0, 3.0, length(intersect)));
            float d = length(uv);
            uv *= rot(t * 0.06 + d * 0.04 + sin(d * 2.0 - t * 0.5) * 0.02);
            tot += getScene(uv, t, nVal, scaleProj);
        }
    }

    tot *= vec3(0.2875, 0.245, 0.26);

    vec2 uvScreen = fragCoord / iResolution.xy;
    tot *= mix(0.15, 1.0, clamp(pow(16.0 * uvScreen.x * uvScreen.y * (1.0 - uvScreen.x) * (1.0 - uvScreen.y), 0.38), 0.0, 1.0));

    tot = tot / (tot + vec3(1.0));
    tot = pow(tot, vec3(0.454545));

    fragColor = vec4(tot, 1.0);
}