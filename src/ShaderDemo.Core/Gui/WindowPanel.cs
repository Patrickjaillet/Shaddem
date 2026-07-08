// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Gui;

public static class WindowPanel
{
    public static void Draw(SecondaryWindow preview, ShaderManager manager)
    {
        ImGui.Text("Secondary output window (mirrors the composed frame)");

        if (!preview.IsOpen)
        {
            if (ImGui.Button("Open Preview Window"))
            {
                preview.Open("ShaderDemo Preview", manager.Pipeline.Width, manager.Pipeline.Height, Console.WriteLine);
            }
        }
        else
        {
            if (ImGui.Button("Close Preview Window"))
            {
                preview.Close();
            }
        }
    }
}
