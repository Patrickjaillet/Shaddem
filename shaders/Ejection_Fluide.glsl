// Ejection Fluide
// Shadertoy ID: sX2Szd
// Description: Ejection Fluide
// Tags: fractal

void mainImage(out vec4 fragColor, in vec2 fragCoord)
{
    vec3 col = vec3(0.0);
    vec2 trg = (fragCoord * 2.0 - iResolution.xy) / iResolution.y;
    
    vec3 p3 = fract(vec3(fragCoord.xyx + iTime) * 0.1031);
    p3 += dot(p3, p3.yzx + 333.33);
    float jitter = fract((p3.x + p3.y) * p3.z);
    
    for(int m = 0; m < 2; m++)
    {
        vec2 uv = (fragCoord + vec2(float(m) * 0.5, float(m == 0 ? 0 : 1) * 0.5) * jitter - iResolution.xy * 0.5) / iResolution.y;
        float t = iTime * 0.15;
        
        vec3 ro = vec3(2.5 * sin(t), 1.8 * cos(t * 0.3), -2.5 - 1.5 * sin(t * 0.5));
        vec3 ta = vec3(0.0, 0.0, 0.0);
        
        vec3 cw = normalize(ta - ro);
        vec3 cp = vec3(sin(t * 0.2), 1.0, 0.0);
        vec3 cu = normalize(cross(cw, cp));
        vec3 cv = cross(cu, cw);
        
        vec3 rd = normalize(uv.x * cu + uv.y * cv + 1.4 * cw);
        
        float tmin = 0.5;
        float tmax = 12.0;
        float numSteps = 128.0;
        float stepSize = (tmax - tmin) / numSteps;
        float rayPos = tmin + stepSize * jitter;
        
        vec4 accum = vec4(0.0);
        
        for(int i = 0; i < 128; i++)
        {
            if(accum.a >= 0.98) break;
            
            vec3 p = ro + rd * rayPos;
            float rSphere = length(p);
            
            if(rSphere < 6.5)
            {
                float sphereMask = smoothstep(6.5, 1.5, rSphere);
                float R = length(p * 0.25);
                
                float angle = atan(p.y, p.x) - iTime * 0.45;
                
                vec3 q = vec3(
                    log(R + 0.15),
                    exp2(-p.z / (R + 1.0)),
                    angle
                );
                
                float rotA = iTime * 0.08 + q.z * 0.2;
                float cA = cos(rotA), sA = sin(rotA);
                q.xy = vec2(q.x * cA - q.y * sA, q.x * sA + q.y * cA);
                
                float n = 0.0;
                float amp = 1.0;
                vec3 np = q * 8.0;
                
                for(int j = 0; j < 6; j++)
                {
                    float cR = 0.8660254, sR = 0.5000000;
                    np.xy = vec2(np.x * cR - np.y * sR, np.x * sR + np.y * cR);
                    
                    n += amp * abs(sin(dot(sin(np), cos(np.yzx + iTime * 0.8))));
                    np *= 2.15;
                    amp *= 0.52;
                }
                
                float density = max(0.0, (n - 0.28) * sphereMask);
                
                if(density > 0.0)
                {
                    vec3 lCol = mix(vec3(1.0, 0.12, 0.01), vec3(0.9, 0.00, 1.00), smoothstep(0.2, 0.7, density));
                    lCol = mix(lCol, vec3(6.0, 1.50, 4.00), smoothstep(0.6, 0.90, density));
                    lCol = mix(lCol, vec3(0.00, 1.50, 12.0), smoothstep(0.88, 1.0, density));
                    
                    float shadow = smoothstep(0.1, 0.9, n);
                    lCol *= shadow;
                    
                    float alpha = density * 0.18 * stepSize * 8.0;
                    accum.rgb += (1.0 - accum.a) * lCol * alpha;
                    accum.a += (1.0 - accum.a) * alpha;
                }
            }
            rayPos += stepSize;
        }
        col += accum.rgb;
    }
    
    col *= 0.5;
    
    col = mix(col, vec3(dot(col, vec3(0.2126, 0.7152, 0.0722))), -0.3);
    
    col = col / (1.0 + col);
    col = pow(col, vec3(1.0 / 2.2));
    
    float vig = smoothstep(2.5, 0.8, length(trg));
    col *= mix(0.1, 1.0, vig);
    
    fragColor = vec4(col, 1.0);
}