// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using ImGuiNET;

namespace ShaderDemo.Core.Gui;

public enum Icon
{
    Play,
    Pause,
    Stop,
    Record,
    Add,
    Delete,
    Save,
    Load,
    Export,
    Search,
    Close,
    Warning,
    Success,
    Info,
    Settings,
    DragHandle,
    Duplicate,
    Screenshot,
    Up,
    Down,
}

public static class Icons
{
    public static void Draw(ImDrawListPtr dl, Icon icon, Vector2 center, float size, uint color)
    {
        float r = size * 0.5f;
        float thickness = MathF.Max(1.2f, size * 0.09f);

        switch (icon)
        {
            case Icon.Play:
                dl.AddTriangleFilled(
                    center + new Vector2(-r * 0.5f, -r * 0.75f),
                    center + new Vector2(-r * 0.5f, r * 0.75f),
                    center + new Vector2(r * 0.75f, 0),
                    color);
                break;

            case Icon.Pause:
                dl.AddRectFilled(center + new Vector2(-r * 0.6f, -r * 0.7f), center + new Vector2(-r * 0.15f, r * 0.7f), color, 1.0f);
                dl.AddRectFilled(center + new Vector2(r * 0.15f, -r * 0.7f), center + new Vector2(r * 0.6f, r * 0.7f), color, 1.0f);
                break;

            case Icon.Stop:
                dl.AddRectFilled(center - new Vector2(r * 0.6f, r * 0.6f), center + new Vector2(r * 0.6f, r * 0.6f), color, 1.5f);
                break;

            case Icon.Record:
                dl.AddCircleFilled(center, r * 0.65f, color, 24);
                break;

            case Icon.Add:
                dl.AddLine(center + new Vector2(-r * 0.7f, 0), center + new Vector2(r * 0.7f, 0), color, thickness);
                dl.AddLine(center + new Vector2(0, -r * 0.7f), center + new Vector2(0, r * 0.7f), color, thickness);
                break;

            case Icon.Delete:
                dl.AddRect(center + new Vector2(-r * 0.55f, -r * 0.45f), center + new Vector2(r * 0.55f, r * 0.75f), color, 1.0f, ImDrawFlags.None, thickness);
                dl.AddLine(center + new Vector2(-r * 0.75f, -r * 0.45f), center + new Vector2(r * 0.75f, -r * 0.45f), color, thickness);
                dl.AddLine(center + new Vector2(-r * 0.25f, -r * 0.45f), center + new Vector2(-r * 0.25f, -r * 0.7f), color, thickness);
                dl.AddLine(center + new Vector2(r * 0.25f, -r * 0.45f), center + new Vector2(r * 0.25f, -r * 0.7f), color, thickness);
                dl.AddLine(center + new Vector2(-r * 0.25f, -r * 0.7f), center + new Vector2(r * 0.25f, -r * 0.7f), color, thickness);
                break;

            case Icon.Save:
                dl.AddRect(center - new Vector2(r * 0.65f, r * 0.65f), center + new Vector2(r * 0.65f, r * 0.65f), color, 1.0f, ImDrawFlags.None, thickness);
                dl.AddLine(center + new Vector2(-r * 0.3f, -r * 0.65f), center + new Vector2(-r * 0.3f, -r * 0.1f), color, thickness);
                dl.AddLine(center + new Vector2(r * 0.3f, -r * 0.65f), center + new Vector2(r * 0.3f, -r * 0.1f), color, thickness);
                dl.AddLine(center + new Vector2(-r * 0.3f, -r * 0.1f), center + new Vector2(r * 0.3f, -r * 0.1f), color, thickness);
                dl.AddRectFilled(center + new Vector2(-r * 0.35f, r * 0.15f), center + new Vector2(r * 0.35f, r * 0.55f), color, 0.5f);
                break;

            case Icon.Load:
                dl.AddLine(center + new Vector2(0, r * 0.6f), center + new Vector2(0, -r * 0.5f), color, thickness);
                dl.AddLine(center + new Vector2(-r * 0.4f, -r * 0.1f), center + new Vector2(0, -r * 0.5f), color, thickness);
                dl.AddLine(center + new Vector2(r * 0.4f, -r * 0.1f), center + new Vector2(0, -r * 0.5f), color, thickness);
                dl.AddLine(center + new Vector2(-r * 0.65f, r * 0.6f), center + new Vector2(r * 0.65f, r * 0.6f), color, thickness);
                break;

            case Icon.Export:
                dl.AddLine(center + new Vector2(0, -r * 0.6f), center + new Vector2(0, r * 0.5f), color, thickness);
                dl.AddLine(center + new Vector2(-r * 0.4f, r * 0.1f), center + new Vector2(0, r * 0.5f), color, thickness);
                dl.AddLine(center + new Vector2(r * 0.4f, r * 0.1f), center + new Vector2(0, r * 0.5f), color, thickness);
                dl.AddLine(center + new Vector2(-r * 0.65f, -r * 0.6f), center + new Vector2(r * 0.65f, -r * 0.6f), color, thickness);
                break;

            case Icon.Search:
                dl.AddCircle(center + new Vector2(-r * 0.15f, -r * 0.15f), r * 0.45f, color, 20, thickness);
                dl.AddLine(center + new Vector2(r * 0.2f, r * 0.2f), center + new Vector2(r * 0.7f, r * 0.7f), color, thickness);
                break;

            case Icon.Close:
                dl.AddLine(center + new Vector2(-r * 0.6f, -r * 0.6f), center + new Vector2(r * 0.6f, r * 0.6f), color, thickness);
                dl.AddLine(center + new Vector2(-r * 0.6f, r * 0.6f), center + new Vector2(r * 0.6f, -r * 0.6f), color, thickness);
                break;

            case Icon.Warning:
                dl.AddTriangle(center + new Vector2(0, -r * 0.75f), center + new Vector2(-r * 0.7f, r * 0.6f), center + new Vector2(r * 0.7f, r * 0.6f), color, thickness);
                dl.AddLine(center + new Vector2(0, -r * 0.25f), center + new Vector2(0, r * 0.15f), color, thickness);
                dl.AddCircleFilled(center + new Vector2(0, r * 0.4f), thickness * 0.6f, color, 8);
                break;

            case Icon.Success:
                dl.AddLine(center + new Vector2(-r * 0.55f, 0), center + new Vector2(-r * 0.1f, r * 0.5f), color, thickness);
                dl.AddLine(center + new Vector2(-r * 0.1f, r * 0.5f), center + new Vector2(r * 0.6f, -r * 0.45f), color, thickness);
                break;

            case Icon.Info:
                dl.AddCircle(center, r * 0.7f, color, 20, thickness);
                dl.AddCircleFilled(center + new Vector2(0, -r * 0.3f), thickness * 0.55f, color, 8);
                dl.AddLine(center + new Vector2(0, -r * 0.05f), center + new Vector2(0, r * 0.4f), color, thickness);
                break;

            case Icon.Settings:
                dl.AddCircle(center, r * 0.65f, color, 6, thickness * 1.3f);
                dl.AddCircleFilled(center, r * 0.22f, color, 12);
                break;

            case Icon.DragHandle:
                for (int row = -1; row <= 1; row++)
                {
                    for (int col = -1; col <= 0; col++)
                    {
                        dl.AddCircleFilled(center + new Vector2(col * r * 0.5f + r * 0.25f, row * r * 0.45f), thickness * 0.5f, color, 6);
                    }
                }

                break;

            case Icon.Duplicate:
                dl.AddRect(center + new Vector2(-r * 0.55f, -r * 0.55f), center + new Vector2(r * 0.25f, r * 0.25f), color, 1.0f, ImDrawFlags.None, thickness);
                dl.AddRect(center + new Vector2(-r * 0.25f, -r * 0.25f), center + new Vector2(r * 0.55f, r * 0.55f), color, 1.0f, ImDrawFlags.None, thickness);
                break;

            case Icon.Up:
                dl.AddTriangleFilled(center + new Vector2(0, -r * 0.6f), center + new Vector2(-r * 0.6f, r * 0.4f), center + new Vector2(r * 0.6f, r * 0.4f), color);
                break;

            case Icon.Down:
                dl.AddTriangleFilled(center + new Vector2(0, r * 0.6f), center + new Vector2(-r * 0.6f, -r * 0.4f), center + new Vector2(r * 0.6f, -r * 0.4f), color);
                break;

            case Icon.Screenshot:
                dl.AddRect(center + new Vector2(-r * 0.7f, -r * 0.45f), center + new Vector2(r * 0.7f, r * 0.5f), color, 1.5f, ImDrawFlags.None, thickness);
                dl.AddCircle(center + new Vector2(0, 0.05f * r), r * 0.3f, color, 16, thickness);
                dl.AddLine(center + new Vector2(-r * 0.25f, -r * 0.45f), center + new Vector2(-r * 0.1f, -r * 0.65f), color, thickness);
                dl.AddLine(center + new Vector2(-r * 0.1f, -r * 0.65f), center + new Vector2(r * 0.25f, -r * 0.65f), color, thickness);
                dl.AddLine(center + new Vector2(r * 0.25f, -r * 0.65f), center + new Vector2(r * 0.4f, -r * 0.45f), color, thickness);
                break;
        }
    }

