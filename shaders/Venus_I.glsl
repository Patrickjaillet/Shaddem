// Venus I
// Shadertoy ID: NffSR2
// Description: Venus
// Tags: fbm, perlin, planet, venus, gaz

#define OCTAVES_VENUS 8
#define PERSISTANCE 0.5
#define LACUNARITE 2.2
#define TAU 6.28318
#define PI 3.141592
#define ANIMER
#define OCTAVES 5

vec3 mod289(vec3 x){return x-floor(x*(1./289.))*289.;}
vec2 mod289(vec2 x){return x-floor(x*(1./289.))*289.;}
vec3 permuter(vec3 x){return mod289(((x*34.)+1.)*x);}

float bruitSimplex(vec2 v){
    const vec4 C=vec4(.211324865405187,.366025403784439,-.577350269189626,.024390243902439);
    vec2 i=floor(v+dot(v,C.yy)),x0=v-i+dot(i,C.xx),i1=(x0.x>x0.y)?vec2(1,0):vec2(0,1);
    vec4 x12=x0.xyxy+C.xxzz;x12.xy-=i1;i=mod289(i);
    vec3 p=permuter(permuter(i.y+vec3(0,i1.y,1))+i.x+vec3(0,i1.x,1)),m=max(.5-vec3(dot(x0,x0),dot(x12.xy,x12.xy),dot(x12.zw,x12.zw)),0.);
    m=m*m;m=m*m;vec3 x=2.*fract(p*C.www)-1.,h=abs(x)-.5,ox=floor(x+.5),a0=x-ox;
    m*=1.79284291400159-.85373472095314*(a0*a0+h*h);
    vec3 g;g.x=a0.x*x0.x+h.x*x0.y;g.yz=a0.yz*x12.xz+h.yz*x12.yw;
    return 130.*dot(m,g);
}
//======================================================================================//
//  >>  Author  : Patrick JAILLET                                                       //
//  >>  Email   : metashader@proton.me                                                  //
//  >>  URL     : https://lside.xo.je                                                   //
//*====================================================================================*//
vec3 hsvEnRgb(vec3 c){
    vec4 K=vec4(1.,2./3.,1./3.,3.);
    vec3 p=abs(fract(c.xxx+K.xyz)*6.-K.www);
    return c.z*mix(K.xxx,clamp(p-K.xxx,0.,1.),c.y);
}

float bruitFractal(vec2 coord,float p,float l){
    float n=0.,f=1.,a=1.;
    for(int i=0;i<OCTAVES;++i){n+=a*bruitSimplex(coord*f);a*=p;f*=l;}
    return n;
}

vec3 nebuleuseFractale(vec2 coord,vec3 col,float tr){return bruitFractal(coord,.5,2.)*col*tr;}
float hachage12(vec2 p){vec3 p3=fract(vec3(p.xyx)*.1031);p3+=dot(p3,p3.yzx+33.33);return fract((p3.x+p3.y)*p3.z);}

float bruitPerlin(vec2 p){
    vec2 i=floor(p),f=fract(p),u=f*f*f*(f*(f*6.-15.)+10.);
    float a=hachage12(i),b=hachage12(i+vec2(1,0)),c=hachage12(i+vec2(0,1)),d=hachage12(i+1.);
    return mix(a,b,u.x)+(c-a)*u.y*(1.-u.x)+(d-b)*u.x*u.y;
}

float fbm_venus(vec2 p){
    float v=0.,a=PERSISTANCE,f=1.;
    for(int i=0;i<OCTAVES_VENUS;i++){v+=a*bruitPerlin(p*f);f*=LACUNARITE;a*=PERSISTANCE;}
    return v;
}

float motif_venus(vec2 p,out vec2 q,out vec2 r){
    q=vec2(fbm_venus(p),fbm_venus(p+vec2(5.2,1.3)));
    r=vec2(fbm_venus(p+4.*q+vec2(1.7,9.2)+.15*iTime),fbm_venus(p+4.*q+vec2(8.3,2.8)+.126*iTime));
    return fbm_venus(p+4.*r);
}

