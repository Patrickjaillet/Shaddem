// Copyright (c) 2026 Patrick JAILLET
using System.Diagnostics;
using ShaderDemo.Core;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.App;

public static class VideoExportTest
{
    public static void Run(string ffmpegPath, string shaderDirectory, bool isGif = false)
    {
        using var window = new GlWindow(AppInfo.Name, 320, 240, fullscreen: false);
        ShaderManager? engine = null;
        int frame = 0;
        const int width = 320;
        const int height = 240;
        const int fps = 30;
        const int totalFrames = 60;
        string outputPath = Path.Combine(Path.GetTempPath(), isGif ? "shaderdemo_export_test.gif" : "shaderdemo_export_test.mp4");

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, width, height);
            ShaderLoader.LoadFromDirectory(engine, shaderDirectory, Console.WriteLine);
            engine.SelectShader(0);
            engine.Recorder.FfmpegPath = ffmpegPath;

            if (File.Exists(outputPath)) File.Delete(outputPath);
            bool started = engine.Recorder.Start(width, height, fps, null, outputPath, null, 0.0, includeAudio: false, isGif, Console.WriteLine);
            Console.WriteLine($"[video-export-test] Recording started: {started}");
        };

        window.UpdateFrame += dt => engine?.Update(dt);
        window.RenderFrame += _ =>
        {
            engine?.RenderFrame();
            frame++;

            if (frame == totalFrames)
            {
                engine?.Recorder.Stop();
                Console.WriteLine("[video-export-test] Recording stopped.");
                window.RequestClose();
            }
        };

        window.Closing += () => engine?.Dispose();
        window.Run();

        if (!File.Exists(outputPath))
        {
            Console.WriteLine("[video-export-test] FAILED: output file not created.");
            return;
        }

        long fileSize = new FileInfo(outputPath).Length;
        Console.WriteLine($"[video-export-test] Output file: {outputPath} ({fileSize} bytes)");

        var startInfo = new ProcessStartInfo
        {
            FileName = ffmpegPath,
            ArgumentList = { "-i", outputPath },
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using Process? probe = Process.Start(startInfo);
        string probeOutput = probe?.StandardError.ReadToEnd() ?? "";
        probe?.WaitForExit();

        Console.WriteLine($"[video-export-test] Expected: {width}x{height} @ {fps}fps, duration ~{totalFrames / (double)fps:F2}s");
        foreach (string line in probeOutput.Split('\n'))
        {
            if (line.Contains("Video:") || line.Contains("Duration:"))
            {
                Console.WriteLine($"[video-export-test] ffmpeg reports: {line.Trim()}");
            }
        }

        File.Delete(outputPath);
        Console.WriteLine("[video-export-test] Done.");
    }
}
