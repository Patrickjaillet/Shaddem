// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Gui;

public static class Model3DPanel
{
    private static string _modelPath = "models/cube.obj";
    private static string _texturePath = "";

    public static void Draw(ShaderManager manager)
    {
        Model3DState state = manager.ModelState;

        ImGui.Checkbox("Show Model", ref state.ShowModel);

        ImGui.InputText("OBJ Path", ref _modelPath, 256);
        if (ImGui.Button("Load Model"))
        {
            manager.Model.LoadModel(_modelPath, Console.WriteLine);
            state.CurrentModelFilename = _modelPath;
            state.ShowModel = true;
        }

        ImGui.InputText("Texture Path", ref _texturePath, 256);
        ImGui.SameLine();
        if (ImGui.Button("Load Texture"))
        {
            manager.Model.LoadTexture(_texturePath, Console.WriteLine);
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
    }
}
