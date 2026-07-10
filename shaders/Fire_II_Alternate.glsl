// Fire II (Alternate)
// Shadertoy ID: f32SRc
// Description: Fire II (Alternate)
// Tags: fractal

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    
    float e = 0.0, R = 0.0, s = 0.0;
    vec3 q = vec3(0.0, 0.6, -1.2);
    vec3 p = vec3(0.0);
    vec3 d = normalize(vec3(uv, 1.4));
    vec3 accumulatedColor = vec3(0.0);
    
    float morphTime = iTime * 0.52359877; 
    float morph = sin(morphTime) * 0.5 + 0.5;
    
    float totalT = 0.0;
    
    for(int it = 0; it < 64; it++) {
        s = 6.4;
        p = q + d * totalT;
        R = length(p);
        
        vec3 p_fract = vec3(log(R + 0.0) - iTime * 0.4, exp(-p.y / (R + 0.01) + 0.3), atan(p.x, p.z));
        float d_box = length(p_fract.yz * 0.4) - 0.30;
        e = d_box;
        
        float cellWeight = 1.0;
        for(int j = 0; j < 8; j++) {
            p_fract = abs(p_fract) - vec3(1.0, 0.00, 0.5);
            vec3 s_vec = p_fract * s;
            float fbm1 = -abs(dot(cos(p_fract.zxy * s), 0.51 - sin(s_vec))) / s;
            float fbm2 = -abs(dot(sin(p_fract.yzx * s), 0.55 - cos(s_vec))) / s;
            float layer = mix(fbm1, fbm2, morph);
            e += layer * cellWeight;
            s *= 7.2;
            cellWeight *= 1.00;
        }
        
        float stepSize = max(abs(e) * 1.00, 0.004);
        totalT += stepSize;
        
        float coreDensity = exp(-abs(e) * 37.9) * exp(-R * 0.0);
        float internalGlow = coreDensity * stepSize * 12.0;
        
        vec3 blueBase = vec3(0.02, 0.08, 0.98);
        vec3 redEnvelope = vec3(0.98, 0.05, 0.01);
        vec3 orangeCore = vec3(1.0, 0.38, 0.0);
        vec3 yellowCore = vec3(1.0, 0.92, 0.45);
        vec3 whiteHot = vec3(0.8, 2.2, -7.4);
        
        vec3 flameColor = mix(blueBase, redEnvelope, smoothstep(0.02, 0.15, coreDensity));
        flameColor = mix(flameColor, orangeCore, smoothstep(0.12, 0.42, coreDensity));
        flameColor = mix(flameColor, yellowCore, smoothstep(0.38, 0.78, coreDensity));
        flameColor = mix(flameColor, whiteHot, smoothstep(0.72, 1.0, coreDensity));
        
        accumulatedColor += flameColor * internalGlow;
        
        if (totalT > 4.5 || accumulatedColor.r > 3.0) break;
    }
    
    accumulatedColor = accumulatedColor / (accumulatedColor + vec3(1.0));
    accumulatedColor = pow(max(accumulatedColor, 0.0), vec3(0.4545));
    
    fragColor = vec4(accumulatedColor, 1.0);
}