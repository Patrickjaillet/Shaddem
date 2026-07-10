// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.Core.Migration;

public static class TimelineMigrator
{
    public static TimelineEngine Migrate(string timelineDbPath, Action<string>? log = null)
    {
        var engine = new TimelineEngine();

        using var connection = new SqliteConnection($"Data Source={timelineDbPath};Mode=ReadOnly");
        connection.Open();

        MigrateClips(connection, engine, log);
        MigrateMarkers(connection, engine, log);

        return engine;
    }

    private static void MigrateClips(SqliteConnection connection, TimelineEngine engine, Action<string>? log)
    {
        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT start, duration, type, resource, params FROM timeline ORDER BY start";

        int count = 0;
        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            double start = reader.GetDouble(0);
            double duration = reader.GetDouble(1);
            string typeText = reader.GetString(2);
            string resource = reader.GetString(3);
            string paramsJson = reader.IsDBNull(4) ? "{}" : reader.GetString(4);

            if (!TryParseClipType(typeText, out ClipType type))
            {
                log?.Invoke($"Unknown clip type '{typeText}', skipped");
                continue;
            }

            Dictionary<string, object?> clipParams = ParseParams(paramsJson);
            engine.Clips.Add(new TimelineClip(start, duration, type, resource, clipParams));
            count++;
        }

        engine.Clips.Sort((a, b) => a.Start.CompareTo(b.Start));
        log?.Invoke($"Timeline migration: {count} clip(s) imported");
    }

    private static void MigrateMarkers(SqliteConnection connection, TimelineEngine engine, Action<string>? log)
    {
        try
        {
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT time, name, color FROM markers ORDER BY time";

            int count = 0;
            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                double time = reader.GetDouble(0);
                string name = reader.GetString(1);
                Vector4 color = ParseColor(reader.IsDBNull(2) ? null : reader.GetString(2));

                engine.Markers.Add(new TimelineMarker(time, name, color));
                count++;
            }

            log?.Invoke($"Timeline migration: {count} marker(s) imported");
        }
        catch (SqliteException ex)
        {
            log?.Invoke($"No markers table (older schema?), skipped: {ex.Message}");
        }
    }

    private static bool TryParseClipType(string pythonTypeValue, out ClipType type)
    {
        switch (pythonTypeValue)
        {
            case "shader": type = ClipType.Shader; return true;
            case "image": type = ClipType.Image; return true;
            case "music": type = ClipType.Music; return true;
            case "effect": type = ClipType.Effect; return true;
            case "text": type = ClipType.Text; return true;
            case "model_3d": type = ClipType.Model3D; return true;
            default: type = default; return false;
        }
    }

    private static Dictionary<string, object?> ParseParams(string json)
    {
        var result = new Dictionary<string, object?>();
        using JsonDocument doc = JsonDocument.Parse(json);
        foreach (JsonProperty prop in doc.RootElement.EnumerateObject())
        {
            result[prop.Name] = prop.Value.Clone();
        }

        return result;
    }

    private static Vector4 ParseColor(string? json)
    {
        if (json == null) return new Vector4(1.0f, 1.0f, 0.0f, 1.0f);

        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            float[] v = doc.RootElement.EnumerateArray().Select(e => e.GetSingle()).ToArray();
            return v.Length >= 4 ? new Vector4(v[0], v[1], v[2], v[3]) : new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
        }
        catch (JsonException)
        {
            return new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
        }
    }
}
