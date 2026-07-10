// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using ImGuiNET;

namespace ShaderDemo.Core.Gui;

public static class SplashScreen
{
    public static void Draw()
    {
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);

        ImGuiWindowFlags flags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings
            | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoBackground;

        if (ImGui.Begin("##splash", flags))
        {
            ImDrawListPtr dl = ImGui.GetWindowDrawList();
            dl.AddRectFilled(viewport.WorkPos, viewport.WorkPos + viewport.WorkSize, ImGui.GetColorU32(Theme.SurfaceBase with { W = 1.0f }));

            Vector2 center = viewport.WorkPos + viewport.WorkSize * 0.5f;

            bool pushedTitle = Theme.PushFontIf(Theme.FontTitle);
            Vector2 titleSize = ImGui.CalcTextSize("ShaderDemo");
            ImGui.SetCursorScreenPos(center - titleSize * 0.5f - new Vector2(0, 24));
            ImGui.TextColored(Theme.Accent, "ShaderDemo");
            Theme.PopFontIf(pushedTitle);

            const string loadingText = "Loading shaders...";
            Vector2 loadingSize = ImGui.CalcTextSize(loadingText);
            ImGui.SetCursorScreenPos(center - loadingSize * 0.5f + new Vector2(0, 16));
            ImGui.TextColored(Theme.TextMuted, loadingText);

            float spinnerRadius = 10.0f;
            Vector2 spinnerCenter = center + new Vector2(0, 50);
            float t = (float)ImGui.GetTime() * 3.0f;
            for (int i = 0; i < 8; i++)
            {
                float angle = t + i * (MathF.PI * 2.0f / 8.0f);
                float fade = (float)i / 8.0f;
                Vector2 dot = spinnerCenter + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * spinnerRadius;
                dl.AddCircleFilled(dot, 2.5f, ImGui.GetColorU32(Theme.Accent with { W = 0.3f + 0.7f * fade }), 8);
            }
        }

        ImGui.End();
    }
}
