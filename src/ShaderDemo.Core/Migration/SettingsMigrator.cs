// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using System.Reflection;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;

namespace ShaderDemo.Core.Migration;

public static class SettingsMigrator
{
    public static AppSettings Migrate(string pythonSettingsFilePath, Action<string>? log = null)
    {
        var settings = new AppSettings();
        EffectParams effects = settings.Effects;
        int applied = 0;
        int skipped = 0;

        foreach (string rawLine in File.ReadAllLines(pythonSettingsFilePath))
        {
            string line = StripComment(rawLine).Trim();
            int eq = line.IndexOf('=');
            if (eq < 0) continue;

            string name = line[..eq].Trim();
            string valueText = line[(eq + 1)..].Trim();
            if (name.Length == 0 || !char.IsUpper(name[0])) continue;

            try
            {
                switch (name)
                {
                    case "WINDOW_WIDTH": settings.WindowWidth = PythonLiteralParser.ParseInt(valueText); break;
                    case "WINDOW_HEIGHT": settings.WindowHeight = PythonLiteralParser.ParseInt(valueText); break;
                    case "SHADER_SWITCH_INTERVAL": settings.ShaderSwitchInterval = PythonLiteralParser.ParseInt(valueText); break;
                    case "MUSIC_FILE": settings.MusicFile = PythonLiteralParser.ParseString(valueText); break;
                    case "MUSIC_VOLUME": settings.MusicVolume = PythonLiteralParser.ParseFloat(valueText); break;
                    case "AUDIO_REACTIVE": settings.AudioReactive = PythonLiteralParser.ParseBool(valueText); break;
                    case "AUTO_SAVE_SETTINGS": settings.AutoSaveSettings = PythonLiteralParser.ParseBool(valueText); break;
                    default:
                        if (name.StartsWith("CUSTOM_", StringComparison.Ordinal))
                        {
                            if (ApplyEffectField(effects, name["CUSTOM_".Length..], valueText))
                            {
                                applied++;
                            }
                            else
                            {
                                skipped++;
                                log?.Invoke($"Unknown field, skipped: {name}");
                            }

                            continue;
                        }

                        continue;
                }

                applied++;
            }
            catch (Exception ex)
            {
                skipped++;
                log?.Invoke($"Failed to parse '{name} = {valueText}': {ex.Message}");
            }
        }

        log?.Invoke($"Settings migration: {applied} fields applied, {skipped} skipped");
        return settings;
    }

    private static bool ApplyEffectField(EffectParams effects, string upperSnakeFieldName, string valueText)
    {
        string fieldName = PythonLiteralParser.ToPascalCaseFromUpperSnake(upperSnakeFieldName);
        FieldInfo? field = typeof(EffectParams).GetField(fieldName);
        if (field == null) return false;

        if (field.FieldType == typeof(float))
        {
            field.SetValue(effects, PythonLiteralParser.ParseFloat(valueText));
        }
        else if (field.FieldType == typeof(int))
        {
            field.SetValue(effects, PythonLiteralParser.ParseInt(valueText));
        }
        else if (field.FieldType == typeof(bool))
        {
            field.SetValue(effects, PythonLiteralParser.ParseBool(valueText));
        }
        else if (field.FieldType == typeof(Vector3))
        {
            float[] v = PythonLiteralParser.ParseTuple(valueText);
            field.SetValue(effects, new Vector3(v[0], v[1], v[2]));
        }
        else if (field.FieldType == typeof(Vector4))
        {
            float[] v = PythonLiteralParser.ParseTuple(valueText);
            field.SetValue(effects, new Vector4(v[0], v[1], v[2], v[3]));
        }
        else
        {
            return false;
        }

        return true;
    }

    private static string StripComment(string line)
    {
        int hash = line.IndexOf('#');
        return hash >= 0 ? line[..hash] : line;
    }
}
