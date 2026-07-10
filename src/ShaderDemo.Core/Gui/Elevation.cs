// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using ImGuiNET;

namespace ShaderDemo.Core.Gui;

public static class Elevation
{
    public static void DrawShadow(Vector2 windowPos, Vector2 windowSize, float rounding = 6.0f, int layers = 5)
    {
        ImDrawListPtr dl = ImGui.GetBackgroundDrawList();

        for (int i = layers; i >= 1; i--)
        {
            float spread = i * 2.5f;
            float alpha = 0.05f * (1.0f - (float)i / (layers + 1));
            Vector2 min = windowPos - new Vector2(spread, spread) + new Vector2(spread * 0.4f, spread * 0.6f);
            Vector2 max = windowPos + windowSize + new Vector2(spread, spread) + new Vector2(spread * 0.4f, spread * 0.6f);
            uint color = ImGui.GetColorU32(new Vector4(0.0f, 0.0f, 0.0f, alpha));
            dl.AddRectFilled(min, max, color, rounding + spread * 0.5f);
        }
    }

    public static void PushDockedBorder()
    {
        ImGui.PushStyleColor(ImGuiCol.Border, Theme.Border with { W = 0.9f });
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.0f);
    }

    public static void PopDockedBorder()
    {
        ImGui.PopStyleVar();
        ImGui.PopStyleColor();
    }

    public static unsafe void SuppressDockMenuButton()
    {
        var windowClass = new ImGuiWindowClass
        {
            DockNodeFlagsOverrideSet = (ImGuiDockNodeFlags)(1 << 14),
        };

        ImGui.SetNextWindowClass(new ImGuiWindowClassPtr(&windowClass));
    }

    private static ImDrawListPtr _cardDrawList;
    private static Vector2 _cardStart;

    public static void BeginCard()
    {
        _cardDrawList = ImGui.GetWindowDrawList();
        _cardDrawList.ChannelsSplit(2);
        _cardDrawList.ChannelsSetCurrent(1);
        _cardStart = ImGui.GetCursorScreenPos();
        ImGui.Indent(Theme.SpaceSm);
        ImGui.Dummy(new Vector2(0, Theme.SpaceXs));
    }

    public static void EndCard()
    {
        ImGui.Dummy(new Vector2(0, Theme.SpaceXs));
        ImGui.Unindent(Theme.SpaceSm);

        Vector2 end = ImGui.GetCursorScreenPos();
        Vector2 min = _cardStart - new Vector2(Theme.SpaceSm, Theme.SpaceXs);
        Vector2 max = new(ImGui.GetWindowPos().X + ImGui.GetWindowSize().X - Theme.SpaceMd, end.Y);

        _cardDrawList.ChannelsSetCurrent(0);
        _cardDrawList.AddRectFilled(min, max, ImGui.GetColorU32(Theme.SurfaceElevated with { W = 0.7f }), 6.0f);
        _cardDrawList.AddRect(min, max, ImGui.GetColorU32(Theme.Border), 6.0f);
        _cardDrawList.ChannelsMerge();

        ImGui.Spacing();
    }
}
