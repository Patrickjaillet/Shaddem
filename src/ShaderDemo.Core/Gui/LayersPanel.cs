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

    private static readonly string[] FitModeNames =
    {
        "Stretch", "Fit", "Fill", "Center",
    };

    private static string _browseDirectory = "media";

    public static void Draw(ShaderManager manager, string layersFilePath)
    {
        MediaPanel._asyncLoader.UploadReady(manager.Gl, (path, texture) => MediaPanel._thumbnailCache[path] = texture);

        ImGui.Text("Effect stack (Bottom -> Top)");

        int maxDimension = manager.MaxLayerImageDimension;
        if (ImGui.SliderInt("Image Layer Max Size (px)", ref maxDimension, 512, 4096))
        {
            manager.MaxLayerImageDimension = maxDimension;
        }

        ImGui.TextColored(Theme.TextMuted, $"Source images larger than this are downscaled on load to limit VRAM use. Estimated VRAM: {GpuResourceTracker.EstimatedVramBytes / (1024.0 * 1024.0):F1} MB");

        if (ImGui.Button("Add Layer") && manager.ShaderNames.Count > 0)
        {
            manager.Layers.Add(new Layer(manager.ShaderNames[0], BlendMode.Normal, 1.0f));
        }

        ImGui.SameLine();
        if (ImGui.Button("Add Image Layer"))
        {
            string filter = NativeFileDialog.BuildFilter(("Image Files", MediaPanel.ImageExtensions));
            string? picked = NativeFileDialog.OpenFile("Select an image layer", filter, _browseDirectory);
            if (picked != null)
            {
                _browseDirectory = Path.GetDirectoryName(picked) ?? _browseDirectory;
                manager.Layers.Add(Layer.CreateImage(picked, BlendMode.Normal, 1.0f));
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("Add Model Layer"))
        {
            string modelFilter = NativeFileDialog.BuildFilter(("OBJ Model Files", Model3DPanel.ObjExtensions));
            string? pickedModel = NativeFileDialog.OpenFile("Select a 3D model layer", modelFilter, _browseDirectory);
            if (pickedModel != null)
            {
                _browseDirectory = Path.GetDirectoryName(pickedModel) ?? _browseDirectory;
                manager.Layers.Add(Layer.CreateModel3D(pickedModel, null, BlendMode.Normal, 1.0f));
            }
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

        if (manager.Layers.Count == 0)
        {
            ImGui.TextColored(Theme.TextMuted, "No layers yet. Click \"Add Layer\" to stack another shader on top of the current one with a blend mode.");
        }

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
            string kindTag = layer.SourceType.ToString();
            if (layer.IsTimelineManaged) kindTag += ", Timeline";
            bool expanded = ImGui.CollapsingHeader($"Layer {i + 1} [{kindTag}]: {layer.DisplayName}", ref keepOpen);
            if (!keepOpen) removeIndex = i;

            if (expanded)
            {
                if (layer.IsTimelineManaged)
                {
                    string notice = layer.SourceType switch
                    {
                        LayerSourceType.Text => "Driven by a Text clip on the Timeline. Edit the clip to change its caption, timing or fades.",
                        LayerSourceType.Model3D => "Driven by a Model3D clip on the Timeline. Edit the clip to change its model, timing or fades.",
                        _ => "Driven by an Image clip on the Timeline. Edit the clip to change its image, timing or fades.",
                    };
                    ImGui.TextColored(Theme.TextMuted, notice);
                }
                else
                {
                    string name = layer.Name;
                    if (ImGui.InputText("Layer Name (for Timeline automation)", ref name, 64) && name.Length > 0) layer.Name = name;
                }

                bool enabled = layer.Enabled;
                if (ImGui.Checkbox("Enabled", ref enabled)) layer.Enabled = enabled;

                switch (layer.SourceType)
                {
                    case LayerSourceType.Image:
                        DrawImageLayerEditor(manager, layer);
                        break;
                    case LayerSourceType.Model3D:
                        DrawModel3DLayerEditor(layer);
                        break;
                    case LayerSourceType.Text:
                        ImGui.TextColored(Theme.TextMuted, "Caption content, font size and color are set on the Timeline clip.");
                        break;
                    default:
                        string[] shaderNames = manager.ShaderNames.ToArray();
                        int shaderIndex = Array.IndexOf(shaderNames, layer.ShaderName);
                        if (shaderIndex < 0) shaderIndex = 0;
                        if (shaderNames.Length > 0 && ImGui.Combo("Shader", ref shaderIndex, shaderNames, shaderNames.Length))
                        {
                            layer.ShaderName = shaderNames[shaderIndex];
                        }

                        break;
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

    private static void DrawImageLayerEditor(ShaderManager manager, Layer layer)
    {
        Texture? thumb = string.IsNullOrEmpty(layer.ImagePath) ? null : MediaPanel.GetThumbnail(layer.ImagePath);
        if (thumb != null)
        {
            ImGui.Image((nint)thumb.Handle, new System.Numerics.Vector2(64, 64));
            ImGui.SameLine();
        }

        ImGui.BeginGroup();
        ImGui.TextWrapped(string.IsNullOrEmpty(layer.ImagePath) ? "(no image)" : layer.ImagePath);
        if (ImGui.Button("Change Image..."))
        {
            string filter = NativeFileDialog.BuildFilter(("Image Files", MediaPanel.ImageExtensions));
            string? picked = NativeFileDialog.OpenFile("Select an image layer", filter, _browseDirectory);
            if (picked != null)
            {
                _browseDirectory = Path.GetDirectoryName(picked) ?? _browseDirectory;
                layer.ImagePath = picked;
            }
        }

        ImGui.EndGroup();

        int fitIndex = (int)layer.FitMode;
        if (ImGui.Combo("Fit Mode", ref fitIndex, FitModeNames, FitModeNames.Length))
        {
            layer.FitMode = (ImageFitMode)fitIndex;
        }

        float posX = layer.PositionX;
        float posY = layer.PositionY;
        if (ImGui.SliderFloat("Position X", ref posX, -1.0f, 1.0f)) layer.PositionX = posX;
        if (ImGui.SliderFloat("Position Y", ref posY, -1.0f, 1.0f)) layer.PositionY = posY;

        float scale = layer.Scale;
        if (ImGui.SliderFloat("Scale", ref scale, 0.1f, 3.0f)) layer.Scale = scale;

        float rotationDegrees = layer.Rotation * (180.0f / MathF.PI);
        if (ImGui.SliderFloat("Rotation", ref rotationDegrees, -180.0f, 180.0f))
        {
            layer.Rotation = rotationDegrees * (MathF.PI / 180.0f);
        }
    }

    private static void DrawModel3DLayerEditor(Layer layer)
    {
        Model3DState state = layer.ModelState;

        ImGui.TextWrapped(string.IsNullOrEmpty(state.CurrentModelFilename) ? "(no model)" : state.CurrentModelFilename);
        if (ImGui.Button("Change Model..."))
        {
            string filter = NativeFileDialog.BuildFilter(("OBJ Model Files", Model3DPanel.ObjExtensions));
            string? picked = NativeFileDialog.OpenFile("Select a 3D model layer", filter, _browseDirectory);
            if (picked != null)
            {
                _browseDirectory = Path.GetDirectoryName(picked) ?? _browseDirectory;
                state.CurrentModelFilename = picked;
            }
        }

        ImGui.TextWrapped(string.IsNullOrEmpty(state.CurrentTextureFilename) ? "(no texture — uses shader iChannel0 as fallback)" : state.CurrentTextureFilename);
        ImGui.SameLine();
        if (ImGui.Button("Change Texture..."))
        {
            string filter = NativeFileDialog.BuildFilter(("Image Files", MediaPanel.ImageExtensions));
            string? picked = NativeFileDialog.OpenFile("Select a model texture", filter, _browseDirectory);
            if (picked != null)
            {
                _browseDirectory = Path.GetDirectoryName(picked) ?? _browseDirectory;
                state.CurrentTextureFilename = picked;
            }
        }

        System.Numerics.Vector3 pos = state.Position;
        if (ImGui.SliderFloat3("Position", ref pos, -10.0f, 10.0f)) state.Position = pos;

        System.Numerics.Vector3 rot = state.Rotation;
        if (ImGui.SliderFloat3("Rotation", ref rot, -180.0f, 180.0f)) state.Rotation = rot;

        System.Numerics.Vector3 scale = state.Scale;
        if (ImGui.SliderFloat3("Scale", ref scale, 0.1f, 5.0f)) state.Scale = scale;

        System.Numerics.Vector3 autoRotate = state.AutoRotateSpeed;
        if (ImGui.SliderFloat3("Auto-Rotate Speed", ref autoRotate, -2.0f, 2.0f)) state.AutoRotateSpeed = autoRotate;

        ImGui.Checkbox("Wireframe", ref state.Wireframe);
        if (state.Wireframe)
        {
            System.Numerics.Vector3 wireColor = state.WireframeColor;
            if (ImGui.ColorEdit3("Wireframe Color", ref wireColor)) state.WireframeColor = wireColor;
        }
    }
}
