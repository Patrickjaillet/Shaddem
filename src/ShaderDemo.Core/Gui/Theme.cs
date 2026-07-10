// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using System.Numerics;

namespace ShaderDemo.Core.Gui;

public static class Theme
{
    public static readonly Vector4 SurfaceBase = new(0.09f, 0.09f, 0.11f, 0.97f);
    public static readonly Vector4 SurfaceElevated = new(0.13f, 0.13f, 0.16f, 1.0f);
    public static readonly Vector4 SurfaceOverlay = new(0.17f, 0.17f, 0.20f, 0.98f);

    public static readonly Vector4 Text = new(0.95f, 0.96f, 0.98f, 1.0f);
    public static readonly Vector4 TextMuted = new(0.60f, 0.61f, 0.65f, 1.0f);

    public static readonly Vector4 Accent = new(0.0f, 0.55f, 0.90f, 1.0f);
    public static readonly Vector4 AccentHover = new(0.15f, 0.65f, 1.0f, 1.0f);
    public static readonly Vector4 AccentActive = new(0.0f, 0.42f, 0.75f, 1.0f);

    public static readonly Vector4 Success = new(0.30f, 0.80f, 0.40f, 1.0f);
    public static readonly Vector4 Warning = new(0.95f, 0.75f, 0.20f, 1.0f);
    public static readonly Vector4 Danger = new(0.95f, 0.35f, 0.35f, 1.0f);
    public static readonly Vector4 Info = new(0.45f, 0.70f, 1.0f, 1.0f);

    public static readonly Vector4 Border = new(0.26f, 0.26f, 0.30f, 0.5f);

    public static readonly Vector4 SectionShaders = new(0.62f, 0.42f, 0.95f, 1.0f);
    public static readonly Vector4 SectionLayers = new(0.35f, 0.78f, 0.45f, 1.0f);
    public static readonly Vector4 SectionEffects = new(0.95f, 0.60f, 0.20f, 1.0f);
    public static readonly Vector4 SectionAudio = new(0.90f, 0.35f, 0.65f, 1.0f);
    public static readonly Vector4 SectionTimeline = new(0.30f, 0.80f, 0.85f, 1.0f);
    public static readonly Vector4 Section3D = new(0.25f, 0.70f, 0.65f, 1.0f);
    public static readonly Vector4 SectionExport = new(0.95f, 0.45f, 0.30f, 1.0f);

    public static readonly Vector4 AudioBass = new(0.90f, 0.35f, 0.30f, 1.0f);
    public static readonly Vector4 AudioMid = Accent;
    public static readonly Vector4 AudioTreble = new(0.30f, 0.85f, 0.90f, 1.0f);

    public static readonly Vector4 ClipShader = new(0.85f, 0.5f, 0.15f, 0.9f);
    public static readonly Vector4 ClipImage = new(0.2f, 0.6f, 0.6f, 0.9f);
    public static readonly Vector4 ClipMusic = new(0.25f, 0.7f, 0.3f, 0.9f);
    public static readonly Vector4 ClipEffect = new(0.6f, 0.3f, 0.8f, 0.9f);
    public static readonly Vector4 ClipText = new(0.85f, 0.8f, 0.2f, 0.9f);
    public static readonly Vector4 ClipModel3D = new(0.25f, 0.45f, 0.85f, 0.9f);
    public static readonly Vector4 ClipLayerAutomation = new(0.8f, 0.4f, 0.55f, 0.9f);

    public const float SpaceXs = 4.0f;
    public const float SpaceSm = 8.0f;
    public const float SpaceMd = 12.0f;
    public const float SpaceLg = 16.0f;
    public const float SpaceXl = 24.0f;

    public static ImFontPtr? FontRegular { get; private set; }
    public static ImFontPtr? FontSemibold { get; private set; }
    public static ImFontPtr? FontMono { get; private set; }
    public static ImFontPtr? FontTitle { get; private set; }

    public static class Icons
    {
        public const string Stop = "■";
        public const string Record = "●";
        public const string Up = "▲";
        public const string Down = "▼";
        public const string Close = "×";
        public const string Bullet = "•";
    }

    public static unsafe void LoadFonts(ImGuiIOPtr io, float dpiScale = 1.0f)
    {
        int size = (int)MathF.Round(17 * dpiScale);
        int monoSize = (int)MathF.Round(15 * dpiScale);
        int titleSize = (int)MathF.Round(23 * dpiScale);

        string regularPath = @"C:\Windows\Fonts\segoeui.ttf";
        string semiboldPath = @"C:\Windows\Fonts\segoeuisl.ttf";
        string monoPath = @"C:\Windows\Fonts\consola.ttf";

        ushort[] ranges =
        {
            0x0020, 0x00FF,
            0x2022, 0x2022,
            0x25A0, 0x25CF,
            0,
        };

        fixed (ushort* rangePtr = ranges)
        {
            nint rangePtrAddress = (nint)rangePtr;
            if (File.Exists(regularPath)) FontRegular = io.Fonts.AddFontFromFileTTF(regularPath, size, null, rangePtrAddress);
            if (File.Exists(semiboldPath)) FontSemibold = io.Fonts.AddFontFromFileTTF(semiboldPath, size, null, rangePtrAddress);
            if (File.Exists(monoPath)) FontMono = io.Fonts.AddFontFromFileTTF(monoPath, monoSize, null, rangePtrAddress);
            if (File.Exists(semiboldPath)) FontTitle = io.Fonts.AddFontFromFileTTF(semiboldPath, titleSize, null, rangePtrAddress);
        }

        if (FontRegular == null) io.Fonts.AddFontDefault();
    }

