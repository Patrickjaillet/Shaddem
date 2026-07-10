// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using ImGuiNET;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;

namespace ShaderDemo.Core.Gui;

public static class CommandPalette
{
    private sealed class Command
    {
        public required string Label;
        public required Action Execute;
    }

    private static bool _isOpen;
    private static string _query = "";
    private static bool _focusRequested;

    public static void Open()
    {
        _isOpen = true;
        _query = "";
        _focusRequested = true;
    }

    public static void Toggle()
    {
        if (_isOpen) _isOpen = false;
        else Open();
    }

    public static void Draw(ShaderManager manager, AppSettings settings)
    {
        if (!_isOpen) return;

        List<Command> commands = BuildCommands(manager, settings);
        List<Command> filtered = string.IsNullOrWhiteSpace(_query)
            ? commands
            : commands.Where(c => c.Label.Contains(_query, StringComparison.OrdinalIgnoreCase)).ToList();

        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        Vector2 size = new(480, 360);
        ImGui.SetNextWindowPos(viewport.GetCenter(), ImGuiCond.Always, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(size, ImGuiCond.Always);

        ImGuiWindowFlags flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings;
        ImGui.Begin("Command Palette (Ctrl+K)", ref _isOpen, flags);

        Elevation.DrawShadow(ImGui.GetWindowPos(), ImGui.GetWindowSize());

        Icons.Inline(Icon.Search, ImGui.GetFontSize(), Theme.TextMuted);
        ImGui.SameLine();

        if (_focusRequested)
        {
            ImGui.SetKeyboardFocusHere();
            _focusRequested = false;
        }

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputText("##query", ref _query, 128))
        {
        }

        if (ImGui.IsKeyPressed(ImGuiKey.Escape))
        {
            _isOpen = false;
        }

        ImGui.Separator();
        ImGui.BeginChild("##results");

        foreach (Command command in filtered)
        {
            if (ImGui.Selectable(command.Label))
            {
                command.Execute();
                _isOpen = false;
            }
        }

        if (filtered.Count == 0)
        {
            ImGui.TextColored(Theme.TextMuted, "No matching command.");
        }

        ImGui.EndChild();
        ImGui.End();
    }

    private static List<Command> BuildCommands(ShaderManager manager, AppSettings settings)
    {
        var commands = new List<Command>
        {
            new() { Label = "Action: Screenshot", Execute = () => { string path = Export.ScreenshotService.Save(manager.LastComposedFrame ?? manager.Pipeline.SceneFbo, "screenshots"); ToastManager.Show($"Screenshot saved: {path}", ToastLevel.Success); } },
            new() { Label = "Action: Next Shader", Execute = manager.NextShader },
            new() { Label = "Action: Previous Shader", Execute = manager.PreviousShader },
            new() { Label = "Action: Toggle Audio Reactivity", Execute = () => { settings.AudioReactive = !settings.AudioReactive; manager.Audio.Enabled = settings.AudioReactive; } },
            new() { Label = "Panel: Layers", Execute = () => EffectsPanel.RequestOpenPanel("Layers") },
            new() { Label = "Panel: Timeline", Execute = () => EffectsPanel.RequestOpenPanel("Timeline") },
            new() { Label = "Panel: Audio", Execute = () => EffectsPanel.RequestOpenPanel("Audio") },
            new() { Label = "Panel: Media", Execute = () => EffectsPanel.RequestOpenPanel("Media") },
            new() { Label = "Panel: 3D Model", Execute = () => EffectsPanel.RequestOpenPanel("3D Model") },
            new() { Label = "Panel: Presets", Execute = () => EffectsPanel.RequestOpenPanel("Presets") },
            new() { Label = "Panel: Live Coding", Execute = () => EffectsPanel.RequestOpenPanel("Live Coding") },
            new() { Label = "Panel: Export", Execute = () => EffectsPanel.RequestOpenPanel("Export") },
            new() { Label = "Panel: System", Execute = () => EffectsPanel.RequestOpenPanel("System") },
            new() { Label = "Panel: About", Execute = () => EffectsPanel.RequestOpenPanel("About") },
        };

        for (int i = 0; i < manager.ShaderNames.Count; i++)
        {
            int index = i;
            commands.Add(new Command { Label = $"Shader: {manager.ShaderNames[i]}", Execute = () => manager.SelectShader(index) });
        }

        return commands;
    }
}