    public static void Inline(Icon icon, float size, Vector4? color = null)
    {
        Vector2 cursorScreen = ImGui.GetCursorScreenPos();
        Vector2 center = cursorScreen + new Vector2(size * 0.5f, size * 0.5f);
        uint col = ImGui.GetColorU32(color ?? Theme.Text);
        Draw(ImGui.GetWindowDrawList(), icon, center, size, col);
        ImGui.Dummy(new Vector2(size, size));
    }

    public static bool IconButton(string id, Icon icon, float size = 22.0f)
    {
        ImGui.PushID(id);
        Vector2 cursorScreen = ImGui.GetCursorScreenPos();
        Vector2 boxSize = new(size + Theme.SpaceSm, size + Theme.SpaceSm);
        bool clicked = ImGui.InvisibleButton("##iconbtn", boxSize);
        bool hovered = ImGui.IsItemHovered();
        bool active = ImGui.IsItemActive();

        ImDrawListPtr dl = ImGui.GetWindowDrawList();
        Vector4 bg = active ? Theme.AccentActive : hovered ? Theme.AccentHover : new Vector4(0.20f, 0.20f, 0.25f, 1.0f);
        dl.AddRectFilled(cursorScreen, cursorScreen + boxSize, ImGui.GetColorU32(bg), 4.0f);

        Vector2 center = cursorScreen + boxSize * 0.5f;
        Draw(dl, icon, center, size, ImGui.GetColorU32(Theme.Text));

        ImGui.PopID();
        return clicked;
    }

