// Strange univers
// Shadertoy ID: 73j3Dy
// Description: Strange univers
// Tags: fractal

mat3 r3d(float a,vec3 axis){
    vec3 n=normalize(axis);float s=sin(a),c=cos(a),r=1.-c;
    return mat3(n.x*n.x*r+c,n.y*n.x*r+n.z*s,n.z*n.x*r-n.y*s,n.x*n.y*r-n.z*s,n.y*n.y*r+c,n.z*n.y*r+n.x*s,n.x*n.z*r+n.y*s,n.y*n.z*r-n.x*s,n.z*n.z*r+c);
}
void mainImage(out vec4 fragColor,in vec2 fragCoord){
    vec2 uv=(fragCoord-.5*iResolution.xy)/iResolution.y;
    float tk=iTime*.05;
    vec3 ro=vec3(0.,0.,tk*3.5);
    vec3 rd=normalize(vec3(uv,2.2));
    float dO=0.;
    vec3 glow=vec3(0.);
    mat3 m1=r3d(tk*.4,vec3(.3,1.,.2)),m2=r3d(tk*.25,vec3(-.5,.2,1.));
    vec3 col=mix(vec3(.001,.0015,.003),vec3(.004,.002,.008),length(uv));
    bool hit=false;vec3 hitP,hitQ;float hitS=1.;
    for(int i=0;i<240;i++){
        vec3 p=ro+rd*dO;
        p.z=mod(p.z+3.,6.)-3.;
        p=p*m1;
        vec3 q=p;float s=1.;
        for(int j=0;j<8;j++){
            q=abs(q*m2)-vec3(.55,.75,.6);
            float r2=dot(q,q);
            float k=1.45/clamp(r2,.015,.85);
            q*=k;s*=k;
        }
        float d=(length(q.xy)-.035)/s;
        glow+=exp(-40.*abs(d))*(vec3(.7,.85,1.)+vec3(sin(p.z*2.)*.3,cos(p.x*1.5)*.3,sin(p.y)*.2));
        if(d<.0002){hit=true;hitP=p;hitQ=q;hitS=s;break;}
        dO+=d*.45;
        if(dO>25.)break;
    }
    if(hit){
        vec2 eps=vec2(.0001,0.);
        vec3 n=normalize(vec3(
            (length(abs((hitP+eps.xyy)*m2)-vec3(.55,.75,.6))-length(abs((hitP-eps.xyy)*m2)-vec3(.55,.75,.6))),
            (length(abs((hitP+eps.yxy)*m2)-vec3(.55,.75,.6))-length(abs((hitP-eps.yxy)*m2)-vec3(.55,.75,.6))),
            (length(abs((hitP+eps.yyx)*m2)-vec3(.55,.75,.6))-length(abs((hitP-eps.yyx)*m2)-vec3(.55,.75,.6)))
        ));
        vec3 ld=normalize(vec3(.6,.8,-.5));
        float diff=max(dot(n,ld),0.);
        float spec=pow(max(dot(reflect(rd*m1,n),ld),0.),96.);
        float ao=clamp(2.5/(1.+hitS*.15),0.,1.);
        vec3 h=fract(tk*.1+hitP.z*.05+vec3(0.,.333,.666));
        vec3 base=clamp(abs(h*6.-3.)-1.,0.,1.);
        col=mix(base*diff,vec3(spec),.65)*ao;
        col+=vec3(.3,.6,1.)*pow(1.-max(dot(-rd*m1,n),0.),5.)*ao;
        col=mix(col,vec3(0.),clamp(dO/25.,0.,1.));
    }
    col+=glow*.0035;
    col=mix(col,vec3(dot(col,vec3(.2126,.7152,.0722))),-.15);
    vec2 scanline=sin(fragCoord.yy*1.5)*.04;
    col-=scanline.xyx*clamp(1.-dO*.05,0.,1.);
    vec2 uvn=fragCoord/iResolution.xy;
    col*=pow(16.*uvn.x*uvn.y*(1.-uvn.x)*(1.-uvn.y),.25);
    fragColor=vec4(pow(1.-exp(-2.4*col),vec3(1./2.2)),1.);
}