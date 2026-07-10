// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.Core.Gui;

public static class TimelinePanel
{
    private static readonly string[] ClipTypeNames = Enum.GetNames<ClipType>();
    private static readonly string[] LayerPropertyNames = { "Opacity", "Enabled" };

    private static float _newStart;
    private static float _newDuration = 10.0f;
    private static int _newTypeIndex = (int)ClipType.Shader;
    private static string _newResource = "";
    private static float _newValue;
    private static int _newLayerPropertyIndex;
    private static float _newLayerStartValue;
    private static float _newLayerEndValue = 1.0f;

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
        ImGui.Text("Automation (live parameter recording, independent of clips)");

        AutomationRecorder automation = manager.Automation;
        if (!automation.Recording)
        {
            if (ImGui.Button("Record Automation"))
            {
                automation.ToggleRecording(manager.Effects, manager.CurrentShaderIndex);
            }
        }
        else
        {
            if (ImGui.Button("Stop Recording")) automation.ToggleRecording(manager.Effects, manager.CurrentShaderIndex);
        }

        ImGui.SameLine();
        if (ImGui.Button(automation.Playing ? "Stop Playback" : "Play Automation")) automation.TogglePlayback();

        ImGui.Text($"Automation Time: {automation.Time:F1}s / {automation.Duration:F1}s");

        ImGui.Separator();
        TimelineCanvas.Draw(timeline, manager);

        ImGui.Separator();
        ImGui.Text("Markers");
        if (ImGui.Button("Add Marker at Playhead"))
        {
            timeline.AddMarker(manager.ElapsedTime, $"M{timeline.Markers.Count + 1}", new System.Numerics.Vector4(1.0f, 1.0f, 0.0f, 1.0f));
        }

        if (timeline.Markers.Count == 0)
        {
            ImGui.TextColored(Theme.TextMuted, "No markers yet. Markers flag a moment on the timeline (e.g. a drop or a scene change) without affecting playback.");
        }

        foreach (TimelineMarker marker in timeline.Markers.ToList())
        {
            ImGui.PushID(marker.GetHashCode());
            ImGui.Text($"{marker.Time:F1}s  {marker.Label}");
            ImGui.SameLine();
            if (Icons.IconLabelButton("RemoveMarker", Icon.Delete, "Remove")) timeline.RemoveMarker(marker.Time);
            ImGui.PopID();
        }

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
        else if (type == ClipType.Music)
        {
            ImGui.TextColored(Theme.TextMuted, "Resource = path to an audio file. Playback starts (seeked to the right offset) when this clip becomes active.");
            ImGui.InputFloat("Volume (0-1)", ref _newValue);
        }
        else if (type == ClipType.Model3D)
        {
            ImGui.TextColored(Theme.TextMuted, "Resource = path to an .obj model file.");
        }
        else if (type == ClipType.LayerAutomation)
        {
            string[] layerNames = manager.Layers.Where(l => !l.IsTimelineManaged).Select(l => l.Name).ToArray();
            int layerIndex = Array.IndexOf(layerNames, _newResource);
            if (layerNames.Length > 0 && ImGui.Combo("Target Layer", ref layerIndex, layerNames, layerNames.Length))
            {
                _newResource = layerNames[layerIndex];
            }
            else if (layerNames.Length == 0)
            {
                ImGui.TextColored(Theme.TextMuted, "No manually-added layers to animate yet — add one in the Layers panel first, and give it a name.");
            }

            ImGui.Combo("Property", ref _newLayerPropertyIndex, LayerPropertyNames, LayerPropertyNames.Length);
            ImGui.InputFloat("Start Value", ref _newLayerStartValue);
            ImGui.InputFloat("End Value", ref _newLayerEndValue);
        }

        if (ImGui.Button("Add") && _newResource.Length > 0)
        {
            Dictionary<string, object?>? clipParams = type switch
            {
                ClipType.Effect => new Dictionary<string, object?> { ["value"] = (double)_newValue },
                ClipType.Music => new Dictionary<string, object?> { ["volume"] = (double)_newValue },
                ClipType.LayerAutomation => new Dictionary<string, object?>
                {
                    ["property"] = LayerPropertyNames[_newLayerPropertyIndex].ToLowerInvariant(),
                    ["start_value"] = (double)_newLayerStartValue,
                    ["end_value"] = (double)_newLayerEndValue,
                },
                _ => null,
            };
            timeline.Add(_newStart, _newDuration, type, _newResource, clipParams);
        }

        ImGui.Separator();
        ImGui.Text($"Clips ({timeline.Clips.Count})");

        if (timeline.Clips.Count == 0)
        {
            ImGui.TextColored(Theme.TextMuted, "No clips yet. Fill in Start/Duration/Type/Resource above and click \"Add\", or drag directly on the canvas ruler once a clip exists.");
        }

        TimelineClip? toRemove = null;
        foreach (TimelineClip clip in timeline.Clips)
        {
            ImGui.PushID(clip.GetHashCode());
            ImGui.Text($"{clip.Start:F1}s - {clip.End:F1}s  [{clip.Type}]  {clip.Resource}");
            ImGui.SameLine();
            if (Icons.IconLabelButton("RemoveClip", Icon.Delete, "Remove")) toRemove = clip;
            ImGui.PopID();
        }

        if (toRemove != null) timeline.Remove(toRemove);
    }
}
