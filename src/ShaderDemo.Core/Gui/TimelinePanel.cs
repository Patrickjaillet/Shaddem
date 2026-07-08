// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.Core.Gui;

public static class TimelinePanel
{
    private static readonly string[] ClipTypeNames = Enum.GetNames<ClipType>();

    private static float _newStart;
    private static float _newDuration = 10.0f;
    private static int _newTypeIndex = (int)ClipType.Shader;
    private static string _newResource = "";
    private static float _newValue;

    public static void Draw(TimelineEngine timeline, ShaderManager manager, string timelineFilePath)
    {
        bool active = timeline.Active;
        if (ImGui.Checkbox("Enable Timeline", ref active)) timeline.Active = active;

        ImGui.Text($"Playhead: {manager.ElapsedTime:F2}s");

        if (ImGui.Button("Undo")) timeline.Undo();
        ImGui.SameLine();
        if (ImGui.Button("Redo")) timeline.Redo();
        ImGui.SameLine();
        if (ImGui.Button("Save")) TimelinePersistence.Save(timeline, timelineFilePath);
        ImGui.SameLine();
        if (ImGui.Button("Load")) TimelinePersistence.Load(timeline, timelineFilePath);

        ImGui.Separator();
        ImGui.Text("Add clip");
        ImGui.InputFloat("Start (s)", ref _newStart);
        ImGui.InputFloat("Duration (s)", ref _newDuration);
        ImGui.Combo("Type", ref _newTypeIndex, ClipTypeNames, ClipTypeNames.Length);
        ImGui.InputText("Resource", ref _newResource, 128);

        var type = (ClipType)_newTypeIndex;
        if (type == ClipType.Shader)
        {
            string[] shaderNames = manager.ShaderNames.ToArray();
            int shaderIndex = Array.IndexOf(shaderNames, _newResource);
            if (shaderNames.Length > 0 && ImGui.Combo("Shader Resource", ref shaderIndex, shaderNames, shaderNames.Length))
            {
                _newResource = shaderNames[shaderIndex];
            }
        }
        else if (type == ClipType.Effect)
        {
            ImGui.InputFloat("Value", ref _newValue);
        }

        if (ImGui.Button("Add") && _newResource.Length > 0)
        {
            var clipParams = type == ClipType.Effect
                ? new Dictionary<string, object?> { ["value"] = (double)_newValue }
                : null;
            timeline.Add(_newStart, _newDuration, type, _newResource, clipParams);
        }

        ImGui.Separator();
        ImGui.Text($"Clips ({timeline.Clips.Count})");

        TimelineClip? toRemove = null;
        foreach (TimelineClip clip in timeline.Clips)
        {
            ImGui.PushID(clip.GetHashCode());
            ImGui.Text($"{clip.Start:F1}s - {clip.End:F1}s  [{clip.Type}]  {clip.Resource}");
            ImGui.SameLine();
            if (ImGui.SmallButton("Remove")) toRemove = clip;
            ImGui.PopID();
        }

        if (toRemove != null) timeline.Remove(toRemove);
    }
}
