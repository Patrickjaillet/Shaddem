// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core.Audio;

namespace ShaderDemo.App;

public static class AudioAnalysisTest
{
    public static void Run(string filePath)
    {
        Console.WriteLine($"[audio-test] Analyzing '{filePath}' (supported: {AudioAnalyzer.IsSupportedFile(filePath)})");

        (float[] bass, float[] mid, float[] treble, double duration) = AudioAnalyzer.Analyze(filePath);
        Console.WriteLine($"[audio-test] Duration={duration:F2}s Bass={bass.Length} Mid={mid.Length} Treble={treble.Length} frames");

        if (bass.Length == 0)
        {
            Console.WriteLine("[audio-test] FAILED: empty envelope");
            return;
        }

        int third = bass.Length / 3;
        float AvgBass(int s, int e) => bass[s..e].DefaultIfEmpty(0f).Average();
        float AvgMid(int s, int e) => mid[s..e].DefaultIfEmpty(0f).Average();
        float AvgTreble(int s, int e) => treble[s..e].DefaultIfEmpty(0f).Average();

        Console.WriteLine($"[audio-test] Segment 1 (bass tone)   -> bass={AvgBass(0, third):F3} mid={AvgMid(0, third):F3} treble={AvgTreble(0, third):F3}");
        Console.WriteLine($"[audio-test] Segment 2 (mid tone)    -> bass={AvgBass(third, 2 * third):F3} mid={AvgMid(third, 2 * third):F3} treble={AvgTreble(third, 2 * third):F3}");
        Console.WriteLine($"[audio-test] Segment 3 (treble tone) -> bass={AvgBass(2 * third, bass.Length):F3} mid={AvgMid(2 * third, bass.Length):F3} treble={AvgTreble(2 * third, bass.Length):F3}");

        var full = AudioAnalyzer.AnalyzeFull(filePath);
        Console.WriteLine($"[audio-test] AnalyzeFull: Bass={full.Bass.Length} Treble={full.Treble.Length} Spectrum={full.Spectrum.Length} Waveform={full.Waveform.Length} Duration={full.Duration:F2}");

        Console.WriteLine("[audio-test] Done.");
    }

    public static void RunLiveDeviceCheck()
    {
        var devices = LiveAudioAnalyzer.ListDevices().ToArray();
        Console.WriteLine($"[audio-test] Found {devices.Length} input device(s)");
        foreach (var (index, name) in devices)
        {
            Console.WriteLine($"[audio-test]  #{index}: {name}");
        }

        if (devices.Length == 0)
        {
            Console.WriteLine("[audio-test] No capture device available on this machine, skipping live capture test.");
            return;
        }

        using var live = new LiveAudioAnalyzer();
        live.Start(0);
        Console.WriteLine($"[audio-test] Live capture started: IsCapturing={live.IsCapturing}");
        Thread.Sleep(1500);
        Console.WriteLine($"[audio-test] Live values -> Bass={live.CurrentBass:F3} Mid={live.CurrentMid:F3} Treble={live.CurrentTreble:F3}");
        live.Stop();
        Console.WriteLine($"[audio-test] Live capture stopped: IsCapturing={live.IsCapturing}");
    }
}
