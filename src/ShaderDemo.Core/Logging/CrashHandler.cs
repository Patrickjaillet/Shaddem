// Copyright (c) 2026 Patrick JAILLET
namespace ShaderDemo.Core.Logging;

public static class CrashHandler
{
    public static void Install(string baseDirectory)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            WriteCrashDump(baseDirectory, e.ExceptionObject as Exception, e.IsTerminating);
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            WriteCrashDump(baseDirectory, e.Exception, isTerminating: false);
            e.SetObserved();
        };
    }

    private static void WriteCrashDump(string baseDirectory, Exception? exception, bool isTerminating)
    {
        string message = exception?.ToString() ?? "Unknown unhandled exception (no Exception object available).";
        AppLog.Error($"Unhandled exception (terminating={isTerminating}): {message}");

        try
        {
            string dumpDirectory = Path.Combine(baseDirectory, "crashes");
            Directory.CreateDirectory(dumpDirectory);
            string dumpPath = Path.Combine(dumpDirectory, $"crash_{DateTime.Now:yyyyMMdd-HHmmss}.log");
            File.WriteAllText(dumpPath, $"{DateTime.Now:O}\r\nTerminating: {isTerminating}\r\n\r\n{message}");
        }
        catch (IOException)
        {
        }
    }
}
