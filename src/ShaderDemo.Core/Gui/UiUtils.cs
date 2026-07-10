// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using ImGuiNET;

namespace ShaderDemo.Core.Gui;

public static class UiUtils
{
    public static bool SliderWithReset(string label, ref float val, float min, float max, float defaultValue)
    {
        bool changed = ImGui.SliderFloat(label, ref val, min, max);
        Vector2 sliderMin = ImGui.GetItemRectMin();
        Vector2 sliderMax = ImGui.GetItemRectMax();

        ImGui.SameLine();
        if (ImGui.Button($"R##{label}"))
        {
            val = defaultValue;
            changed = true;
        }

        DrawDeltaStrip(sliderMin, sliderMax, min, max, val, defaultValue);

        return changed;
    }

    private static void DrawDeltaStrip(Vector2 sliderMin, Vector2 sliderMax, float min, float max, float val, float defaultValue)
    {
        if (max <= min) return;

        float range = max - min;
        float valueT = Math.Clamp((val - min) / range, 0.0f, 1.0f);
        float defaultT = Math.Clamp((defaultValue - min) / range, 0.0f, 1.0f);

        const float stripHeight = 3.0f;
        Vector2 stripMin = new(sliderMin.X, sliderMax.Y + 2.0f);
        Vector2 stripMax = new(sliderMax.X, sliderMax.Y + 2.0f + stripHeight);

        ImDrawListPtr dl = ImGui.GetWindowDrawList();
        dl.AddRectFilled(stripMin, stripMax, ImGui.GetColorU32(Theme.SurfaceElevated), 1.5f);

        bool strayed = MathF.Abs(valueT - defaultT) > 0.001f;
        if (strayed)
        {
            float fillFromX = stripMin.X + MathF.Min(valueT, defaultT) * (stripMax.X - stripMin.X);
            float fillToX = stripMin.X + MathF.Max(valueT, defaultT) * (stripMax.X - stripMin.X);
            dl.AddRectFilled(new Vector2(fillFromX, stripMin.Y), new Vector2(fillToX, stripMax.Y), ImGui.GetColorU32(Theme.Accent with { W = 0.6f }));
        }

        float defaultX = stripMin.X + defaultT * (stripMax.X - stripMin.X);
        dl.AddLine(new Vector2(defaultX, stripMin.Y - 1.0f), new Vector2(defaultX, stripMax.Y + 1.0f), ImGui.GetColorU32(Theme.TextMuted), 1.5f);

        ImGui.Dummy(new Vector2(0, stripHeight + 3.0f));
    }
}
