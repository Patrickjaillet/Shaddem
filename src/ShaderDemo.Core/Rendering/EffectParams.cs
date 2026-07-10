// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;

namespace ShaderDemo.Core.Rendering;

public sealed class EffectParams
{
    public Vector4 Color = new(1.0f, 1.0f, 1.0f, 1.0f);
    public float Speed = 1.0f;
    public float Intensity = 1.0f;
    public float Shake = 0.0f;
    public float Strobe = 0.0f;
    public float Vignette = 0.0f;
    public float Noise = 0.0f;
    public float Scanlines = 0.0f;
    public float Fisheye = 0.0f;
    public float Pixelate = 1.0f;
    public float RgbSplit = 0.0f;
    public float Wave = 0.0f;
    public float Mirror = 0.0f;
    public float Rotation = 0.0f;
    public float RotationSpeed = 0.0f;
    public float Bloom = 0.0f;
    public float Glitch = 0.0f;
    public float Vortex = 0.0f;
    public float GlitchHard = 0.0f;
    public float MotionBlur = 0.0f;
    public float Datamosh = 0.0f;

    public bool ParticlesActive = false;
    public float ParticlesSize = 10.0f;
    public Vector4 ParticlesColor = new(1.0f, 1.0f, 1.0f, 0.8f);
    public int ParticlesCount = 2000;

    public float FeedbackOpacity = 0.0f;
    public float FeedbackScale = 1.0f;
    public float FeedbackRotation = 0.0f;

    public Vector3 TintColor = new(1.0f, 1.0f, 1.0f);
    public float TintIntensity = 0.0f;
    public float Posterize = 0.0f;
    public float ShaderParam1 = 8.0f;
    public float KickIntensity = 1.0f;

    public float Brightness = 1.0f;
    public float Contrast = 1.0f;
    public float Saturation = 1.0f;
    public float Scale = 1.0f;
    public float ChromaticAberration = 0.0f;
    public float Sepia = 0.0f;
    public float Invert = 0.0f;
    public float Solarize = 0.0f;
    public float Swirl = 0.0f;
    public float Mosaic = 0.0f;
    public float Vhs = 0.0f;
    public float HueShift = 0.0f;
    public float EdgeDetect = 0.0f;
    public float Crosshatch = 0.0f;
    public float Dither = 0.0f;
    public float Halftone = 0.0f;
    public float NightVision = 0.0f;
    public float Thermal = 0.0f;

    public float FrostedGlass = 0.0f;
    public float Emboss = 0.0f;
    public float Sharpen = 0.0f;
    public float Blur = 0.0f;
    public float ZoomBlur = 0.0f;
    public float RgbShiftVert = 0.0f;
    public float GlitchAnalog = 0.0f;
    public float Crt = 0.0f;
    public float Kaleidoscope = 0.0f;
    public float Polar = 0.0f;
    public float Ripple = 0.0f;
    public float Spiral = 0.0f;
    public float BlockNoise = 0.0f;
    public float ColorReduce = 0.0f;
    public float Gamma = 1.0f;
    public float Exposure = 0.0f;
    public float Vibrance = 0.0f;
    public float SobelNeon = 0.0f;
    public float DotMatrix = 0.0f;
    public float FilmGrain = 0.0f;

    public void CopyFrom(EffectParams other)
    {
        Color = other.Color;
        Speed = other.Speed;
        Intensity = other.Intensity;
        Shake = other.Shake;
        Strobe = other.Strobe;
        Vignette = other.Vignette;
        Noise = other.Noise;
        Scanlines = other.Scanlines;
        Fisheye = other.Fisheye;
        Pixelate = other.Pixelate;
        RgbSplit = other.RgbSplit;
        Wave = other.Wave;
        Mirror = other.Mirror;
        Rotation = other.Rotation;
        RotationSpeed = other.RotationSpeed;
        Bloom = other.Bloom;
        Glitch = other.Glitch;
        Vortex = other.Vortex;
        GlitchHard = other.GlitchHard;
        MotionBlur = other.MotionBlur;
        Datamosh = other.Datamosh;

        ParticlesActive = other.ParticlesActive;
        ParticlesSize = other.ParticlesSize;
        ParticlesColor = other.ParticlesColor;
        ParticlesCount = other.ParticlesCount;

        FeedbackOpacity = other.FeedbackOpacity;
        FeedbackScale = other.FeedbackScale;
        FeedbackRotation = other.FeedbackRotation;

        TintColor = other.TintColor;
        TintIntensity = other.TintIntensity;
        Posterize = other.Posterize;
        ShaderParam1 = other.ShaderParam1;
        KickIntensity = other.KickIntensity;

        Brightness = other.Brightness;
        Contrast = other.Contrast;
        Saturation = other.Saturation;
        Scale = other.Scale;
        ChromaticAberration = other.ChromaticAberration;
        Sepia = other.Sepia;
        Invert = other.Invert;
        Solarize = other.Solarize;
        Swirl = other.Swirl;
        Mosaic = other.Mosaic;
        Vhs = other.Vhs;
        HueShift = other.HueShift;
        EdgeDetect = other.EdgeDetect;
        Crosshatch = other.Crosshatch;
        Dither = other.Dither;
        Halftone = other.Halftone;
        NightVision = other.NightVision;
        Thermal = other.Thermal;

        FrostedGlass = other.FrostedGlass;
        Emboss = other.Emboss;
        Sharpen = other.Sharpen;
        Blur = other.Blur;
        ZoomBlur = other.ZoomBlur;
        RgbShiftVert = other.RgbShiftVert;
        GlitchAnalog = other.GlitchAnalog;
        Crt = other.Crt;
        Kaleidoscope = other.Kaleidoscope;
        Polar = other.Polar;
        Ripple = other.Ripple;
        Spiral = other.Spiral;
        BlockNoise = other.BlockNoise;
        ColorReduce = other.ColorReduce;
        Gamma = other.Gamma;
        Exposure = other.Exposure;
        Vibrance = other.Vibrance;
        SobelNeon = other.SobelNeon;
        DotMatrix = other.DotMatrix;
        FilmGrain = other.FilmGrain;
    }