    public static bool PushFontIf(ImFontPtr? font)
    {
        if (font is not { } f) return false;
        ImGui.PushFont(f);
        return true;
    }

    public static void PopFontIf(bool pushed)
    {
        if (pushed) ImGui.PopFont();
    }

    public static void Heading(string text, Vector4? color = null)
    {
        bool pushed = PushFontIf(FontSemibold);
        ImGui.TextColored(color ?? Text, text);
        PopFontIf(pushed);
    }

    public static void Mono(string text, Vector4? color = null)
    {
        bool pushed = PushFontIf(FontMono);
        ImGui.TextColored(color ?? Text, text);
        PopFontIf(pushed);
    }

    public static void Apply()
    {
        ImGuiStylePtr style = ImGui.GetStyle();
        var colors = style.Colors;

        colors[(int)ImGuiCol.Text] = Text;
        colors[(int)ImGuiCol.TextDisabled] = TextMuted;
        colors[(int)ImGuiCol.WindowBg] = SurfaceBase;
        colors[(int)ImGuiCol.ChildBg] = SurfaceElevated with { W = 1.0f };
        colors[(int)ImGuiCol.PopupBg] = SurfaceOverlay;
        colors[(int)ImGuiCol.Border] = Border;
        colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

        colors[(int)ImGuiCol.FrameBg] = SurfaceElevated;
        colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.20f, 0.20f, 0.24f, 1.0f);
        colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.25f, 0.25f, 0.30f, 1.0f);

        colors[(int)ImGuiCol.TitleBg] = new Vector4(0.08f, 0.08f, 0.10f, 1.0f);
        colors[(int)ImGuiCol.TitleBgActive] = SurfaceElevated with { W = 1.0f };
        colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.08f, 0.08f, 0.10f, 0.5f);

        colors[(int)ImGuiCol.MenuBarBg] = SurfaceElevated with { W = 1.0f };

        colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.07f, 0.07f, 0.09f, 1.0f);
        colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.30f, 0.30f, 0.35f, 1.0f);
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.40f, 0.40f, 0.45f, 1.0f);
        colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.50f, 0.50f, 0.55f, 1.0f);

        colors[(int)ImGuiCol.CheckMark] = Accent;

        colors[(int)ImGuiCol.SliderGrab] = Accent;
        colors[(int)ImGuiCol.SliderGrabActive] = AccentActive;

        colors[(int)ImGuiCol.Button] = new Vector4(0.20f, 0.20f, 0.25f, 1.0f);
        colors[(int)ImGuiCol.ButtonHovered] = AccentHover;
        colors[(int)ImGuiCol.ButtonActive] = AccentActive;

        colors[(int)ImGuiCol.Header] = new Vector4(0.20f, 0.20f, 0.25f, 1.0f);
        colors[(int)ImGuiCol.HeaderHovered] = AccentHover;
        colors[(int)ImGuiCol.HeaderActive] = AccentActive;

        colors[(int)ImGuiCol.Separator] = Border;
        colors[(int)ImGuiCol.SeparatorHovered] = AccentHover;
        colors[(int)ImGuiCol.SeparatorActive] = AccentActive;

        colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.30f, 0.30f, 0.35f, 1.0f);
        colors[(int)ImGuiCol.ResizeGripHovered] = AccentHover;
        colors[(int)ImGuiCol.ResizeGripActive] = AccentActive;

        colors[(int)ImGuiCol.Tab] = new Vector4(0.16f, 0.16f, 0.19f, 1.0f);
        colors[(int)ImGuiCol.TabHovered] = AccentHover;
        colors[(int)ImGuiCol.TabActive] = Accent;
        colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.09f, 0.09f, 0.11f, 1.0f);
        colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.18f, 0.18f, 0.22f, 1.0f);

        colors[(int)ImGuiCol.PlotLines] = Text;
        colors[(int)ImGuiCol.PlotLinesHovered] = AccentHover;
        colors[(int)ImGuiCol.PlotHistogram] = Accent;
        colors[(int)ImGuiCol.PlotHistogramHovered] = AccentHover;

        colors[(int)ImGuiCol.TextSelectedBg] = Accent with { W = 0.5f };
        colors[(int)ImGuiCol.DragDropTarget] = Warning;

        style.WindowRounding = 5.0f;
        style.ChildRounding = 5.0f;
        style.FrameRounding = 4.0f;
        style.GrabRounding = 4.0f;
        style.PopupRounding = 5.0f;
        style.ScrollbarRounding = 9.0f;
        style.TabRounding = 4.0f;

        style.WindowPadding = new Vector2(SpaceMd, SpaceMd);
        style.FramePadding = new Vector2(SpaceSm, SpaceXs);
        style.ItemSpacing = new Vector2(SpaceSm, SpaceSm);
        style.ItemInnerSpacing = new Vector2(SpaceXs, SpaceXs);
        style.IndentSpacing = SpaceLg;
        style.ScrollbarSize = SpaceLg;
        style.GrabMinSize = SpaceMd;
    }
}
