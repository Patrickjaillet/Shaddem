// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Gui;

public static class MacroControlsPanel
{
    private static float _intensityMacro = 0.0f;
    private static float _colorMoodMacro = 0.0f;

    public static void Draw(ShaderManager manager)
    {
        EffectParams e = manager.Effects;

        ImGui.TextColored(Theme.TextMuted, "A small set of combined controls. Switch to Advanced for every individual slider.");

        DrawIntensityOnly(manager);
        ImGui.TextColored(Theme.TextMuted, "Scales bloom, glitch, chromatic aberration, and vignette together.");

        if (ImGui.SliderFloat("Color Mood", ref _colorMoodMacro, -1.0f, 1.0f))
        {
            e.HueShift = Math.Clamp((_colorMoodMacro + 1.0f) * 0.5f, 0.0f, 1.0f);
            e.Saturation = 1.0f + (_colorMoodMacro * 0.5f);
            e.Contrast = 1.0f + (Math.Abs(_colorMoodMacro) * 0.3f);
        }

        ImGui.TextColored(Theme.TextMuted, "Drives hue shift, saturation, and contrast together (negative = cool/muted, positive = warm/vivid).");

        ImGui.Separator();
        UiUtils.SliderWithReset("Speed", ref e.Speed, 0.0f, 5.0f, 1.0f);
        UiUtils.SliderWithReset("Zoom / Scale", ref e.Scale, 0.1f, 5.0f, 1.0f);
        ImGui.ColorEdit4("Global Color", ref e.Color);
        UiUtils.SliderWithReset("Kick Pulse", ref e.KickIntensity, 0.0f, 5.0f, 1.0f);
    }

    public static void DrawIntensityOnly(ShaderManager manager)
    {
        EffectParams e = manager.Effects;
        if (ImGui.SliderFloat("Intensity", ref _intensityMacro, 0.0f, 2.0f))
        {
            e.Bloom = _intensityMacro * 0.8f;
            e.Glitch = _intensityMacro * 0.15f;
            e.ChromaticAberration = _intensityMacro * 0.2f;
            e.Vignette = _intensityMacro * 0.5f;
        }
    }
}
