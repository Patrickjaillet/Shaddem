// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using ImGuiNET;
using ShaderDemo.Core;
using ShaderDemo.Core.Export;
using ShaderDemo.Core.Gui;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.App;

public static class TimelineUiTest
{
    public static void Run()
    {
        using var window = new GlWindow(AppInfo.Name, 900, 700, fullscreen: false);
        ShaderManager? engine = null;
        var timeline = new TimelineEngine();
        int frame = 0;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            engine.RegisterShader("TestPattern", "void mainImage(out vec4 fragColor, in vec2 fragCoord) { fragColor = vec4(0.1, 0.1, 0.15, 1.0); }");
            engine.SelectShader(0);

            timeline.Add(0, 10, ClipType.Shader, "matrix_rain.hlsl");
            timeline.Add(10, 20, ClipType.Shader, "sea_glass.hlsl");
            timeline.Add(2, 4, ClipType.Text, "DEMO SHADER");
            timeline.Add(0, 30, ClipType.Music, "audio/music.wav");
            timeline.Add(5, 5, ClipType.Effect, "speed", new Dictionary<string, object?> { ["value"] = 2.0 });
            timeline.AddMarker(8, "Drop", new Vector4(1, 1, 0, 1));
            timeline.AddMarker(18, "Break", new Vector4(0, 1, 1, 1));
            Console.WriteLine("[timeline-ui-test] Sample clips and markers added");
        };

        bool hadException = false;

        window.UpdateFrame += dt => engine?.Update(dt);
        window.RenderFrame += _ => engine?.RenderFrame();
        window.ImGuiRender += () =>
        {
            if (engine == null) return;

            try
            {
                ImGui.SetNextWindowSize(new Vector2(860, 400), ImGuiCond.Always);
                ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always);
                ImGui.Begin("Timeline UI Test");
                TimelineCanvas.Draw(timeline, engine);
                ImGui.End();

                if (frame == 20)
                {
                    string path = ScreenshotService.Save(engine.LastComposedFrame ?? engine.Pipeline.SceneFbo, "screenshots");
                    Console.WriteLine($"[timeline-ui-test] Screenshot saved: {path}");
                }
            }
            catch (Exception ex)
            {
                hadException = true;
                Console.WriteLine($"[timeline-ui-test] EXCEPTION: {ex}");
            }

            frame++;
            if (frame == 30) window.RequestClose();
        };
        window.Closing += () =>
        {
            engine?.Dispose();
            Console.WriteLine(hadException
                ? "[timeline-ui-test] RESULT: FAIL (see exception(s) above)"
                : "[timeline-ui-test] RESULT: PASS (30 frames, TimelineCanvas drew clips/markers/playhead with no exceptions)");
        };
        window.Run();
    }
}
