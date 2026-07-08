// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;

namespace ShaderDemo.Core.Gui;

public static class LayersPanel
{
    private static readonly string[] BlendModeNames =
    {
        "Replace", "Add", "Multiply", "Screen", "Overlay", "Normal", "Difference", "Exclusion",
    };

    public static void Draw(ShaderManager manager, string layersFilePath)
    {
        ImGui.Text("Effect stack (Bottom -> Top)");

        if (ImGui.Button("Add Layer") && manager.ShaderNames.Count > 0)
        {
            manager.Layers.Add(new Layer(manager.ShaderNames[0], BlendMode.Normal, 1.0f));
        }

        ImGui.SameLine();
        if (ImGui.Button("Save Layers"))
        {
            LayerPersistence.Save(manager.Layers, layersFilePath);
        }

        ImGui.SameLine();
        if (ImGui.Button("Load Layers"))
        {
            List<Layer> loaded = LayerPersistence.Load(layersFilePath);
            manager.Layers.Clear();
            manager.Layers.AddRange(loaded);
        }

        ImGui.Separator();

        int removeIndex = -1;
        int moveUp = -1;
        int moveDown = -1;

        for (int i = 0; i < manager.Layers.Count; i++)
        {
            Layer layer = manager.Layers[i];
            ImGui.PushID(i);

            if (i > 0 && ImGui.ArrowButton("##up", ImGuiDir.Up)) moveUp = i;
            ImGui.SameLine();
            if (i < manager.Layers.Count - 1 && ImGui.ArrowButton("##down", ImGuiDir.Down)) moveDown = i;
            ImGui.SameLine();

            bool keepOpen = true;
            bool expanded = ImGui.CollapsingHeader($"Layer {i + 1}: {layer.ShaderName}", ref keepOpen);
            if (!keepOpen) removeIndex = i;

            if (expanded)
            {
                bool enabled = layer.Enabled;
                if (ImGui.Checkbox("Enabled", ref enabled)) layer.Enabled = enabled;

                string[] shaderNames = manager.ShaderNames.ToArray();
                int shaderIndex = Array.IndexOf(shaderNames, layer.ShaderName);
                if (shaderIndex < 0) shaderIndex = 0;
                if (shaderNames.Length > 0 && ImGui.Combo("Shader", ref shaderIndex, shaderNames, shaderNames.Length))
                {
                    layer.ShaderName = shaderNames[shaderIndex];
                }

                int blendIndex = (int)layer.BlendMode;
                if (ImGui.Combo("Blend Mode", ref blendIndex, BlendModeNames, BlendModeNames.Length))
                {
                    layer.BlendMode = (BlendMode)blendIndex;
                }

                float opacity = layer.Opacity;
                if (ImGui.SliderFloat("Opacity", ref opacity, 0.0f, 1.0f)) layer.Opacity = opacity;
            }

            ImGui.PopID();
            ImGui.Separator();
        }

        if (moveUp > 0)
        {
            (manager.Layers[moveUp - 1], manager.Layers[moveUp]) = (manager.Layers[moveUp], manager.Layers[moveUp - 1]);
        }

        if (moveDown >= 0 && moveDown < manager.Layers.Count - 1)
        {
            (manager.Layers[moveDown + 1], manager.Layers[moveDown]) = (manager.Layers[moveDown], manager.Layers[moveDown + 1]);
        }

        if (removeIndex >= 0)
        {
            manager.Layers.RemoveAt(removeIndex);
        }
    }
}
