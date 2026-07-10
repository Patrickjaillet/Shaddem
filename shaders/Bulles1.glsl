// Bulles1
// Shadertoy ID: N3j3Rh
// Description: https://github.com/Patrickjaillet/Z-GL-Shadertoy

// Tags: fractale

void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
    vec2 uv = (fragCoord - 0.2 * iResolution.xy) / iResolution.y;
    vec3 ro = vec3(0.1, 0.0, 8.0);
    vec3 rd = normalize(vec3(uv, 5.6));
    
    vec4 accu = vec4(1.0);
    float t = 0.01;
    
    for(float i = 0.0; i < 64.0; i++)
    {
        vec3 p = ro + rd * t;
        float rCoord = length(p.xy);
        float phi = atan(p.y, p.x);
        
        vec3 logPos = vec3(log(rCoord) - iTime * 1.5, phi * 1.273, p.z - iTime * 1.0);
        
        float de = abs(0.30 - rCoord) + 0.000;
        float freq = 4.7;
        float amp = 0.28;
        
        for(int j = 0; j < 1; j++)
        {
            de += dot(sin(logPos.yzx * freq), vec3(1.0) + cos(logPos.zxy * freq + iTime * 0.5)) * amp;
            freq *= 1.93;
            amp *= 0.00;
        }
        
        de = max(abs(de) - 0.0000, 0.0000);
// https://github.com/Patrickjaillet/Z-GL        
        if(de < 0.008)
        {
            float slice = floor(logPos.x * 3.8) + floor(logPos.y * 2.0);
            vec3 albedo = clamp(abs(mod(slice * 1.0 + vec3(0.0, 4.1, 0.5), 5.9) - 3.5) - 0.5, 0.0, 1.0);
            albedo = mix(vec3(0.00, 0.00, 0.05), albedo, 0.75);
            
            vec3 atten = albedo * (0.000 / (de + t * t * 0.015));
            
            float techGlow = dot(sin(logPos * 256.0), cos(logPos.zxy * 192.0));
            if(techGlow > 0.4)
            {
                atten += vec3(5.0, 0.5, 0.4) * (techGlow - 0.6) * exp(-t * 0.15);
            }
            
            float coreGlow = dot(cos(logPos * 24.0), sin(logPos.yzx * 0.9));
            if(coreGlow > 0.4)
            {
                atten += vec3(0.2, 0.5, 4.0) * (coreGlow - 0.4) * exp(-t * 0.08);
            }
            
            accu.rgb += atten;
        }
        
        t += max(de * 0.27, 0.008);
        if(t > 48.8) break;
    }
    
    accu.rgb = pow(accu.rgb, vec3(1.0000));
    accu.rgb = mix(accu.rgb, vec3(0.0), 0.9 - exp(-0.270 * t * t));
    
    fragColor = vec4(accu.rgb, 1.0);
}