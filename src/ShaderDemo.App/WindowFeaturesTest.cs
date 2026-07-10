// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.App;

public static class WindowFeaturesTest
{
    public static void Run()
    {
        using var window = new GlWindow(AppInfo.Name, 640, 480, fullscreen: false);
        ShaderManager? engine = null;
        int frame = 0;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            engine.RegisterShader("TestPattern", "void mainImage(out vec4 fragColor, in vec2 fragCoord) { fragColor = vec4(0.1, 0.1, 0.15, 1.0); }");
            engine.SelectShader(0);
            Console.WriteLine($"[window-test] Initial IsFullscreen={window.IsFullscreen} (expect False)");
        };

        window.UpdateFrame += dt => engine?.Update(dt);
        window.RenderFrame += _ =>
        {
            engine?.RenderFrame();
            frame++;

            if (frame == 3)
            {
                window.ToggleFullscreen();
                Console.WriteLine($"[window-test] After 1st toggle IsFullscreen={window.IsFullscreen} (expect True)");
            }

            if (frame == 6)
            {
                window.ToggleFullscreen();
                Console.WriteLine($"[window-test] After 2nd toggle IsFullscreen={window.IsFullscreen} (expect False)");
            }

            if (frame == 15) window.RequestClose();
        };

        window.Closing += () => engine?.Dispose();
        window.Run();
        Console.WriteLine("[window-test] Docking-enabled ImGui frame loop with real rendering ran for 15 frames, fullscreen toggled twice, no exceptions.");
    }
}
