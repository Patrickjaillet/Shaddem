// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using ImGuiNET;
using ShaderDemo.Core.Logging;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.Core.Gui;

public static class PopupsPanel
{
    private const string DefaultShaderTemplate = """
        void mainImage(out vec4 fragColor, in vec2 fragCoord) {
            vec2 uv = fragCoord / iResolution.xy;
            fragColor = vec4(uv, 0.5 + 0.5 * sin(iTime), 1.0);
        }
        """;

    private static string _newShaderName = "new_shader";
    private static string _duplicateNewName = "";
    private static string _deleteConfirmName = "";
    private static string _snippetName = "";
    private static string _snippetCode = "";

    private static string _newTrackName = "Custom";
    private static readonly bool[] _newTrackTypeFlags = new bool[Enum.GetValues<ClipType>().Length];

    public static void Draw(ShaderManager manager, TimelineEngine timeline, string shaderDirectory)
    {
        ImGui.Text("Shader Management");

        if (ImGui.Button("New Shader"))
        {
            _newShaderName = "new_shader";
            ImGui.OpenPopup("Create Shader");
        }

        ImGui.SameLine();
        if (ImGui.Button("Duplicate Current"))
        {
            _duplicateNewName = manager.CurrentShaderName != null ? "copy_of_" + manager.CurrentShaderName : "copy.glsl";
            ImGui.OpenPopup("Duplicate Shader");
        }

        ImGui.SameLine();
        if (ImGui.Button("Delete Current"))
        {
            _deleteConfirmName = manager.CurrentShaderName ?? "";
            ImGui.OpenPopup("Delete Shader");
        }

        DrawCreateShaderPopup(manager, shaderDirectory);
        DrawDuplicateShaderPopup(manager, shaderDirectory);
        DrawDeleteShaderPopup(manager, shaderDirectory);

        ImGui.Separator();
        ImGui.Text("Snippets");
        if (ImGui.Button("Add Snippet"))
        {
            _snippetName = "";
            _snippetCode = "";
            ImGui.OpenPopup("Add Snippet");
        }

        DrawAddSnippetPopup();

        ImGui.Separator();
        ImGui.Text("Tracks");
        if (ImGui.Button("Manage Tracks"))
        {
            ImGui.OpenPopup("Manage Tracks");
        }

        DrawTrackManagementPopup(timeline);
    }

    private static void DrawCreateShaderPopup(ShaderManager manager, string shaderDirectory)
    {
        if (!ImGui.BeginPopupModal("Create Shader")) return;

        ImGui.InputText("File Name", ref _newShaderName, 128);
        ImGui.TextWrapped("Creates a new .glsl file with a starter template and loads it.");

        if (ImGui.Button("Create"))
        {
            string fileName = _newShaderName.EndsWith(".glsl", StringComparison.OrdinalIgnoreCase) ? _newShaderName : _newShaderName + ".glsl";
            string path = Path.Combine(shaderDirectory, fileName);

            if (!File.Exists(path))
            {
                Directory.CreateDirectory(shaderDirectory);
                File.WriteAllText(path, DefaultShaderTemplate);
            }

            if (ShaderLoader.LoadFile(manager, path, AppLog.Info))
            {
                int idx = manager.ShaderNames.ToList().IndexOf(fileName);
                if (idx >= 0) manager.SelectShader(idx);
            }

            ImGui.CloseCurrentPopup();
        }

        ImGui.SameLine();
        if (ImGui.Button("Cancel")) ImGui.CloseCurrentPopup();

        ImGui.EndPopup();
    }

    private static void DrawDuplicateShaderPopup(ShaderManager manager, string shaderDirectory)
    {
        if (!ImGui.BeginPopupModal("Duplicate Shader")) return;

        string sourceName = manager.CurrentShaderName ?? "<none>";
        ImGui.Text($"Source: {sourceName}");
        ImGui.InputText("New File Name", ref _duplicateNewName, 128);

        if (ImGui.Button("Duplicate") && manager.CurrentShaderName != null)
        {
            string sourcePath = Path.Combine(shaderDirectory, manager.CurrentShaderName);
            string fileName = _duplicateNewName.EndsWith(".glsl", StringComparison.OrdinalIgnoreCase) ? _duplicateNewName : _duplicateNewName + ".glsl";
            string destPath = Path.Combine(shaderDirectory, fileName);

            if (File.Exists(sourcePath) && !File.Exists(destPath))
            {
                File.Copy(sourcePath, destPath);
                if (ShaderLoader.LoadFile(manager, destPath, AppLog.Info))
                {
                    int idx = manager.ShaderNames.ToList().IndexOf(fileName);
                    if (idx >= 0) manager.SelectShader(idx);
                }
            }

            ImGui.CloseCurrentPopup();
        }

        ImGui.SameLine();
        if (ImGui.Button("Cancel")) ImGui.CloseCurrentPopup();

        ImGui.EndPopup();
    }

