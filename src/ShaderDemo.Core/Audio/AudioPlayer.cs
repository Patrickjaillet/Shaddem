// Copyright (c) 2026 Patrick JAILLET
using NAudio.Wave;

namespace ShaderDemo.Core.Audio;

public sealed class AudioPlayer : IDisposable
{
    private WaveOutEvent? _output;
    private AudioFileReader? _reader;

    public bool IsPlaying => _output?.PlaybackState == PlaybackState.Playing;
    public string? CurrentFilePath { get; private set; }

    public void Play(string filePath, float volume, TimeSpan startOffset = default)
    {
        Stop();
        _reader = new AudioFileReader(filePath) { Volume = Math.Clamp(volume, 0f, 1f) };
        if (startOffset > TimeSpan.Zero && startOffset < _reader.TotalTime)
        {
            _reader.CurrentTime = startOffset;
        }

        _output = new WaveOutEvent();
        _output.Init(_reader);
        _output.Play();
        CurrentFilePath = filePath;
    }

    public void SetVolume(float volume)
    {
        if (_reader != null) _reader.Volume = Math.Clamp(volume, 0f, 1f);
    }

    public void Stop()
    {
        _output?.Stop();
        _output?.Dispose();
        _output = null;
        _reader?.Dispose();
        _reader = null;
        CurrentFilePath = null;
    }

    public void Dispose()
    {
        Stop();
    }
}
