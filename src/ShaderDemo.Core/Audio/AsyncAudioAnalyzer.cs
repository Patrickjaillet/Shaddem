// Copyright (c) 2026 Patrick JAILLET
using System.Collections.Concurrent;

namespace ShaderDemo.Core.Audio;

public sealed class AsyncAudioAnalyzer
{
    private readonly ConcurrentQueue<(string Path, AudioAnalysisResult Result)> _completed = new();
    private readonly HashSet<string> _pending = new();
    private readonly object _pendingLock = new();

    public void RequestAnalysis(string filePath)
    {
        lock (_pendingLock)
        {
            if (!_pending.Add(filePath)) return;
        }

        Task.Run(() =>
        {
            try
            {
                AudioAnalysisResult result = AudioAnalyzer.AnalyzeFull(filePath);
                _completed.Enqueue((filePath, result));
            }
            finally
            {
                lock (_pendingLock)
                {
                    _pending.Remove(filePath);
                }
            }
        });
    }

    public void DrainCompleted(Action<string, AudioAnalysisResult> onReady)
    {
        while (_completed.TryDequeue(out (string Path, AudioAnalysisResult Result) item))
        {
            onReady(item.Path, item.Result);
        }
    }
}
