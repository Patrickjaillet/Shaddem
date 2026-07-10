// Copyright (c) 2026 Patrick JAILLET
using NAudio.Dsp;
using NAudio.Vorbis;
using NAudio.Wave;

namespace ShaderDemo.Core.Audio;

public static class AudioAnalyzer
{
    private const int WindowSize = 1024;
    private const int StepSize = 512;
    private const float BassFreqMin = 20f;
    private const float BassFreqMax = 150f;
    private const float MidFreqMin = 150f;
    private const float MidFreqMax = 2000f;

    private static readonly string[] SupportedExtensions = { ".wav", ".mp3", ".ogg", ".flac", ".aac", ".wma", ".m4a" };

    public static bool IsSupportedFile(string filePath) =>
        Array.IndexOf(SupportedExtensions, Path.GetExtension(filePath).ToLowerInvariant()) >= 0;

    public static (float[] BassEnvelope, float[] MidEnvelope, float[] TrebleEnvelope, double Duration) Analyze(string filePath)
    {
        if (!File.Exists(filePath) || !IsSupportedFile(filePath))
        {
            return (Array.Empty<float>(), Array.Empty<float>(), Array.Empty<float>(), 0.0);
        }

        (ISampleProvider provider, int channels, int sampleRate, double duration, IDisposable disposable) = OpenAudioFile(filePath);
        using (disposable)
        {
            float[] samples = ReadAllSamples(provider);
            float[] mono = channels > 1 ? DownmixToMono(samples, channels) : samples;

            int m = (int)Math.Log2(WindowSize);
            float freqResolution = (float)sampleRate / WindowSize;
            var bassEnvelope = new List<float>();
            var midEnvelope = new List<float>();
            var trebleEnvelope = new List<float>();

            for (int i = 0; i + WindowSize < mono.Length; i += StepSize)
            {
                var data = new Complex[WindowSize];
                for (int k = 0; k < WindowSize; k++)
                {
                    data[k].X = mono[i + k];
                    data[k].Y = 0;
                }

                FastFourierTransform.FFT(true, m, data);

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

                bassEnvelope.Add(bassCount > 0 ? (float)(bassSum / bassCount) : 0f);
                midEnvelope.Add(midCount > 0 ? (float)(midSum / midCount) : 0f);
                trebleEnvelope.Add(trebleCount > 0 ? (float)(trebleSum / trebleCount) : 0f);
            }

            float[] bass = bassEnvelope.ToArray();
            float[] mid = midEnvelope.ToArray();
            float[] treble = trebleEnvelope.ToArray();
            NormalizeInPlace(bass);
            NormalizeInPlace(mid);
            NormalizeInPlace(treble);

            return (bass, mid, treble, duration);
        }
    }

    private static (ISampleProvider Provider, int Channels, int SampleRate, double Duration, IDisposable Disposable) OpenAudioFile(string filePath)
    {
        if (string.Equals(Path.GetExtension(filePath), ".ogg", StringComparison.OrdinalIgnoreCase))
        {
            var vorbisReader = new VorbisWaveReader(filePath);
            return (vorbisReader, vorbisReader.WaveFormat.Channels, vorbisReader.WaveFormat.SampleRate, vorbisReader.TotalTime.TotalSeconds, vorbisReader);
        }

        var reader = new AudioFileReader(filePath);
        return (reader, reader.WaveFormat.Channels, reader.WaveFormat.SampleRate, reader.TotalTime.TotalSeconds, reader);
    }

    private static float[] ReadAllSamples(ISampleProvider provider)
    {
        var chunks = new List<float[]>();
        int total = 0;
        float[] buffer = new float[65536];
        int read;
        while ((read = provider.Read(buffer, 0, buffer.Length)) > 0)
        {
            var chunk = new float[read];
            Array.Copy(buffer, chunk, read);
            chunks.Add(chunk);
            total += read;
        }

        var result = new float[total];
        int offset = 0;
        foreach (float[] chunk in chunks)
        {
            Array.Copy(chunk, 0, result, offset, chunk.Length);
            offset += chunk.Length;
        }

        return result;
    }

    private static float[] DownmixToMono(float[] interleaved, int channels)
    {
        var mono = new float[interleaved.Length / channels];
        for (int i = 0; i < mono.Length; i++)
        {
            float sum = 0;
            for (int c = 0; c < channels; c++) sum += interleaved[(i * channels) + c];
            mono[i] = sum / channels;
        }

        return mono;
    }

    private const float TrebleFreqMin = 2000f;
    private const float TrebleFreqMax = 8000f;
    public const int SpectrumBandsCount = 64;
    private const int SpectrumBands = SpectrumBandsCount;
    private const int AnalysisFps = 60;