float hachage21(vec2 p){p=fract(p*vec2(123.34,456.21));p+=dot(p,p+45.32);return fract(p.x*p.y);}

vec3 renduEclair(vec2 uv,vec2 s,float t,float graine){
    vec2 p=uv-s;float b=0.,g=0.,puls=sin(t*100.*hachage21(vec2(graine)))*.5+.5,f=smoothstep(1.,0.,fract(t*5.));
    for(float i=1.;i<4.;i++){
        float l=abs(.003/(p.x+bruitSimplex(uv*(i*6.)+t*.2)*(.1/i)+p.y*.2));
        b+=l;g+=l*20.*(.05/(.05+dot(p,p)));
    }
    return vec3(.5,.7,1)*(b+g)*puls*f;
}

vec3 dessinerFilaments(vec2 uv,float t){
    float seauTps=floor(t*6.);if(hachage21(vec2(seauTps,13.4))<.8)return vec3(0);
    vec2 impact=vec2(hachage21(vec2(seauTps)),hachage21(vec2(seauTps,4.2)))*4.-2.;
    vec3 c=renduEclair(uv,impact,t,seauTps);
    for(int j=0;j<3;j++){
        float fj=float(j);vec2 s=vec2(seauTps,fj);
        c+=renduEclair(uv*1.5,impact+vec2(hachage21(s),hachage21(s+.5))*.4-.2,t,seauTps+fj)*.4;
    }
    return c;
}

void mainImage(out vec4 O,vec2 C){
    vec2 uv=(C-.5*iResolution.xy)/iResolution.y,cn=C/max(iResolution.x,iResolution.y);
    float a=iTime*.3;vec3 ro=vec3(1.2*sin(a),.2*cos(a*.5),1.2*cos(a)),fw=normalize(-ro),ri=normalize(cross(vec3(0,1,0),fw)),rd=normalize(fw+uv.x*ri+uv.y*cross(fw,ri)),bg=vec3(0);
    
    bg+=nebuleuseFractale(cn+vec2(.1+iTime*0.01, .1)+bruitSimplex(cn*2.+iTime*.05)*.02,hsvEnRgb(vec3(.5+.5*sin(iTime*.1),.5,.25)),1.);
    bg+=nebuleuseFractale(cn+vec2(0,.2-iTime*0.015)+bruitSimplex(cn*3.+iTime*.08+10.)*.015,hsvEnRgb(vec3(.5+.5*sin(iTime*.21),1,.25)),.5);

    float b=dot(ro,rd),h=b*b-(dot(ro,ro)-.2025);
    if(h<0.)O.rgb=bg;else{
        vec3 p=ro+rd*(-b-sqrt(h)),n=normalize(p);
        vec2 sU=vec2(atan(n.z,n.x)+iTime*.05,asin(n.y))*2.,q,r;
        float f=motif_venus(sU,q,r);
        vec3 vC=mix(mix(vec3(.8,.5,.2),vec3(.4,.2,.1),clamp(f*f*4.,0.,1.)),vec3(.9,.7,.4),clamp(length(q),0.,1.)),fi=dessinerFilaments(sU,iTime);
        vC=(vC+fi*3.)*(max(dot(n,normalize(vec3(1,.5,1))),0.)+.05)+pow(1.-max(dot(n,-rd),0.),4.)*vec3(.9,.6,.3)*.4;
        O.rgb=mix(bg,vC,smoothstep(0.,.01,h))+fi*.6;
    }
    vec2 v=C/iResolution.xy;O.rgb*=pow(v.x*v.y*(1.-v.x)*(1.-v.y)*15.,.15);
    O=vec4(pow(O.rgb,vec3(.4545)),1);
}