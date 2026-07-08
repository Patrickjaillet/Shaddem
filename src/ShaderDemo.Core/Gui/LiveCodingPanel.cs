// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Gui;

public static class LiveCodingPanel
{
    private static string _code = """
        void mainImage(out vec4 fragColor, in vec2 fragCoord) {
            vec2 uv = fragCoord / iResolution.xy;
            fragColor = vec4(uv, 0.5 + 0.5 * sin(iTime), 1.0);
        }
        """;

    private static string _status = "";
    private static int _snippetIndex;
    private static readonly string[] SnippetNames = Snippets.Library.Keys.ToArray();

    public static void Draw(ShaderManager manager)
    {
        bool active = manager.IsLiveCodingActive;
        if (ImGui.Checkbox("Live Coding Active", ref active)) manager.IsLiveCodingActive = active;

        ImGui.Combo("Snippet", ref _snippetIndex, SnippetNames, SnippetNames.Length);
        ImGui.SameLine();
        if (ImGui.Button("Insert"))
        {
            _code += "\n" + Snippets.Library[SnippetNames[_snippetIndex]];
        }

        ImGui.InputTextMultiline("##code", ref _code, 8192, new System.Numerics.Vector2(-1, 200));

        if (ImGui.Button("Compile"))
        {
            bool ok = manager.CompileLiveShader(_code, msg => _status = msg);
            _status = ok ? "Compiled successfully." : _status;
        }

        if (_status.Length > 0)
        {
            ImGui.TextWrapped(_status);
        }
    }
}
