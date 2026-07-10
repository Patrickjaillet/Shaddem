// Champ d'etoiles
// Shadertoy ID: fXXGRM
// Description: https://github.com/Patrickjaillet/Z-GL-Shadertoy
// Tags: stars

float hash12(vec2 p){
    vec3 p3=fract(vec3(p.xyx)*.1031);
    p3+=dot(p3,p3.yzx+33.33);
    return fract((p3.x+p3.y)*p3.z);
}
vec2 hash22(vec2 p){
    vec3 p3=fract(vec3(p.xyx)*vec3(.1031,.103,.0973));
    p3+=dot(p3,p3.yzx+33.33);
    return fract((p3.xx+p3.yz)*p3.zy);
}
mat2 rot(float a){
    float s=sin(a),c=cos(a);
    return mat2(c,-s,s,c);
} // https://github.com/Patrickjaillet/Z-GL-Shadertoy
vec3 getStarField(vec2 uv,float zoom,float time,float seed){
    vec2 gv=fract(uv*zoom)-.5;
    vec2 id=floor(uv*zoom);
    vec3 col=vec3(0.);
    for(int y=-1;
    y<=1;
    y++){
        for(int x=-1;
        x<=1;
        x++){
            vec2 offs=vec2(float(x),float(y));
            vec2 n=hash22(id+offs+seed);
            float pTime=time*(.3+n.x*.7)+n.y*6.28;
            float size=(.04+.12*hash12(id+offs+seed+121.3))*(sin(pTime)*.5+.5);
            vec2 p=offs+n-.5;
            float d=length(gv-p);
            vec3 starCol=mix(vec3(.5,.7,1.),vec3(1.,.5,.3),hash12(id+offs+seed+45.1));
            starCol=mix(starCol,vec3(1.,.9,.7),n.x*n.y);
            float light=(size*.015)/(d+5e-4);
            float glow=(size*.003)/(d*d+8e-5);
            vec2 r_uv=(gv-p)*rot(pTime*.5);
            float rays=pow(max(0.,1.-abs(r_uv.x*r_uv.y*1e3)),12.)*(size*.1/(d+.01));
            rays+=pow(max(0.,1.-abs(r_uv.x)),50.)*(size*.05/(d+.01));
            col+=(light+glow+rays)*starCol;
        }
    }
    return col;
}
void mainImage(out vec4 fragColor,in vec2 fragCoord){
    vec2 uv=(fragCoord-.5*iResolution.xy)/iResolution.y;
    float t=iTime*.15;
    vec2 camPath=vec2(sin(t*.5),cos(t*.3))*2.;
    float camRot=sin(t*.2)*.4;
    vec3 finalCol=vec3(0.);
    float noise=hash12(fragCoord+iTime);
    for(float i=0.;
    i<1.;
    i+=1./8.){
        float depth=fract(i-t*.5);
        float zoom=mix(15.,.05,depth);
        float fade=smoothstep(0.,.4,depth)*smoothstep(1.,.8,depth);
        vec2 p_uv=uv;
        p_uv*=rot(camRot*depth);
        p_uv+=camPath*depth;
        finalCol+=getStarField(p_uv,zoom,iTime,i*951.4)*fade;
    }
    finalCol=pow(finalCol,vec3(.8));
    finalCol*=1.2;
    vec2 screenUv=fragCoord/iResolution.xy;
    float vign=length(screenUv-.5);
    finalCol*=smoothstep(1.2,.3,vign);
    finalCol+=(noise-.5)*.012;
    vec3 bloom=finalCol*finalCol;
    finalCol+=bloom*.3;
    finalCol=mix(finalCol,vec3(dot(finalCol,vec3(.299,.587,.114))),-.1);
    fragColor=vec4(clamp(finalCol,0.,1.),1.);
}