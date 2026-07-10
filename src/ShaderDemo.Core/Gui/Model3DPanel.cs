// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Logging;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;

namespace ShaderDemo.Core.Gui;

public static class Model3DPanel
{
    internal static readonly string[] ObjExtensions = { ".obj" };
    private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".bmp" };
    private static string _modelPath = "models/cube.obj";
    private static string _texturePath = "";
    private static Dictionary<string, Model3DState>? _presets;
    private static string _newPresetName = "";
    private const string PresetsFilePath = "model_presets.json";

    public static void Draw(ShaderManager manager)
    {
        Model3DState state = manager.ModelState;

        ImGui.Checkbox("Show Model", ref state.ShowModel);

        ImGui.InputText("OBJ Path", ref _modelPath, 256);
        ImGui.SameLine();
        if (ImGui.Button("Browse...##Model"))
        {
            string filter = NativeFileDialog.BuildFilter(("OBJ Model Files", ObjExtensions));
            string? picked = NativeFileDialog.OpenFile("Select a 3D model file", filter, Path.GetDirectoryName(_modelPath));
            if (picked != null) _modelPath = picked;
        }

        FileValidationHint.Draw(_modelPath, ObjExtensions);
        if (ImGui.Button("Load Model"))
        {
            manager.Model.LoadModel(_modelPath, AppLog.Info);
            state.CurrentModelFilename = _modelPath;
            state.ShowModel = true;
            ToastManager.Show(manager.Model.Model != null ? $"Model loaded: {Path.GetFileName(_modelPath)}" : "Failed to load model", manager.Model.Model != null ? ToastLevel.Success : ToastLevel.Danger);
        }

        ImGui.InputText("Texture Path", ref _texturePath, 256);
        ImGui.SameLine();
        if (ImGui.Button("Browse...##ModelTexture"))
        {
            string filter = NativeFileDialog.BuildFilter(("Image Files", ImageExtensions));
            string? picked = NativeFileDialog.OpenFile("Select a texture image", filter, Path.GetDirectoryName(_texturePath));
            if (picked != null) _texturePath = picked;
        }

        FileValidationHint.Draw(_texturePath, ImageExtensions);
        ImGui.SameLine();
        if (ImGui.Button("Load Texture"))
        {
            manager.Model.LoadTexture(_texturePath, AppLog.Info);
            state.CurrentTextureFilename = _texturePath;
        }

        System.Numerics.Vector3 pos = state.Position;
        if (ImGui.SliderFloat3("Position", ref pos, -10.0f, 10.0f)) state.Position = pos;

        System.Numerics.Vector3 rot = state.Rotation;
        if (ImGui.SliderFloat3("Rotation", ref rot, -180.0f, 180.0f)) state.Rotation = rot;

        System.Numerics.Vector3 scale = state.Scale;
        if (ImGui.SliderFloat3("Scale", ref scale, 0.1f, 5.0f)) state.Scale = scale;

        System.Numerics.Vector3 autoRotate = state.AutoRotateSpeed;
        if (ImGui.SliderFloat3("Auto-Rotate Speed", ref autoRotate, -2.0f, 2.0f)) state.AutoRotateSpeed = autoRotate;

        System.Numerics.Vector3 lightDir = state.LightDir;
        if (ImGui.SliderFloat3("Light Direction", ref lightDir, -1.0f, 1.0f)) state.LightDir = lightDir;

        ImGui.Checkbox("Wireframe", ref state.Wireframe);
        if (state.Wireframe)
        {
            System.Numerics.Vector3 wireColor = state.WireframeColor;
            if (ImGui.ColorEdit3("Wireframe Color", ref wireColor)) state.WireframeColor = wireColor;
        }

        ImGui.Separator();
        ImGui.Text("Model Presets");
        _presets ??= ModelPresetsService.Load(PresetsFilePath);

        ImGui.InputText("New Model Preset Name", ref _newPresetName, 64);
        ImGui.SameLine();
        if (ImGui.Button("Save Preset") && _newPresetName.Length > 0)
        {
            ModelPresetsService.CreatePreset(_presets, _newPresetName, state, PresetsFilePath);
        }

        foreach (string name in _presets.Keys.ToList())
        {
            ImGui.PushID(name);
            ImGui.Text(name);
            ImGui.SameLine();
            if (ImGui.SmallButton("Apply")) ModelPresetsService.ApplyPreset(_presets, name, state);
            ImGui.SameLine();
            if (ImGui.SmallButton("Delete"))
            {
                _presets.Remove(name);
                ModelPresetsService.Save(_presets, PresetsFilePath);
            }

            ImGui.PopID();
        }
    }
}