    private static void DrawDeleteShaderPopup(ShaderManager manager, string shaderDirectory)
    {
        if (!ImGui.BeginPopupModal("Delete Shader")) return;

        ImGui.TextColored(Theme.Danger, $"Delete '{_deleteConfirmName}'? This removes the file from disk.");

        if (Icons.IconLabelButton("DeleteShader", Icon.Delete, "Delete") && _deleteConfirmName.Length > 0)
        {
            manager.RemoveShader(_deleteConfirmName);
            string path = Path.Combine(shaderDirectory, _deleteConfirmName);
            if (File.Exists(path)) File.Delete(path);
            manager.SelectShader(manager.CurrentShaderIndex);

            ImGui.CloseCurrentPopup();
        }

        ImGui.SameLine();
        if (ImGui.Button("Cancel")) ImGui.CloseCurrentPopup();

        ImGui.EndPopup();
    }

    private static void DrawAddSnippetPopup()
    {
        if (!ImGui.BeginPopupModal("Add Snippet")) return;

        ImGui.InputText("Name", ref _snippetName, 128);
        ImGui.InputTextMultiline("Code", ref _snippetCode, 4096, new Vector2(400, 150));

        if (ImGui.Button("Save") && _snippetName.Length > 0)
        {
            CustomSnippets.Library[_snippetName] = _snippetCode;
            ImGui.CloseCurrentPopup();
        }

        ImGui.SameLine();
        if (ImGui.Button("Cancel")) ImGui.CloseCurrentPopup();

        ImGui.EndPopup();
    }

    private static void DrawTrackManagementPopup(TimelineEngine timeline)
    {
        if (!ImGui.BeginPopupModal("Manage Tracks")) return;

        for (int i = 0; i < timeline.Tracks.Count; i++)
        {
            Track track = timeline.Tracks[i];
            ImGui.PushID(i);
            ImGui.Separator();

            string name = track.Name;
            if (ImGui.InputText("Name", ref name, 64)) track.Name = name;

            bool visible = track.Visible;
            if (ImGui.Checkbox("Visible", ref visible)) track.Visible = visible;

            ImGui.SameLine();
            float height = track.Height;
            if (ImGui.SliderFloat("Height", ref height, 10.0f, 100.0f)) track.Height = height;

            Vector4 color = track.Background;
            if (ImGui.ColorEdit4("Color", ref color)) track.Background = color;

            if (Icons.IconLabelButton("TrackUp", Icon.Up, "Up") && i > 0)
            {
                (timeline.Tracks[i - 1], timeline.Tracks[i]) = (timeline.Tracks[i], timeline.Tracks[i - 1]);
            }

            ImGui.SameLine();
            if (Icons.IconLabelButton("TrackDown", Icon.Down, "Down") && i < timeline.Tracks.Count - 1)
            {
                (timeline.Tracks[i + 1], timeline.Tracks[i]) = (timeline.Tracks[i], timeline.Tracks[i + 1]);
            }

            ImGui.SameLine();
            if (Icons.IconLabelButton("TrackDelete", Icon.Delete, "Delete"))
            {
                timeline.Tracks.RemoveAt(i);
                ImGui.PopID();
                break;
            }

            ImGui.PopID();
        }

        ImGui.Separator();
        ImGui.Text("Add New Track");
        ImGui.InputText("New Track Name", ref _newTrackName, 64);

        ClipType[] allTypes = Enum.GetValues<ClipType>();
        for (int t = 0; t < allTypes.Length; t++)
        {
            ImGui.Checkbox(allTypes[t].ToString(), ref _newTrackTypeFlags[t]);
            if (t < allTypes.Length - 1) ImGui.SameLine();
        }

        if (ImGui.Button("Add Track"))
        {
            var types = new List<ClipType>();
            for (int t = 0; t < allTypes.Length; t++)
            {
                if (_newTrackTypeFlags[t]) types.Add(allTypes[t]);
            }

            if (types.Count > 0)
            {
                timeline.Tracks.Add(new Track(_newTrackName, types, 30.0f, new Vector4(0.2f, 0.2f, 0.2f, 1.0f)));
                Array.Clear(_newTrackTypeFlags);
            }
        }

        ImGui.Separator();
        if (ImGui.Button("Close")) ImGui.CloseCurrentPopup();

        ImGui.EndPopup();
    }
}
