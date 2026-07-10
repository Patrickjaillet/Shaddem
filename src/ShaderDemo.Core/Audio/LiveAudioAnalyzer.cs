// Copyright (c) 2026 Patrick JAILLET
using NAudio.Dsp;
using NAudio.Wave;

namespace ShaderDemo.Core.Audio;

public sealed class LiveAudioAnalyzer : IDisposable
{
    private const int WindowSize = 1024;
    private const int SampleRate = 44100;
    private const float BassFreqMin = 20f;
    private const float BassFreqMax = 150f;
    private const float MidFreqMin = 150f;
    private const float MidFreqMax = 2000f;
    private const float TrebleFreqMin = 2000f;
    private const float TrebleFreqMax = 8000f;
    private const float PeakDecay = 0.98f;

    private readonly object _lock = new();
    private readonly float[] _ring = new float[WindowSize];
    private WaveInEvent? _waveIn;
    private int _ringPos;
    private int _ringFilled;
    private float _bassPeak = 0.0001f;
    private float _midPeak = 0.0001f;
    private float _treblePeak = 0.0001f;

    public float CurrentBass { get; private set; }
    public float CurrentMid { get; private set; }
    public float CurrentTreble { get; private set; }
    public bool IsCapturing { get; private set; }

    public static IEnumerable<(int DeviceNumber, string ProductName)> ListDevices()
    {
        for (int i = 0; i < WaveInEvent.DeviceCount; i++)
        {
            yield return (i, WaveInEvent.GetCapabilities(i).ProductName);
        }
    }

    public void Start(int deviceNumber = 0)
    {
        if (IsCapturing) return;

        _waveIn = new WaveInEvent
        {
            DeviceNumber = deviceNumber,
            WaveFormat = new WaveFormat(SampleRate, 16, 1),
            BufferMilliseconds = 20,
        };
        _waveIn.DataAvailable += OnDataAvailable;
        _waveIn.StartRecording();
        IsCapturing = true;
    }

    public void Stop()
    {
        if (_waveIn == null) return;

        _waveIn.DataAvailable -= OnDataAvailable;
        _waveIn.StopRecording();
        _waveIn.Dispose();
        _waveIn = null;
        IsCapturing = false;
        CurrentBass = 0f;
        CurrentMid = 0f;
        CurrentTreble = 0f;
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        int sampleCount = e.BytesRecorded / 2;

        lock (_lock)
        {
            for (int i = 0; i < sampleCount; i++)
            {
                short raw = BitConverter.ToInt16(e.Buffer, i * 2);
                _ring[_ringPos] = raw / 32768f;
                _ringPos = (_ringPos + 1) % WindowSize;
                if (_ringFilled < WindowSize) _ringFilled++;
            }

            if (_ringFilled >= WindowSize)
            {
                AnalyzeWindow();
            }
        }
    }

    private void AnalyzeWindow()
    {
        var data = new Complex[WindowSize];
        for (int k = 0; k < WindowSize; k++)
        {
            int idx = (_ringPos + k) % WindowSize;
            data[k].X = _ring[idx];
            data[k].Y = 0;
        }

        int m = (int)Math.Log2(WindowSize);
        FastFourierTransform.FFT(true, m, data);

        float freqResolution = (float)SampleRate / WindowSize;
        double bassSum = 0; int bassCount = 0;
        double midSum = 0; int midCount = 0;
        double trebleSum = 0; int trebleCount = 0;

        for (int k = 0; k <= WindowSize / 2; k++)
        {
            float freq = k * freqResolution;
            double mag = Math.Sqrt((data[k].X * data[k].X) + (data[k].Y * data[k].Y));

            if (freq >= BassFreqMin && freq <= BassFreqMax) { bassSum += mag; bassCount++; }
            if (freq >= MidFreqMin && freq <= MidFreqMax) { midSum += mag; midCount++; }
            if (freq >= TrebleFreqMin && freq <= TrebleFreqMax) { trebleSum += mag; trebleCount++; }
        }

        float bass = bassCount > 0 ? (float)(bassSum / bassCount) : 0f;
        float mid = midCount > 0 ? (float)(midSum / midCount) : 0f;
        float treble = trebleCount > 0 ? (float)(trebleSum / trebleCount) : 0f;

        _bassPeak = MathF.Max(bass, _bassPeak * PeakDecay);
        _midPeak = MathF.Max(mid, _midPeak * PeakDecay);
        _treblePeak = MathF.Max(treble, _treblePeak * PeakDecay);

        CurrentBass = Math.Clamp(bass / _bassPeak, 0f, 1f);
        CurrentMid = Math.Clamp(mid / _midPeak, 0f, 1f);
        CurrentTreble = Math.Clamp(treble / _treblePeak, 0f, 1f);
    }

    public void Dispose() => Stop();
}
