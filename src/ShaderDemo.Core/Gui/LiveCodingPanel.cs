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
    private static string? _editingFilePath;

    public static void Draw(ShaderManager manager, string shaderDirectory)
    {
        bool active = manager.IsLiveCodingActive;
        if (ImGui.Checkbox("Live Coding Active", ref active)) manager.IsLiveCodingActive = active;

        if (ImGui.Button("Load Current Shader Into Editor") && manager.CurrentShaderName != null)
        {
            string path = Path.Combine(shaderDirectory, manager.CurrentShaderName);
            if (File.Exists(path))
            {
                _code = File.ReadAllText(path);
                _editingFilePath = path;
                _status = $"Loaded {manager.CurrentShaderName} into editor.";
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Save to File") && _editingFilePath != null)
        {
            try
            {
                File.WriteAllText(_editingFilePath, _code);
                _status = $"Saved to {_editingFilePath}";
            }
            catch (IOException ex)
            {
                _status = $"Save failed: {ex.Message}";
            }
        }

        string[] snippetNames = Snippets.Library.Keys.Concat(CustomSnippets.Library.Keys).ToArray();
        _snippetIndex = Math.Clamp(_snippetIndex, 0, Math.Max(0, snippetNames.Length - 1));
        if (snippetNames.Length > 0) ImGui.Combo("Snippet", ref _snippetIndex, snippetNames, snippetNames.Length);
        ImGui.SameLine();
        if (ImGui.Button("Insert") && snippetNames.Length > 0)
        {
            string name = snippetNames[_snippetIndex];
            string body = Snippets.Library.TryGetValue(name, out string? builtin) ? builtin : CustomSnippets.Library[name];
            _code += "\n" + body;
        }

        if (Theme.FontMono is { } mono) ImGui.PushFont(mono);
        ImGui.InputTextMultiline("##code", ref _code, 8192, new System.Numerics.Vector2(-1, 200));
        if (Theme.FontMono != null) ImGui.PopFont();

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
