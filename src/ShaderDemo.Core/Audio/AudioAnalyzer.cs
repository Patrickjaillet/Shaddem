// Copyright (c) 2026 Patrick JAILLET
using NAudio.Dsp;
using NAudio.Wave;

namespace ShaderDemo.Core.Audio;

public static class AudioAnalyzer
{
    private const int WindowSize = 1024;
    private const int StepSize = 512;
    private const float BassFreqMin = 20f;
    private const float BassFreqMax = 150f;

    public static (float[] BassEnvelope, double Duration) Analyze(string filePath)
    {
        if (!File.Exists(filePath) || !string.Equals(Path.GetExtension(filePath), ".wav", StringComparison.OrdinalIgnoreCase))
        {
            return (Array.Empty<float>(), 0.0);
        }

        using var reader = new AudioFileReader(filePath);
        int channels = reader.WaveFormat.Channels;
        int sampleRate = reader.WaveFormat.SampleRate;

        float[] samples = ReadAllSamples(reader);
        float[] mono = channels > 1 ? DownmixToMono(samples, channels) : samples;

        int m = (int)Math.Log2(WindowSize);
        float freqResolution = (float)sampleRate / WindowSize;
        var envelope = new List<float>();

        for (int i = 0; i + WindowSize < mono.Length; i += StepSize)
        {
            var data = new Complex[WindowSize];
            for (int k = 0; k < WindowSize; k++)
            {
                data[k].X = mono[i + k];
                data[k].Y = 0;
            }

            FastFourierTransform.FFT(true, m, data);

            double sum = 0;
            int count = 0;
            for (int k = 0; k <= WindowSize / 2; k++)
            {
                float freq = k * freqResolution;
                if (freq >= BassFreqMin && freq <= BassFreqMax)
                {
                    sum += Math.Sqrt(data[k].X * data[k].X + data[k].Y * data[k].Y);
                    count++;
                }
            }

            envelope.Add(count > 0 ? (float)(sum / count) : 0f);
        }

        float[] result = envelope.ToArray();
        float max = result.Length > 0 ? result.Max() : 0f;
        if (max > 0)
        {
            for (int i = 0; i < result.Length; i++) result[i] /= max;
        }

        return (result, reader.TotalTime.TotalSeconds);
    }

    private static float[] ReadAllSamples(AudioFileReader reader)
    {
        long estimatedCount = reader.Length / sizeof(float);
        var samples = new float[estimatedCount];
        int offset = 0;
        float[] buffer = new float[65536];
        int read;
        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            if (offset + read > samples.Length)
            {
                Array.Resize(ref samples, offset + read);
            }

            Array.Copy(buffer, 0, samples, offset, read);
            offset += read;
        }

        if (offset != samples.Length)
        {
            Array.Resize(ref samples, offset);
        }

        return samples;
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
        if (!File.Exists(filePath) || !string.Equals(Path.GetExtension(filePath), ".wav", StringComparison.OrdinalIgnoreCase))
        {
            return new AudioAnalysisResult();
        }

        using var reader = new AudioFileReader(filePath);
        int channels = reader.WaveFormat.Channels;
        int sampleRate = reader.WaveFormat.SampleRate;

        float[] interleaved = ReadAllSamples(reader);
        float[] leftChannel = channels > 1 ? TakeLeftChannel(interleaved, channels) : interleaved;

        double duration = reader.TotalTime.TotalSeconds;
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
