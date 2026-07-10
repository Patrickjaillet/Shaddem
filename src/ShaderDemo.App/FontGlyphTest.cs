// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core;
using ShaderDemo.Core.Gui;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.App;

public static class FontGlyphTest
{
    public static void Run()
    {
        using var window = new GlWindow(AppInfo.Name, 320, 240, fullscreen: false);

        window.Load += () =>
        {
            string[] icons = { Theme.Icons.Stop, Theme.Icons.Record, Theme.Icons.Up, Theme.Icons.Down, Theme.Icons.Close, Theme.Icons.Bullet };
            ImFontPtr font = Theme.FontRegular ?? ImGui.GetIO().Fonts.Fonts[0];

            Console.WriteLine($"[font-test] FontRegular loaded: {Theme.FontRegular != null}");
            Console.WriteLine($"[font-test] FontSemibold loaded: {Theme.FontSemibold != null}");
            Console.WriteLine($"[font-test] FontMono loaded: {Theme.FontMono != null}");

            foreach (string icon in icons)
            {
                int codepoint = char.ConvertToUtf32(icon, 0);
                ImFontGlyphPtr glyph = font.FindGlyphNoFallback((ushort)codepoint);
                bool found;
                unsafe { found = glyph.NativePtr != null; }
                Console.WriteLine($"[font-test] Icon U+{codepoint:X4} '{icon}': glyph found = {found}");
            }

            Environment.Exit(0);
        };

        window.Run();
    }
}
