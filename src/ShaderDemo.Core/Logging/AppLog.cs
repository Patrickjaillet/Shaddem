// Copyright (c) 2026 Patrick JAILLET
namespace ShaderDemo.Core.Logging;

public static class AppLog
{
    private static readonly object FileLock = new();
    private static string? _logFilePath;

    public static void Initialize(string logFilePath)
    {
        _logFilePath = logFilePath;
        string? directory = Path.GetDirectoryName(logFilePath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
    }

    public static void Info(string message) => Write("INFO", message);

    public static void Warn(string message) => Write("WARN", message);

    public static void Error(string message) => Write("ERROR", message);

    private static void Write(string level, string message)
    {
        string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        Console.WriteLine(line);

        if (_logFilePath == null) return;

        lock (FileLock)
        {
            try
            {
                File.AppendAllText(_logFilePath, line + Environment.NewLine);
            }
            catch (IOException)
            {
            }
        }
    }
}
