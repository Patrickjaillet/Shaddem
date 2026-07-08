// Basé sur le shading chrome de la source [cite: 25, 26]
float map(vec3 p){
    float t = iTime * 0.5;
    p.zy = vec2(sin(p.z*0.5+t), cos(p.y*0.5+t)); // Distorsion spatiale
    return length(p) - (1.5 + sin(p.x + t)*0.2);
}

void mainImage(out vec4 fragColor, in vec2 fragCoord){
    vec2 uv = (fragCoord - 0.5*iResolution.xy) / iResolution.y;
    vec3 ro = vec3(0.0, 0.0, -3.0);
    vec3 rd = normalize(vec3(uv, 1.2));
    
    float t = 0.0;
    for(int i = 0; i < 80; i++){
        float d = map(ro + rd * t);
        if(d < 0.001 || t > 10.0) break;
        t += d;
    }
    
    vec3 col = vec3(0.02);
    if(t < 10.0){
        vec3 p = ro + rd * t;
        vec2 e = vec2(0.01, 0.0);
        vec3 n = normalize(vec3(map(p+e.xyy)-map(p-e.xyy), map(p+e.yxy)-map(p-e.yxy), map(p+e.yyx)-map(p-e.yyx)));
        vec3 r = reflect(rd, n);
        col = mix(vec3(0.05), vec3(0.4, 0.45, 0.5), r.y * 0.5 + 0.5); // Reflet ciel [cite: 17]
        col += pow(max(dot(n, normalize(vec3(1,2,-1))), 0.0), 32.0); // Specular [cite: 28]
    }
    fragColor = vec4(col, 1.0);
}