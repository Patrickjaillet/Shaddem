// Fract 130
// Shadertoy ID: 73BXDd
// Description: f
// Tags: fractal, tunnel

void mainImage(out vec4 O, vec2 C)
{
    vec3 d = vec3((C - iResolution.xy * vec2(.5, .35)) / iResolution.y, 1),
         q = vec3(0, .7, 1.5), o = q * 0., p;
    float i = 0., e = 0., R = 0., s, pulse = sin(iTime * .035) * .05 + .95;
    for(; i++ < 28.;)
    {
        o += .004 / (.005 + abs(e)) * vec3(.1 + .1 * sin(iTime * .05 + R), .04, .15 * cos(R * .5));
        R = length(p = q += d * e * R * .18);
        p = vec3(log(R) - iTime * .075, acos(p.z / R) * 1.5, atan(p.y, p.x) + iTime * .0375);
        e = p.y - 1.2;
        s = 1.5;
        for(int j = 0; j < 12; j++)
        {
            p.xy = vec2(p.x - p.y, p.x + p.y) * .7;
            p = abs(p - vec3(0, .4, .2)) - vec3(.5, 0, 1);
            s *= 1.65 * pulse;
            e += dot(sin(p * s), cos(p.zxy * s)) / s * .45;
        }
    }
    O = vec4(vec3(dot(pow(o, vec3(1.2)), vec3(.2126, .7152, .0722))), 0);
}