    public static AudioAnalysisResult AnalyzeFull(string filePath)
    {
        if (!File.Exists(filePath) || !IsSupportedFile(filePath))
        {
            return new AudioAnalysisResult();
        }

        (ISampleProvider provider, int channels, int sampleRate, double duration, IDisposable disposable) = OpenAudioFile(filePath);
        using var disposableGuard = disposable;

        float[] interleaved = ReadAllSamples(provider);
        float[] leftChannel = channels > 1 ? TakeLeftChannel(interleaved, channels) : interleaved;

        int samplesPerFrame = sampleRate / AnalysisFps;
        int totalFrames = (int)(duration * AnalysisFps);
        int maxSamples = totalFrames * samplesPerFrame;

        if (leftChannel.Length < maxSamples)
        {
            totalFrames = leftChannel.Length / samplesPerFrame;
            maxSamples = totalFrames * samplesPerFrame;
        }

        var waveform = new float[totalFrames][];
        var bass = new float[totalFrames];
        var treble = new float[totalFrames];
        var spectrum = new float[totalFrames][];

        float[] hanning = BuildHanningWindow(samplesPerFrame);
        int m = (int)Math.Log2(NextPowerOfTwo(samplesPerFrame));
        int fftSize = 1 << m;
        float freqResolution = (float)sampleRate / fftSize;
        int binsHalf = (fftSize / 2) + 1;
        int binsPerBand = (binsHalf / 2) / SpectrumBands;

        for (int frame = 0; frame < totalFrames; frame++)
        {
            int offset = frame * samplesPerFrame;
            waveform[frame] = new float[samplesPerFrame];

            var data = new Complex[fftSize];
            for (int i = 0; i < samplesPerFrame; i++)
            {
                float sample = leftChannel[offset + i];
                waveform[frame][i] = sample;
                data[i].X = sample * hanning[i];
                data[i].Y = 0;
            }

            FastFourierTransform.FFT(true, m, data);

            double bassSum = 0; int bassCount = 0;
            double trebleSum = 0; int trebleCount = 0;
            var magnitudes = new float[binsHalf];

            for (int k = 0; k < binsHalf; k++)
            {
                float freq = k * freqResolution;
                float mag = (float)Math.Sqrt((data[k].X * data[k].X) + (data[k].Y * data[k].Y));
                magnitudes[k] = mag;

                if (freq >= BassFreqMin && freq <= BassFreqMax) { bassSum += mag; bassCount++; }
                if (freq >= TrebleFreqMin && freq <= TrebleFreqMax) { trebleSum += mag; trebleCount++; }
            }

            bass[frame] = bassCount > 0 ? (float)(bassSum / bassCount) : 0f;
            treble[frame] = trebleCount > 0 ? (float)(trebleSum / trebleCount) : 0f;

            var bands = new float[SpectrumBands];
            if (binsPerBand > 0)
            {
                for (int band = 0; band < SpectrumBands; band++)
                {
                    double bandSum = 0;
                    for (int b = 0; b < binsPerBand; b++)
                    {
                        bandSum += magnitudes[(band * binsPerBand) + b];
                    }

                    bands[band] = (float)(bandSum / binsPerBand);
                }
            }

            spectrum[frame] = bands;
        }

        NormalizeInPlace(bass);
        NormalizeInPlace(treble);
        NormalizeInPlace2D(spectrum);

        return new AudioAnalysisResult
        {
            Bass = bass,
            Treble = treble,
            Spectrum = spectrum,
            Waveform = waveform,
            Duration = duration,
        };
    }

    private static float[] TakeLeftChannel(float[] interleaved, int channels)
    {
        var left = new float[interleaved.Length / channels];
        for (int i = 0; i < left.Length; i++)
        {
            left[i] = interleaved[i * channels];
        }

        return left;
    }

    private static float[] BuildHanningWindow(int size)
    {
        var window = new float[size];
        for (int i = 0; i < size; i++)
        {
            window[i] = 0.5f - (0.5f * MathF.Cos(2f * MathF.PI * i / (size - 1)));
        }

        return window;
    }

    private static int NextPowerOfTwo(int value)
    {
        int power = 1;
        while (power < value) power <<= 1;
        return power;
    }

    private static void NormalizeInPlace(float[] values)
    {
        float max = values.Length > 0 ? values.Max() : 0f;
        if (max <= 0) return;
        for (int i = 0; i < values.Length; i++) values[i] /= max;
    }

    private static void NormalizeInPlace2D(float[][] values)
    {
        float max = 0f;
        foreach (float[] row in values)
        {
            foreach (float v in row)
            {
                if (v > max) max = v;
            }
        }

        if (max <= 0) return;

        foreach (float[] row in values)
        {
            for (int i = 0; i < row.Length; i++) row[i] /= max;
        }
    }
}
