// Etoile I
// Shadertoy ID: ffBGDt
// Description: Etoile I
// Tags: etoile

#define temps iTime
#define resolution iResolution

float rebond;

float sdBoite(vec3 p, vec3 b) {
  vec3 d = abs(p) - b;
  return min(max(d.x, max(d.y, d.z)), 0.) + length(max(d, 0.));
}

void pR(inout vec2 p, float a) {
  p = cos(a) * p + sin(a) * vec2(p.y, -p.x);
}

float bruit(vec3 p) {
  vec3 ip = floor(p);
  p -= ip; 
  vec3 s = vec3(7, 157, 113);
  vec4 h = vec4(0., s.yz, s.y + s.z) + dot(ip, s);
  p = p * p * (3. - 2. * p); 
  h = mix(fract(sin(h) * 43758.5), fract(sin(h + s.x) * 43758.5), p.x);
  h.xy = mix(h.xz, h.yw, p.y);
  return mix(h.x, h.y, p.z); 
}

float carte(vec3 p) {
  vec3 p2 = p;
  p2.z -= temps * 1.5;
  pR(p2.xy, p2.z * 0.1);
  float tunnel = 1.8 - length(p2.xy);
  float distorsion = 0.4 * bruit(8. * p2 + 3. * rebond);
  return (tunnel + distorsion) * 0.5;
}

vec3 calculerNormale(vec3 pos) {
  vec2 eps = vec2(0.001, 0.0);
  return normalize(vec3(
    carte(pos + eps.xyy) - carte(pos - eps.xyy),
    carte(pos + eps.yxy) - carte(pos - eps.yxy),
    carte(pos + eps.yyx) - carte(pos - eps.yyx)
  ));
}

float castRayx(vec3 ro, vec3 rd) {
  float signe_fonction = (carte(ro) < 0.) ? -1. : 1.;
  float precis = .0001;
  float h = precis * 2.;
  float t = 0.;
  for(int i = 0; i < 150; i++) {
    if(abs(h) < precis || t > 30.) break;
    h = signe_fonction * carte(ro + rd * t);
    t += h;
  }
  return t;
}

float refr(vec3 pos, vec3 lig, vec3 dir, vec3 nor, float angle, out float t2, out vec3 nor2) {
  float h = 0.;
  t2 = 2.;
  vec3 dir2 = refract(dir, nor, angle);  
  for(int i = 0; i < 60; i++) {
    if(abs(h) > 5.) break;
    h = carte(pos + dir2 * t2);
    t2 -= h;
  }
  nor2 = calculerNormale(pos + dir2 * t2);
  return(.5 * clamp(dot(-lig, nor2), 0., 1.) + pow(max(dot(reflect(dir2, nor2), lig), 0.), 12.));
}

float softshadow(vec3 ro, vec3 rd) {
  float sh = 1.;
  float t = .05;
  for(int i = 0; i < 24; i++) {
    float h = carte(ro + rd * t);
    sh = min(sh, 8. * h / t);
    t += clamp(h, 0.02, 0.5);
    if(sh < 0.01 || t > 15.) break;
  }
  return clamp(sh, 0., 1.);
}

void mainImage(out vec4 fragColor, in vec2 fragCoord) {   
  rebond = abs(fract(0.05 * temps) - .5) * 20.; 
    
  vec2 uv = (2.0 * fragCoord - resolution.xy) / resolution.y;
   
  // Secousses de la caméra et pulsation de matière associées
  float t_wobble = .1 * temps;
  float f_wobble = fract(t_wobble);
  float s_wobble = floor(t_wobble);
  float wobble_pulse = 0.0;
  float flare_intensity = 0.0;
  
  if(f_wobble >= 0.9) {
    wobble_pulse = 1.0;
    // Intensité de l'éjection lumineuse
    flare_intensity = exp(-4.0 * (1.0 - (f_wobble - 0.9) * 10.0));
  }
  
  float wobble = wobble_pulse * fract(-temps) * 0.1 * sin(30. * temps);

  vec3 dir = normalize(vec3(uv, 2.5));
  vec3 org = vec3(0.0, 2. * wobble, 0.0);  
    
  vec3 color = vec3(0.);
  vec3 color2 = vec3(0.);
  float t = castRayx(org, dir);
  vec3 pos = org + dir * t;
  vec3 nor = calculerNormale(pos);

  vec3 lig = normalize(vec3(.2, 6.0, .5));
  float depth = exp(-0.05 * t);
    
  if(t < 30.0) {
    float occ = clamp(carte(pos + nor * 0.1) * 10.0, 0.0, 1.0);
    float sha = softshadow(pos, lig);
    float dif = clamp(dot(nor, lig), 0.0, 1.0);
    float spe = pow(clamp(dot(reflect(dir, nor), lig), 0.0, 1.0), 32.0);
    
    color2 = (dif + spe) * sha * occ * vec3(1.1);
    
    float t2;
    vec3 nor2;
    color2 += refr(pos, lig, dir, nor, 0.85, t2, nor2) * depth * 0.6;
    color2 -= clamp(0.05 * t2, 0.0, 0.4);
  }      

  float T = 1.;
  
  // Pulsation de matière
  float b_intensity = 0.12 * -sin(.209 * temps + 1.) + 0.06;
  float intensity = mix(b_intensity, b_intensity * 3.5, flare_intensity); 
  
  vec3 pVol = org;
  
  for(int i = 0; i < 110; i++) {
    float nebula = bruit(pVol * 0.8 + rebond * 0.2);
    float density = intensity - carte(pVol) * nebula;
    if(density > 0.) {
      float step_t = density / 80.;
      T *= 1. - step_t * 18.;
      if(T <= 0.01) break;
    }
    pVol += dir * 0.14;
  }    
  
  // Couleur de base (feu/glace)
  vec3 basecol = mix(vec3(1.0, 0.4, 0.1), vec3(0.1, 0.4, 1.0), 0.5 + 0.5 * sin(temps * 0.2));
  T = clamp(T, 0.0, 1.0); 
  color += basecol * exp(3.5 * (0.5 - T) - 0.5);
  
  color2 *= depth;
  color2 += (1. - depth) * bruit(dir * 5.0 + temps * 0.1) * 0.15;
  
  // Effet de cœur et d'éjection lumineuse (Flares)
  float masqueCentre = pow(1.0 - clamp(length(uv * 0.6), 0., 1.), 3.0);
  float pulse = (0.5 + 0.5 * sin(temps * 3.0 + rebond));
  
  // Éjection lumineuse lors des secousses
  float flare_size = masqueCentre * (1.0 + 3.0 * flare_intensity);
  pulse = mix(pulse, pulse * 1.5 + 4.0 * flare_intensity * masqueCentre, flare_intensity);
  color += basecol * flare_size * pulse * depth * 1.2; 

  // Éjection directionnelle simple (rayons)
  float rayon = abs(atan(uv.y, uv.x));
  float flare_rayons = pow(abs(sin(rayon * 8.0 + temps * 2.0)), 10.0) * flare_intensity * masqueCentre;
  color += vec3(1.0, 0.8, 0.5) * flare_rayons * depth * 5.0;

  vec3 final = (color + color2 * 0.9) * 1.4;
  final = pow(final, vec3(0.4545));
  final *= 1.0 - dot(uv, uv) * 0.2;
  
  fragColor = vec4(final, 1.0);
}