// Copyright (c) 2026 Patrick JAILLET
namespace ShaderDemo.Core.Rendering;

public static class Snippets
{
    public static readonly IReadOnlyDictionary<string, string> Library = new Dictionary<string, string>
    {
        ["Palette (IQ)"] = """
            vec3 palette( float t ) {
                vec3 a = vec3(0.5, 0.5, 0.5);
                vec3 b = vec3(0.5, 0.5, 0.5);
                vec3 c = vec3(1.0, 1.0, 1.0);
                vec3 d = vec3(0.263,0.416,0.557);
                return a + b*cos( 6.28318*(c*t+d) );
            }
            """,
        ["Rotation 2D"] = """
            mat2 rotate2d(float angle){
                return mat2(cos(angle),-sin(angle),
                            sin(angle),cos(angle));
            }
            """,
        ["Noise 2D (Simple)"] = """
            float random (vec2 st) {
                return fract(sin(dot(st.xy,
                                     vec2(12.9898,78.233)))
                             * 43758.5453123);
            }
            """,
        ["Raymarching Loop"] = """
            for(int i=0; i<100; i++) {
                // ro = ray origin, rd = ray direction, t = distance
                vec3 p = ro + rd * t;
                float d = map(p); // map() is your distance function
                if(d < 0.001) { break; } // Hit
                t += d;
                if(t > 20.0) { break; } // Miss
            }
            """,
    };
}