    public static bool IconLabelButton(string id, Icon icon, string label, Vector2 sizeArg = default)
    {
        ImGui.PushID(id);
        Vector2 cursorScreen = ImGui.GetCursorScreenPos();
        Vector2 textSize = ImGui.CalcTextSize(label);
        float iconSize = ImGui.GetFontSize();
        Vector2 contentSize = new(iconSize + Theme.SpaceXs + textSize.X, MathF.Max(iconSize, textSize.Y));
        Vector2 padding = new(Theme.SpaceSm, Theme.SpaceXs);
        Vector2 boxSize = sizeArg != default
            ? sizeArg
            : contentSize + padding * 2.0f;

        bool clicked = ImGui.InvisibleButton("##iconlabelbtn", boxSize);
        bool hovered = ImGui.IsItemHovered();
        bool active = ImGui.IsItemActive();

        ImDrawListPtr dl = ImGui.GetWindowDrawList();
        Vector4 bg = active ? Theme.AccentActive : hovered ? Theme.AccentHover : new Vector4(0.20f, 0.20f, 0.25f, 1.0f);
        dl.AddRectFilled(cursorScreen, cursorScreen + boxSize, ImGui.GetColorU32(bg), 4.0f);

        Vector2 innerStart = cursorScreen + (boxSize - contentSize) * 0.5f;
        Vector2 iconCenter = innerStart + new Vector2(iconSize * 0.5f, contentSize.Y * 0.5f);
        Draw(dl, icon, iconCenter, iconSize * 0.85f, ImGui.GetColorU32(Theme.Text));

        Vector2 textPos = innerStart + new Vector2(iconSize + Theme.SpaceXs, (contentSize.Y - textSize.Y) * 0.5f);
        dl.AddText(textPos, ImGui.GetColorU32(Theme.Text), label);

        ImGui.PopID();
        return clicked;
    }
}