    public void ClampToValidRanges()
    {
        Intensity = Math.Clamp(Intensity, 0.0f, 5.0f);
        Brightness = Math.Clamp(Brightness, 0.0f, 3.0f);
        Contrast = Math.Clamp(Contrast, 0.0f, 3.0f);
        Saturation = Math.Clamp(Saturation, 0.0f, 3.0f);
        Gamma = Math.Clamp(Gamma, 0.1f, 3.0f);
        Exposure = Math.Clamp(Exposure, -2.0f, 2.0f);
        Vibrance = Math.Clamp(Vibrance, -1.0f, 1.0f);
        Bloom = Math.Clamp(Bloom, 0.0f, 5.0f);
        Strobe = Math.Clamp(Strobe, 0.0f, 1.0f);
        KickIntensity = Math.Clamp(KickIntensity, 0.0f, 5.0f);

        TintIntensity = Math.Clamp(TintIntensity, 0.0f, 1.0f);
        Posterize = Math.Clamp(Posterize, 0.0f, 32.0f);
        Sepia = Math.Clamp(Sepia, 0.0f, 1.0f);
        Invert = Math.Clamp(Invert, 0.0f, 1.0f);
        Solarize = Math.Clamp(Solarize, 0.0f, 1.0f);
        ColorReduce = Math.Clamp(ColorReduce, 0.0f, 1.0f);
        HueShift = Math.Clamp(HueShift, 0.0f, 1.0f);
        Thermal = Math.Clamp(Thermal, 0.0f, 1.0f);
        NightVision = Math.Clamp(NightVision, 0.0f, 1.0f);

        Scale = Math.Clamp(Scale, 0.1f, 5.0f);
        Fisheye = Math.Clamp(Fisheye, -1.0f, 1.0f);
        Mirror = Math.Clamp(Mirror, 0.0f, 3.0f);
        Vortex = Math.Clamp(Vortex, -1.0f, 1.0f);
        Wave = Math.Clamp(Wave, 0.0f, 5.0f);
        Swirl = Math.Clamp(Swirl, -10.0f, 10.0f);
        Spiral = Math.Clamp(Spiral, -10.0f, 10.0f);
        Ripple = Math.Clamp(Ripple, 0.0f, 1.0f);
        Polar = Math.Clamp(Polar, 0.0f, 1.0f);
        Kaleidoscope = Math.Clamp(Kaleidoscope, 0.0f, 1.0f);
        Mosaic = Math.Clamp(Mosaic, 0.0f, 5.0f);
        Rotation = Math.Clamp(Rotation, 0.0f, 360.0f);
        RotationSpeed = Math.Clamp(RotationSpeed, -180.0f, 180.0f);

        Glitch = Math.Clamp(Glitch, 0.0f, 1.0f);
        GlitchHard = Math.Clamp(GlitchHard, 0.0f, 1.0f);
        GlitchAnalog = Math.Clamp(GlitchAnalog, 0.0f, 1.0f);
        RgbSplit = Math.Clamp(RgbSplit, 0.0f, 0.1f);
        RgbShiftVert = Math.Clamp(RgbShiftVert, 0.0f, 1.0f);
        ChromaticAberration = Math.Clamp(ChromaticAberration, 0.0f, 1.0f);
        Pixelate = Math.Clamp(Pixelate, 1.0f, 50.0f);
        Datamosh = Math.Clamp(Datamosh, 0.0f, 1.0f);
        BlockNoise = Math.Clamp(BlockNoise, 0.0f, 1.0f);
        Vhs = Math.Clamp(Vhs, 0.0f, 1.0f);
        Crt = Math.Clamp(Crt, 0.0f, 1.0f);
        DotMatrix = Math.Clamp(DotMatrix, 0.0f, 1.0f);
        Scanlines = Math.Clamp(Scanlines, 0.0f, 1.0f);
        Noise = Math.Clamp(Noise, 0.0f, 1.0f);
        Vignette = Math.Clamp(Vignette, 0.0f, 2.0f);

        FrostedGlass = Math.Clamp(FrostedGlass, 0.0f, 1.0f);
        Blur = Math.Clamp(Blur, 0.0f, 1.0f);
        ZoomBlur = Math.Clamp(ZoomBlur, 0.0f, 1.0f);
        Sharpen = Math.Clamp(Sharpen, 0.0f, 1.0f);
        Emboss = Math.Clamp(Emboss, 0.0f, 1.0f);
        EdgeDetect = Math.Clamp(EdgeDetect, 0.0f, 1.0f);
        SobelNeon = Math.Clamp(SobelNeon, 0.0f, 1.0f);
        Crosshatch = Math.Clamp(Crosshatch, 0.0f, 1.0f);
        Dither = Math.Clamp(Dither, 0.0f, 1.0f);
        Halftone = Math.Clamp(Halftone, 0.0f, 2.0f);
        FilmGrain = Math.Clamp(FilmGrain, 0.0f, 1.0f);

        Speed = Math.Clamp(Speed, 0.0f, 5.0f);
        Shake = Math.Clamp(Shake, 0.0f, 5.0f);
        MotionBlur = Math.Clamp(MotionBlur, 0.0f, 0.99f);

        FeedbackOpacity = Math.Clamp(FeedbackOpacity, 0.0f, 0.99f);
        FeedbackScale = Math.Clamp(FeedbackScale, 0.5f, 1.5f);
        FeedbackRotation = Math.Clamp(FeedbackRotation, -0.1f, 0.1f);

        ParticlesSize = Math.Clamp(ParticlesSize, 1.0f, 100.0f);
        ParticlesCount = Math.Clamp(ParticlesCount, 100, 10000);

        ShaderParam1 = Math.Clamp(ShaderParam1, 1.0f, 20.0f);
    }

