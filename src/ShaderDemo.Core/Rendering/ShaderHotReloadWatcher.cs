// Copyright (c) 2026 Patrick JAILLET
using System.Collections.Concurrent;

namespace ShaderDemo.Core.Rendering;

public sealed class ShaderHotReloadWatcher : IDisposable
{
    private readonly FileSystemWatcher? _watcher;
    private readonly ConcurrentQueue<string> _pending = new();
    private readonly HashSet<string> _queuedPaths = new();
    private readonly object _lock = new();

    public bool IsWatching { get; }

    public ShaderHotReloadWatcher(string directoryPath)
    {
        IsWatching = Directory.Exists(directoryPath);
        if (!IsWatching) return;

        _watcher = new FileSystemWatcher(directoryPath)
        {
            NotifyFilter = NotifyFilters.LastWrite,
        };
        _watcher.Filters.Add("*.glsl");
        _watcher.Filters.Add("*.hlsl");
        _watcher.Changed += OnChanged;
        _watcher.EnableRaisingEvents = true;
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        lock (_lock)
        {
            if (_queuedPaths.Add(e.FullPath))
            {
                _pending.Enqueue(e.FullPath);
            }
        }
    }

    public void ProcessPending(ShaderManager manager, Action<string>? log = null)
    {
        while (_pending.TryDequeue(out string? path))
        {
            lock (_lock)
            {
                _queuedPaths.Remove(path);
            }

            if (File.Exists(path))
            {
                ShaderLoader.LoadFile(manager, path, msg => log?.Invoke($"[hot-reload] {msg}"));
            }
        }
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}
