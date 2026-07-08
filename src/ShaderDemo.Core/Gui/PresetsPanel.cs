// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;

namespace ShaderDemo.Core.Gui;

public static class PresetsPanel
{
    private static Dictionary<string, EffectParams>? _presets;
    private static string _newPresetName = "";
    private static readonly Random _random = new();

    public static void Draw(ShaderManager manager, string presetsFilePath)
    {
        _presets ??= PresetsService.Load(presetsFilePath);

        ImGui.InputText("New Preset Name", ref _newPresetName, 64);
        ImGui.SameLine();
        if (ImGui.Button("Save") && _newPresetName.Length > 0)
        {
            PresetsService.CreatePreset(_presets, _newPresetName, manager.Effects, presetsFilePath);
        }

        if (ImGui.Button("Randomize"))
        {
            string theme = PresetsService.GenerateRandomPreset(manager.Effects, _random);
            Console.WriteLine($"Random preset generated (theme: {theme})");
        }

        ImGui.Separator();

        foreach (string name in _presets.Keys.ToList())
        {
            ImGui.PushID(name);
            ImGui.Text(name);
            ImGui.SameLine();
            if (ImGui.SmallButton("Apply"))
            {
                PresetsService.ApplyPreset(_presets, name, manager.Effects);
            }

            ImGui.SameLine();
            if (ImGui.SmallButton("Delete"))
            {
                _presets.Remove(name);
                PresetsService.Save(_presets, presetsFilePath);
            }

            ImGui.PopID();
        }
    }
}