    public void Apply(ShaderProgram program)
    {
        program.SetUniform("customColor", Color);
        program.SetUniform("customSpeed", Speed);
        program.SetUniform("customIntensity", Intensity);
        program.SetUniform("customStrobe", Strobe);
        program.SetUniform("customVignette", Vignette);
        program.SetUniform("customNoise", Noise);
        program.SetUniform("customScanlines", Scanlines);
        program.SetUniform("customFisheye", Fisheye);
        program.SetUniform("customPixelate", Pixelate);
        program.SetUniform("customRgbSplit", RgbSplit);
        program.SetUniform("customWave", Wave);
        program.SetUniform("customMirror", Mirror);
        program.SetUniform("customRotation", Rotation);
        program.SetUniform("customRotationSpeed", RotationSpeed);
        program.SetUniform("customBloom", Bloom);
        program.SetUniform("customGlitch", Glitch);
        program.SetUniform("customVortex", Vortex);
        program.SetUniform("customGlitchHard", GlitchHard);
        program.SetUniform("customTintIntensity", TintIntensity);
        program.SetUniform("customTintColor", TintColor);
        program.SetUniform("customPosterize", Posterize);
        program.SetUniform("customChromaticAberration", ChromaticAberration);
        program.SetUniform("customSepia", Sepia);
        program.SetUniform("customInvert", Invert);
        program.SetUniform("customSolarize", Solarize);
        program.SetUniform("customSwirl", Swirl);
        program.SetUniform("customMosaic", Mosaic);
        program.SetUniform("customVhs", Vhs);
        program.SetUniform("customHueShift", HueShift);
        program.SetUniform("customEdgeDetect", EdgeDetect);
        program.SetUniform("customCrosshatch", Crosshatch);
        program.SetUniform("customDither", Dither);
        program.SetUniform("customHalftone", Halftone);
        program.SetUniform("customNightVision", NightVision);
        program.SetUniform("customThermal", Thermal);
        program.SetUniform("customFrostedGlass", FrostedGlass);
        program.SetUniform("customEmboss", Emboss);
        program.SetUniform("customSharpen", Sharpen);
        program.SetUniform("customBlur", Blur);
        program.SetUniform("customZoomBlur", ZoomBlur);
        program.SetUniform("customRgbShiftVert", RgbShiftVert);
        program.SetUniform("customGlitchAnalog", GlitchAnalog);
        program.SetUniform("customCrt", Crt);
        program.SetUniform("customKaleidoscope", Kaleidoscope);
        program.SetUniform("customPolar", Polar);
        program.SetUniform("customRipple", Ripple);
        program.SetUniform("customSpiral", Spiral);
        program.SetUniform("customBlockNoise", BlockNoise);
        program.SetUniform("customColorReduce", ColorReduce);
        program.SetUniform("customGamma", Gamma);
        program.SetUniform("customExposure", Exposure);
        program.SetUniform("customVibrance", Vibrance);
        program.SetUniform("customSobelNeon", SobelNeon);
        program.SetUniform("customDotMatrix", DotMatrix);
        program.SetUniform("customFilmGrain", FilmGrain);
        program.SetUniform("customBrightness", Brightness);
        program.SetUniform("customContrast", Contrast);
        program.SetUniform("customSaturation", Saturation);
        program.SetUniform("customScale", Scale);
        program.SetUniform("customShaderParam1", ShaderParam1);
    }
}
