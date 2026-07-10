// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Settings;

public static class TemplatesService
{
    public static readonly IReadOnlyList<DemoTemplate> BuiltIn = new List<DemoTemplate>
    {
        new()
        {
            Name = "Cyberpunk City Loop",
            Description = "Neon city tunnel with scanlines, RGB split, and a magenta/cyan grade.",
            ShaderName = "cyberpunk_city.glsl",
            Configure = e =>
            {
                e.Bloom = 0.6f;
                e.Scanlines = 0.25f;
                e.RgbSplit = 0.015f;
                e.Vignette = 0.5f;
                e.HueShift = 0.85f;
                e.Saturation = 1.2f;
            },
        },
        new()
        {
            Name = "Ambient Particles",
            Description = "Slow drifting particle field over a soft nebula backdrop.",
            ShaderName = "DeepSpace_Nebula.glsl",
            Configure = e =>
            {
                e.Speed = 0.4f;
                e.Bloom = 0.5f;
                e.Vignette = 0.4f;
                e.FeedbackOpacity = 0.15f;
                e.FeedbackScale = 1.01f;
            },
            EnableParticles = true,
        },
        new()
        {
            Name = "Music Visualizer",
            Description = "Kick-reactive plasma tuned to pulse hard with bass — load a track and press play.",
            ShaderName = "plasma_wave.glsl",
            Configure = e =>
            {
                e.KickIntensity = 2.2f;
                e.Bloom = 0.7f;
                e.ChromaticAberration = 0.1f;
            },
        },
        new()
        {
            Name = "Retro VHS",
            Description = "Warped scanlines, tape noise, and a slight rainbow split, straight off a rental tape.",
            ShaderName = "matrix_rain.glsl",
            Configure = e =>
            {
                e.Vhs = 0.4f;
                e.Scanlines = 0.5f;
                e.RgbSplit = 0.02f;
                e.Noise = 0.1f;
                e.FilmGrain = 0.15f;
            },
        },
        new()
        {
            Name = "Liquid Chrome",
            Description = "Molten reflective flow with heavy bloom and a slow feedback trail.",
            ShaderName = "LiquidChrome_Flow.glsl",
            Configure = e =>
            {
                e.Bloom = 0.9f;
                e.FeedbackOpacity = 0.2f;
                e.Speed = 0.7f;
            },
        },
        new()
        {
            Name = "Kaleidoscope Trip",
            Description = "Mirrored, hue-cycling kaleidoscope for a hypnotic, trippy loop.",
            ShaderName = "kaleidoscope_shader.glsl",
            Configure = e =>
            {
                e.Kaleidoscope = 0.6f;
                e.HueShift = 0.5f;
                e.Wave = 0.8f;
                e.Speed = 0.8f;
            },
        },
        new()
        {
            Name = "Deep Space Drift",
            Description = "Twinkling starfield with slow rotation, calm and minimal.",
            ShaderName = "TwinklingStarfield.glsl",
            Configure = e =>
            {
                e.Speed = 0.3f;
                e.RotationSpeed = 2.0f;
                e.Bloom = 0.3f;
                e.Vignette = 0.6f;
            },
        },
        new()
        {
            Name = "Fire & Embers",
            Description = "Warm particle fire with a heavy orange grade and gentle shake.",
            ShaderName = "fire_particles.glsl",
            Configure = e =>
            {
                e.Color = new Vector4(1.0f, 0.55f, 0.2f, 1.0f);
                e.TintIntensity = 0.2f;
                e.TintColor = new Vector3(1.0f, 0.6f, 0.3f);
                e.Shake = 0.3f;
                e.Bloom = 0.5f;
            },
        },
        new()
        {
            Name = "Black Hole",
            Description = "Gravitational lensing centerpiece with a tight vignette and cool grade.",
            ShaderName = "black_hole.glsl",
            Configure = e =>
            {
                e.Vignette = 0.9f;
                e.Contrast = 1.2f;
                e.Saturation = 0.8f;
                e.Bloom = 0.4f;
            },
        },
        new()
        {
            Name = "Glitch Art",
            Description = "Datamoshed, block-noise heavy digital-decay look for a harsher, artier loop.",
            ShaderName = "art.glsl",
            Configure = e =>
            {
                e.Glitch = 0.2f;
                e.BlockNoise = 0.15f;
                e.RgbSplit = 0.02f;
                e.EdgeDetect = 0.3f;
            },
        },
        new()
        {
            Name = "Fractal Dive",
            Description = "Raymarched mandelbulb with a slow rotate-in and soft rim light.",
            ShaderName = "mandelbulb.glsl",
            Configure = e =>
            {
                e.Speed = 0.5f;
                e.Bloom = 0.4f;
                e.RotationSpeed = 3.0f;
            },
        },
        new()
        {
            Name = "Sea Glass Calm",
            Description = "Gentle underwater refraction, cool tones, minimal motion — a calm loop.",
            ShaderName = "sea_glass.glsl",
            Configure = e =>
            {
                e.Speed = 0.4f;
                e.Saturation = 0.9f;
                e.Vignette = 0.3f;
            },
        },
        new()
        {
            Name = "Image Layer Showcase",
            Description = "Shader background composited with a blended image layer — demonstrates the megademo image-layer pipeline end-to-end.",
            ShaderName = "Cosmic_Space.glsl",
            Configure = e =>
            {
                e.Bloom = 0.4f;
                e.Vignette = 0.3f;
            },
            Layers = () =>
            {
                Layer imageLayer = Layer.CreateImage(Path.Combine(AppContext.BaseDirectory, "assets", "screenshot-workspace.png"), BlendMode.Screen, 0.5f);
                imageLayer.FitMode = ImageFitMode.Fit;
                imageLayer.Scale = 0.6f;
                imageLayer.PositionY = -0.25f;
                return new List<Layer> { imageLayer };
            },
        },
    }.AsReadOnly();

    public static bool Apply(DemoTemplate template, ShaderManager manager)
    {
        int index = -1;
        for (int i = 0; i < manager.ShaderNames.Count; i++)
        {
            if (manager.ShaderNames[i] == template.ShaderName) { index = i; break; }
        }

        if (index < 0) return false;

        manager.SelectShader(index);
        manager.Effects.CopyFrom(new EffectParams());
        template.Configure(manager.Effects);
        manager.Effects.ParticlesActive = template.EnableParticles;
        manager.Effects.ClampToValidRanges();

        manager.Layers.RemoveAll(l => !l.IsTimelineManaged);
        if (template.Layers != null)
        {
            manager.Layers.AddRange(template.Layers());
        }

        return true;
    }

    public static string Randomize(ShaderManager manager, AppSettings settings, Random random)
    {
        if (manager.ShaderNames.Count > 0)
        {
            manager.SelectShader(random.Next(manager.ShaderNames.Count));
        }

        string theme = PresetsService.GenerateRandomPreset(manager.Effects, random);

        if (!string.IsNullOrEmpty(settings.MusicFile) && File.Exists(settings.MusicFile))
        {
            settings.AudioReactive = true;
            manager.Audio.Enabled = true;
        }

        return theme;
    }
}
