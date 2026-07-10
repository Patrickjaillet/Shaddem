// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core;
using ShaderDemo.Core.Export;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.App;

public static class ProjectFeaturesTest
{
    public static void Run()
    {
        TestAutomation();
        TestSrtExport();
        TestAudioExportFailureHandling();
        Console.WriteLine("[project-features-test] Done.");
    }

    private static void TestAudioExportFailureHandling()
    {
        string missingSource = Path.Combine(Path.GetTempPath(), "shaderdemo_does_not_exist.wav");
        bool result1 = AudioExporter.ExportAudioOnly("ffmpeg", missingSource, "out.wav", null, msg => Console.WriteLine($"[project-features-test] AudioExporter (missing source): {msg}"));
        Console.WriteLine($"[project-features-test] Missing-source export returned {result1} (expect false, no exception)");

        string realSource = Path.Combine(Path.GetTempPath(), "shaderdemo_probe.wav");
        File.WriteAllBytes(realSource, new byte[44]);
        bool result2 = AudioExporter.ExportAudioOnly("ffmpeg_definitely_not_on_path", realSource, "out.wav", null, msg => Console.WriteLine($"[project-features-test] AudioExporter (missing ffmpeg): {msg}"));
        File.Delete(realSource);
        Console.WriteLine($"[project-features-test] Missing-ffmpeg export returned {result2} (expect false, no exception)");
    }

    private static void TestAutomation()
    {
        using var window = new GlWindow(AppInfo.Name, 320, 240, fullscreen: false);
        ShaderManager? engine = null;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            engine.RegisterShader("A", "void mainImage(out vec4 fragColor, in vec2 fragCoord) { fragColor = vec4(1.0); }");
            engine.RegisterShader("B", "void mainImage(out vec4 fragColor, in vec2 fragCoord) { fragColor = vec4(0.5); }");
            engine.SelectShader(0);

            engine.Automation.ToggleRecording(engine.Effects, engine.CurrentShaderIndex);
            engine.Effects.Speed = 2.0f;
            engine.Automation.Update(0.5f, engine.Effects, engine);
            engine.Effects.Speed = 4.0f;
            engine.SelectShader(1);
            engine.Automation.Update(0.5f, engine.Effects, engine);
            engine.Automation.ToggleRecording(engine.Effects, engine.CurrentShaderIndex);

            Console.WriteLine($"[project-features-test] Recorded automation duration={engine.Automation.Duration:F2}s (expected ~1.0s)");

            engine.Effects.Speed = 0f;
            engine.SelectShader(0);
            engine.Automation.TogglePlayback();

            engine.Automation.Update(0.25f, engine.Effects, engine);
            Console.WriteLine($"[project-features-test] Playback t=0.25s Speed={engine.Effects.Speed:F2} (expect between 1.0 and 2.0)");

            engine.Automation.Update(0.5f, engine.Effects, engine);
            Console.WriteLine($"[project-features-test] Playback t=0.75s Speed={engine.Effects.Speed:F2} ShaderIndex={engine.CurrentShaderIndex} (expect Speed near 3.0, index=1)");

            window.RequestClose();
        };

        window.Closing += () => engine?.Dispose();
        window.Run();
    }

    private static void TestSrtExport()
    {
        var timeline = new TimelineEngine();
        timeline.Add(1.0, 3.0, ClipType.Text, "Hello world");
        timeline.Add(5.0, 2.0, ClipType.Text, "Second line");
        timeline.Add(0.0, 10.0, ClipType.Shader, "unrelated.glsl");

        string path = Path.Combine(Path.GetTempPath(), "shaderdemo_test.srt");
        int count = SrtExporter.Export(timeline, path);
        string content = File.ReadAllText(path);
        File.Delete(path);

        Console.WriteLine($"[project-features-test] SRT export: {count} entries (expect 2)");
        Console.WriteLine(content);
    }
}
