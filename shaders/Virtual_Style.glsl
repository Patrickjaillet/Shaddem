// Virtual Style
// Shadertoy ID: 7XjGz3
// Description: Virtual Style
// Tags: tunnel

void mainImage(out vec4 fragColor, in vec2 fragCoord) {
    vec2 uv = (fragCoord - 0.5 * iResolution.xy) / iResolution.y;
    
    float t = iTime * 0.4;
    float a = atan(uv.y, uv.x);
    float r = length(uv);
    
    float depth = 1.0 / max(r, 0.0001);
    
    vec3 ro = vec3(0.0, 0.0, t * 1.5);
    vec3 rd = normalize(vec3(uv, -1.0));
    
    float s = sin(t * 0.2);
    float c = cos(t * 0.2);
    rd.xy *= mat2(c, -s, s, c);
    
    float d = 0.0;
    float totGlow = 0.0;
    vec3 col = vec3(0.0);
    
    for(int i = 0; i < 80; i++) {
        vec3 p = ro + rd * d;
        
        float pA = atan(p.y, p.x);
        float pR = length(p.xy);
        
        float tunnelDist = -(pR - 1.5);
        
        float twist = p.z * 0.15 + pA * 3.0;
        float disp = sin(twist + t * 2.0) * cos(p.z * 0.8 - t) * 0.25;
        disp += sin(pA * 8.0 - p.z * 1.2) * 0.08;
        
        float sceneDist = tunnelDist + disp;
        
        float pGlow = exp(-abs(sceneDist) * 12.0);
        totGlow += pGlow;
        
        vec3 pColor = vec3(0.0);
        pColor += vec3(0.5, 0.05, 0.9) * (sin(p.z * 0.5 + 0.0) * 0.5 + 0.5);
        pColor += vec3(0.0, 0.8, 1.0) * (cos(p.z * 0.3 + 2.0) * 0.5 + 0.5);
        pColor += vec3(1.0, 0.0, 0.4) * (sin(pA * 4.0 - p.z) * 0.5 + 0.5);
        
        col += pColor * pGlow * exp(-d * 0.15);
        
        if(sceneDist < 0.001 || d > 40.0) break;
        d += max(abs(sceneDist) * 0.4, 0.01);
    }
    
    col *= 0.14;
    
    float centerGlow = exp(-r * 3.5);
    col += vec3(1.0, 0.85, 0.4) * centerGlow * 1.8;
    
    float spec = pow(max(0.0, sin(a * 12.0 + t * 4.0) * cos(depth * 0.5 - t * 8.0)), 32.0);
    col += vec3(0.8, 0.9, 1.0) * spec * exp(-d * 0.08) * 2.5;
    
    col = mix(col, vec3(0.0), 1.0 - exp(-1.2 * r * r));
    
    col = col / (col + vec3(1.0));
    col = pow(col, vec3(1.0 / 2.2));
    
    fragColor = vec4(col, 1.0);
}