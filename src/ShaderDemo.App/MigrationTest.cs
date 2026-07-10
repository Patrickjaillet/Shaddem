// Copyright (c) 2026 Patrick JAILLET
using ShaderDemo.Core.Migration;

namespace ShaderDemo.App;

public static class MigrationTest
{
    public static void Run(string pythonProjectDirectory)
    {
        MigrationResult result = ProjectMigrator.MigrateFromPythonProject(pythonProjectDirectory, Console.WriteLine);

        Console.WriteLine($"[migration-test] Settings migrated: {result.Settings != null}");
        if (result.Settings != null)
        {
            Console.WriteLine($"[migration-test]   WindowWidth={result.Settings.WindowWidth} WindowHeight={result.Settings.WindowHeight} MusicFile={result.Settings.MusicFile} AudioReactive={result.Settings.AudioReactive}");
        }

        Console.WriteLine($"[migration-test] Layers migrated: {result.Layers?.Count ?? 0}");

        Console.WriteLine($"[migration-test] Timeline migrated: {result.Timeline != null}");
        if (result.Timeline != null)
        {
            Console.WriteLine($"[migration-test]   Clips={result.Timeline.Clips.Count} Markers={result.Timeline.Markers.Count}");
            foreach (var clip in result.Timeline.Clips.Take(5))
            {
                Console.WriteLine($"[migration-test]   Clip: {clip.Start:F2}s-{clip.End:F2}s [{clip.Type}] {clip.Resource}");
            }
        }

        Console.WriteLine("[migration-test] Done.");
    }
}
