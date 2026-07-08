// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using System.Numerics;

namespace ShaderDemo.Core.Gui;

public static class Theme
{
    public static void Apply()
    {
        ImGuiStylePtr style = ImGui.GetStyle();
        var colors = style.Colors;

        Vector4 bg = new(0.12f, 0.12f, 0.14f, 0.95f);
        Vector4 text = new(0.95f, 0.96f, 0.98f, 1.0f);
        Vector4 textDisabled = new(0.5f, 0.5f, 0.5f, 1.0f);

        Vector4 accent = new(0.0f, 0.48f, 0.8f, 1.0f);
        Vector4 accentHover = new(0.1f, 0.58f, 0.9f, 1.0f);
        Vector4 accentActive = new(0.0f, 0.38f, 0.7f, 1.0f);

        Vector4 area = new(0.08f, 0.08f, 0.1f, 1.0f);
        Vector4 border = new(0.25f, 0.25f, 0.28f, 0.5f);

        colors[(int)ImGuiCol.Text] = text;
        colors[(int)ImGuiCol.TextDisabled] = textDisabled;
        colors[(int)ImGuiCol.WindowBg] = bg;
        colors[(int)ImGuiCol.ChildBg] = area;
        colors[(int)ImGuiCol.PopupBg] = new Vector4(0.15f, 0.15f, 0.17f, 0.95f);
        colors[(int)ImGuiCol.Border] = border;
        colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

        colors[(int)ImGuiCol.FrameBg] = area;
        colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.2f, 0.2f, 0.25f, 1.0f);
        colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.25f, 0.25f, 0.3f, 1.0f);

        colors[(int)ImGuiCol.TitleBg] = new Vector4(0.1f, 0.1f, 0.12f, 1.0f);
        colors[(int)ImGuiCol.TitleBgActive] = accentHover;
        colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.1f, 0.1f, 0.12f, 0.5f);

        colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.15f, 0.15f, 0.17f, 1.0f);

        colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.08f, 0.08f, 0.1f, 1.0f);
        colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.3f, 0.3f, 0.35f, 1.0f);
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.4f, 0.4f, 0.45f, 1.0f);
        colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.5f, 0.5f, 0.55f, 1.0f);

        colors[(int)ImGuiCol.CheckMark] = accent;

        colors[(int)ImGuiCol.SliderGrab] = accent;
        colors[(int)ImGuiCol.SliderGrabActive] = accentActive;

        colors[(int)ImGuiCol.Button] = new Vector4(0.2f, 0.2f, 0.25f, 1.0f);
        colors[(int)ImGuiCol.ButtonHovered] = accentHover;
        colors[(int)ImGuiCol.ButtonActive] = accentActive;

        colors[(int)ImGuiCol.Header] = new Vector4(0.2f, 0.2f, 0.25f, 1.0f);
        colors[(int)ImGuiCol.HeaderHovered] = accentHover;
        colors[(int)ImGuiCol.HeaderActive] = accentActive;

        colors[(int)ImGuiCol.Separator] = border;
        colors[(int)ImGuiCol.SeparatorHovered] = accentHover;
        colors[(int)ImGuiCol.SeparatorActive] = accentActive;

        colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.3f, 0.3f, 0.35f, 1.0f);
        colors[(int)ImGuiCol.ResizeGripHovered] = accentHover;
        colors[(int)ImGuiCol.ResizeGripActive] = accentActive;

        colors[(int)ImGuiCol.Tab] = new Vector4(0.18f, 0.18f, 0.2f, 1.0f);
        colors[(int)ImGuiCol.TabHovered] = accentHover;
        colors[(int)ImGuiCol.TabActive] = accent;
        colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.1f, 0.1f, 0.12f, 1.0f);
        colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.2f, 0.2f, 0.25f, 1.0f);

        colors[(int)ImGuiCol.PlotLines] = text;
        colors[(int)ImGuiCol.PlotLinesHovered] = accentHover;
        colors[(int)ImGuiCol.PlotHistogram] = accent;
        colors[(int)ImGuiCol.PlotHistogramHovered] = accentHover;

        colors[(int)ImGuiCol.TextSelectedBg] = accent;
        colors[(int)ImGuiCol.DragDropTarget] = new Vector4(1.0f, 1.0f, 0.0f, 0.9f);

        style.WindowRounding = 5.0f;
        style.ChildRounding = 5.0f;
        style.FrameRounding = 4.0f;
        style.GrabRounding = 4.0f;
        style.PopupRounding = 5.0f;
        style.ScrollbarRounding = 9.0f;
        style.TabRounding = 4.0f;
    }
}
