// Utilise la rotation matricielle de ta source [cite: 4]
vec2 rotate(vec2 p, float a){
    return mat2(cos(a),-sin(a),sin(a),cos(a))*p;
}

float map(vec3 p){
    p.xy = rotate(p.xy, p.z * 0.1 + iTime * 0.2); // Torsion
    p.xy = mod(p.xy + 2.0, 4.0) - 2.0; // Répétition infinie
    return length(max(abs(p) - vec3(0.5, 0.5, 10.0), 0.0));
}

void mainImage(out vec4 fragColor, in vec2 fragCoord){
    vec2 uv = (fragCoord - 0.5*iResolution.xy) / iResolution.y;
    vec3 ro = vec3(0.0, 0.0, iTime * 5.0);
    vec3 rd = normalize(vec3(uv, 1.0));
    
    float t = 0.0;
    for(int i = 0; i < 100; i++){
        float d = map(ro + rd * t);
        if(d < 0.01) break;
        t += d * 0.5;
    }
    vec3 col = vec3(0.1, 0.8, 0.4) * exp(-t * 0.1); // Brouillard technologique
    fragColor = vec4(col, 1.0);
}