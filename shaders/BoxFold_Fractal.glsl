// BoxFold Fractal
// Shadertoy ID: ffSSWV
// Description: https://github.com/Patrickjaillet/Z-GL-Shadertoy
// Tags: raymarching, mandelbox, pbr, beerlambert

void rot(inout vec3 p,vec3 a){
    vec3 s=sin(a);
    vec3 c=cos(a);
    mat2 mX=mat2(c.x,-s.x,s.x,c.x);
    mat2 mY=mat2(c.y,-s.y,s.y,c.y);
    mat2 mZ=mat2(c.z,-s.z,s.z,c.z);
    p.yz*=mX;
    p.xz*=mY;
    p.xy*=mZ;
}
float map(vec3 p,float timeOffset,int iterations){
    float time=(iTime+timeOffset)*.1;
    rot(p,vec3(time,time*1.3,time*.7));
    float scale=1.;
    for(int i=0;
    i<iterations;
    i++){
        p=clamp(p,-1.,1.)*2.-p;
        float r2=dot(p,p);
        float k=max(1.2/r2,.4);
        p*=k;
        scale*=k;
    }
    return(length(p.xz)-.05)/scale;
}
vec3 getColor(float d,vec3 p,float timeOffset){
    return mix(vec3(.05,.2,.8),vec3(1.,.05,.2),smoothstep(-1.,1.,sin(length(p)*.5-(iTime+timeOffset))));
}
vec3 ACESFilm(vec3 x){
    return clamp((x*(2.51*x+.03))/(x*(2.43*x+.59)+.14),0.,1.);
}
vec3 render(vec2 uv,float timeOffset,bool isBackground,vec3 params,int iterations,int shadowSteps){
    float steps=params.x;
    float marchSize=params.y;
    vec3 ro=vec3(0.,0.,-4.);
    vec3 rd=normalize(vec3(uv,1.5));
    float t=0.;
    vec3 col=vec3(0.);
    float transmittance=1.;
    vec3 ld=normalize(vec3(1.,1.,-1.));
    for(int i=0;
    i<int(steps);
    i++){
        vec3 p=ro+rd*t;
        float d=map(p,timeOffset,iterations);
        float absD=abs(d);
        if(absD<.2){
            float density=exp(-absD*15.);
            float shadow=1.;
            for(int s=1;
            s<=shadowSteps;
            s++){
                shadow*=mix(1.,smoothstep(0.,.1,map(p+ld*float(s)*.1,timeOffset,iterations)),.5);
            }
            float stepAlpha=density*marchSize;
            col+=getColor(d,p,timeOffset)*density*shadow*transmittance*stepAlpha*50.;
            transmittance*=(1.-stepAlpha*2.);
        }
        if(transmittance<.01)break;
        t+=max(marchSize,absD*.5);
    }
    if(isBackground){
        col+=vec3(.02,.04,.08)*(1.-transmittance);
    }
    return col;
}
void mainImage(out vec4 fragColor,vec2 fragCoord){
    vec2 uv=(fragCoord-.5*iResolution.xy)/iResolution.y;
    vec3 config=vec3(35.,.08,3.);
    int iterations=5;
    int shadowSteps=3;
    vec3 finalCol=vec3(0.);
    float shutterSpeed=.08;
    float dither=fract(sin(dot(fragCoord,vec2(12.9898,78.233)))*43758.5453);
    for(int s=0;
    s<int(config.z);
    s++){
        float jitter=(float(s)+dither)/config.z;
        float offset=jitter*shutterSpeed;
        vec2 bgUv=uv+(dither-.5)*.01*shutterSpeed;
        finalCol+=render(bgUv,offset,true,config,iterations,shadowSteps);
    }
    finalCol/=config.z;
    finalCol=ACESFilm(finalCol*.8);
    finalCol=pow(finalCol,vec3(.4545));
    finalCol+=(dither-.5)*.005;
    fragColor=vec4(finalCol,1.);
}