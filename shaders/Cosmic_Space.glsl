// Cosmic Space
// Shadertoy ID: N3lGWl
// Description: https://github.com/Patrickjaillet
// Tags: starfield

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
}
vec3 getStarField(vec2 uv,float zoom,float time,float seed){
    vec2 gv=fract(uv*zoom)-.5;
    vec2 id=floor(uv*zoom);
    vec3 col=vec3(0.);
    for(int y=-1;
    y<=0;
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
    // https://github.com/Patrickjaillet/Z-GL
}
void mainImage(out vec4 fragColor,in vec2 fragCoord){
    vec2 r=iResolution.xy;
    vec2 uv=(fragCoord-.5*r)/r.y;
    float t=iTime;
    vec3 starLayer=vec3(0.);
    float noise=hash12(fragCoord+t);
    float starTime=t*.15;
    vec2 camPath=vec2(sin(starTime*.5),cos(starTime*.3))*2.;
    float camRot=sin(starTime*.2)*.4;
    for(float i=0.;
    i<.4;
    i+=1./24.){
        float depth=fract(i-starTime*.5);
        float zoom=mix(15.,.05,depth);
        float fade=smoothstep(0.,.4,depth)*smoothstep(1.,.8,depth);
        vec2 p_uv=uv;
        p_uv*=rot(camRot*depth);
        p_uv+=camPath*depth;
        starLayer+=getStarField(p_uv,zoom,t,i*951.4)*fade;
    }
    starLayer=pow(starLayer,vec3(.8))*1.2;
    starLayer+=(noise-.5)*.012;
    vec3 bloom=starLayer*starLayer;
    starLayer+=bloom*.3;
    starLayer=mix(starLayer,vec3(dot(starLayer,vec3(.299,.587,.114))),-.1);
    float vign=length(fragCoord/r-.5);
    starLayer*=smoothstep(1.2,.3,vign);
    vec4 fractalCol=vec4(0);
    for(float i=0.,g=0.,e,s;
    ++i<49.;
    ){
        vec3 p=vec3((fragCoord-.5*r)/r.y*2.2,g-2.5);
        float a=t*.2;
        mat2 m=mat2(cos(a),sin(a),-sin(a),cos(a));
        p.xz*=m;
        p.xy*=m;
        s=.3;
        for(int j=0;
        j<5;
        j++){
            p=abs(p)-vec3(1.2,2.5,1.8);
            if(p.x<p.y)p.yx=p.xy;
            if(p.x<p.z)p.zx=p.xz;
            if(p.y<p.z)p.zy=p.yz;
            e=1.85/clamp(dot(p,p),.15,1.);
            p=p*e-vec3(.5,4.,.2);
            s*=e;
        }
        float d=length(p.xz)/s;
        g+=d*.2;
        vec3 c=1.+1.*cos(log(s)*.5+vec3(0,1,2)+g);
        fractalCol.rgb+=c*(.02/(.01+d*d*4e2));
    }
    fractalCol.rgb=pow(fractalCol.rgb,vec3(.4545));
    float mask=smoothstep(.01,.2,length(fractalCol.rgb));
    fragColor=vec4(mix(starLayer,fractalCol.rgb,mask),1.);
}