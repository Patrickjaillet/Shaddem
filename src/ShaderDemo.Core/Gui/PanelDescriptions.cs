// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;

namespace ShaderDemo.Core.Gui;

public static class PanelDescriptions
{
    private static readonly Dictionary<string, string> Descriptions = new()
    {
        ["General"] = "Timeline on/off, audio reactivity, volume, and live bass/treble levels at a glance.",
        ["System"] = "FPS/time readout, screenshot, reset, project save/load, and settings persistence.",
        ["Layers"] = "Stack multiple shaders with blend modes and opacity to build up a composite look.",
        ["Timeline"] = "Sequence shaders, text, effects, and music on a scrubbable timeline with markers.",
        ["Audio"] = "Load a music file or live mic input to drive audio-reactive effects and overlays.",
        ["Media"] = "Load an image as the iChannel0 input texture, or browse a folder of media files.",
        ["3D Model"] = "Load and position an OBJ model rendered on top of the shader background.",
        ["Templates"] = "Curated shader + effect combos that produce a complete look in one click, plus a full-demo randomizer.",
        ["Presets"] = "Save, apply, and randomize full effect-parameter snapshots.",
        ["Live Coding"] = "Edit and hot-compile shader code directly, with reusable snippets.",
        ["Export"] = "Record video/GIF, take screenshots, and export audio-only or subtitle files.",
        ["Migration"] = "One-time import of a project from the original Python version.",
        ["Window"] = "Open a secondary output window for a second monitor or projector.",
        ["Shader / Snippet / Track Management"] = "Create, duplicate, or delete shader files, manage code snippets and timeline tracks.",
        ["Debug / Performance"] = "Opt-in GPU timing per render pass, for diagnosing frame-time cost.",
        ["About"] = "Version, copyright, and tech-stack credits.",
    };

    public static IReadOnlyDictionary<string, string> All => Descriptions;

    public static void Draw(string panelName)
    {
        if (Descriptions.TryGetValue(panelName, out string? description))
        {
            ImGui.TextColored(Theme.TextMuted, description);
        }
    }
}
