// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Gui;

public static class MediaPanel
{
    private static string _texturePath = "";

    public static void Draw(ShaderManager manager)
    {
        ImGui.Text("iChannel0 Texture");
        ImGui.InputText("Image Path", ref _texturePath, 512);

        if (ImGui.Button("Load"))
        {
            manager.LoadChannel0Texture(_texturePath, Console.WriteLine);
        }

        ImGui.Text($"Current: {manager.Channel0Texture.Width}x{manager.Channel0Texture.Height}");
    }
}
