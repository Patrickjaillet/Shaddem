// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using ImGuiNET;

namespace ShaderDemo.Core.Gui;

public enum ToastLevel
{
    Info,
    Success,
    Warning,
    Danger,
}

public static class ToastManager
{
    private sealed class Toast
    {
        public required string Message;
        public required ToastLevel Level;
        public required double CreatedAtSeconds;
    }

    private const double LifetimeSeconds = 4.0;
    private const double FadeSeconds = 0.4;

    private static readonly List<Toast> _toasts = new();
    private static double _clockSeconds;

    public static void Show(string message, ToastLevel level = ToastLevel.Info)
    {
        _toasts.Add(new Toast { Message = message, Level = level, CreatedAtSeconds = _clockSeconds });
    }

    public static void Draw(float deltaSeconds)
    {
        _clockSeconds += deltaSeconds;
        _toasts.RemoveAll(t => _clockSeconds - t.CreatedAtSeconds > LifetimeSeconds);

        if (_toasts.Count == 0) return;

        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        Vector2 basePos = new(viewport.WorkPos.X + viewport.WorkSize.X - Theme.SpaceLg, viewport.WorkPos.Y + viewport.WorkSize.Y - Theme.SpaceLg);
        float cursorY = 0;

        for (int i = _toasts.Count - 1; i >= 0; i--)
        {
            Toast toast = _toasts[i];
            double age = _clockSeconds - toast.CreatedAtSeconds;
            double remaining = LifetimeSeconds - age;

            float alpha = age < FadeSeconds
                ? (float)(age / FadeSeconds)
                : remaining < FadeSeconds ? (float)(remaining / FadeSeconds) : 1.0f;
            alpha = Math.Clamp(alpha, 0.0f, 1.0f);

            Vector4 color = toast.Level switch
            {
                ToastLevel.Success => Theme.Success,
                ToastLevel.Warning => Theme.Warning,
                ToastLevel.Danger => Theme.Danger,
                _ => Theme.Info,
            };

            Icon icon = toast.Level switch
            {
                ToastLevel.Success => Icon.Success,
                ToastLevel.Warning => Icon.Warning,
                ToastLevel.Danger => Icon.Warning,
                _ => Icon.Info,
            };

            ImGui.SetNextWindowPos(new Vector2(basePos.X, basePos.Y - cursorY), ImGuiCond.Always, new Vector2(1.0f, 1.0f));
            ImGui.SetNextWindowBgAlpha(0.92f * alpha);
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, alpha);

            ImGuiWindowFlags flags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings
                | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.AlwaysAutoResize;

            if (ImGui.Begin($"##toast{i}", flags))
            {
                Icons.Inline(icon, ImGui.GetFontSize(), color);
                ImGui.SameLine();
                ImGui.PushTextWrapPos(ImGui.GetCursorPosX() + 280.0f);
                ImGui.TextColored(color, toast.Message);
                ImGui.PopTextWrapPos();
                cursorY += ImGui.GetWindowSize().Y + Theme.SpaceSm;
            }

            ImGui.End();
            ImGui.PopStyleVar();
        }
    }
}
