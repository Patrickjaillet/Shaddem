// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.App;

public static class FramePacingTest
{
    public static void Run()
    {
        using var window = new GlWindow(AppInfo.Name, 320, 240, fullscreen: false, vsync: true, enableImGui: false);
        bool hadFailure = false;

        window.Load += () =>
        {
            Console.WriteLine($"[frame-pacing-test] Initial: VSyncEnabled={window.VSyncEnabled}, IsThrottled={window.IsThrottled}");
            if (!window.VSyncEnabled)
            {
                hadFailure = true;
                Console.WriteLine("[frame-pacing-test] FAIL: expected VSyncEnabled=true initially (constructor arg was true)");
            }

            if (window.IsThrottled)
            {
                hadFailure = true;
                Console.WriteLine("[frame-pacing-test] FAIL: expected IsThrottled=false on a freshly created, focused window");
            }

            window.SetVSync(false);
            Console.WriteLine($"[frame-pacing-test] After SetVSync(false): VSyncEnabled={window.VSyncEnabled}");
            if (window.VSyncEnabled)
            {
                hadFailure = true;
                Console.WriteLine("[frame-pacing-test] FAIL: SetVSync(false) did not update VSyncEnabled");
            }

            window.SetVSync(true);
            Console.WriteLine($"[frame-pacing-test] After SetVSync(true): VSyncEnabled={window.VSyncEnabled}");
            if (!window.VSyncEnabled)
            {
                hadFailure = true;
                Console.WriteLine("[frame-pacing-test] FAIL: SetVSync(true) did not update VSyncEnabled");
            }

            Console.WriteLine("[frame-pacing-test] NOTE: focus/minimize throttling (IsThrottled flipping true) is driven by real OS window-manager events (Silk.NET IWindow.FocusChanged/StateChanged) and cannot be synthesized in this headless harness — verified by code review against the same event-subscription pattern already proven for Resize/Closing in this file, not by triggering a real OS focus-loss here.");

            Console.WriteLine(hadFailure ? "[frame-pacing-test] RESULT: FAIL" : "[frame-pacing-test] RESULT: PASS");
            window.RequestClose();
        };

        window.Run();
    }
}
