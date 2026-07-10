// La Chutte
// Shadertoy ID: fcSSzW
// Description: Chutte
// Tags: tunnel

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    
    float i = 0.0, e = 0.0, R = 0.0, s = 0.0;
    vec3 q = vec3(0.0), p = vec3(0.0);
    vec3 d = vec3(uv.x, 0.9, uv.y);
    vec3 accumulatedColor = vec3(0.0);
    
    q.yz -= 1.0;
    
    float morphTime = iTime * 0.52359877; 
    float morph = sin(morphTime) * 0.5 + 0.5;

    for(int it = 0; it < 80; it++) {
        float glow = min(e * s, 0.6 - e) * 0.03;
        
        accumulatedColor += vec3(glow * 2.5);
        
        s = 2.0;
        p = q += d * e * R * 0.3;
        R = length(p);
        
        p = vec3(log(R + 0.001) - iTime * 0.5, exp(-p.y / R + 0.5), atan(p.x, p.z));
        e = p.y - 1.0;
        
        for(int j = 0; j < 7; j++) {
            vec3 s_vec = p * s;
            float fbm1 = -abs(dot(cos(p.zxy * s), 0.2 - sin(s_vec))) / s * 0.4;
            float fbm2 = -abs(dot(sin(p.yzx * s), 0.2 - cos(s_vec))) / s * 0.4;
            e += mix(fbm1, fbm2, morph);
            s *= 2.0;
            if(s > 1000.0) break;
        }
    }

    accumulatedColor = pow(accumulatedColor, vec3(0.4545));

    fragColor = vec4(accumulatedColor, 1.0);
}