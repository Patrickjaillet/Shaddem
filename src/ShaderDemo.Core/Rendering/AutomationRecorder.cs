// Copyright (c) 2026 Patrick JAILLET
using System.Reflection;

namespace ShaderDemo.Core.Rendering;

public sealed class AutomationRecorder
{
    private static readonly FieldInfo[] FloatFields = typeof(EffectParams)
        .GetFields(BindingFlags.Public | BindingFlags.Instance)
        .Where(f => f.FieldType == typeof(float))
        .ToArray();

    private readonly Dictionary<string, List<(float Time, float Value)>> _data = new();
    private readonly Dictionary<string, float> _lastValues = new();
    private readonly List<(float Time, int Value)> _shaderIndexKeyframes = new();
    private int _lastShaderIndex;

    public bool Recording { get; private set; }
    public bool Playing { get; private set; }
    public float Time { get; private set; }
    public float Duration { get; private set; }

    public void ToggleRecording(EffectParams effects, int currentShaderIndex)
    {
        if (Recording)
        {
            Recording = false;
            return;
        }

        Recording = true;
        Playing = false;
        Time = 0f;
        Duration = 0f;
        _data.Clear();
        _shaderIndexKeyframes.Clear();
        _lastValues.Clear();

        foreach (FieldInfo field in FloatFields)
        {
            _lastValues[field.Name] = (float)field.GetValue(effects)!;
        }

        _lastShaderIndex = currentShaderIndex;
    }

    public void TogglePlayback()
    {
        if (Playing)
        {
            Playing = false;
            return;
        }

        Playing = true;
        Recording = false;
        if (Time >= Duration) Time = 0f;
    }

    public void Stop()
    {
        Recording = false;
        Playing = false;
    }

    public void Update(float dt, EffectParams effects, ShaderManager manager)
    {
        if (Recording)
        {
            Time += dt;
            Duration = Math.Max(Duration, Time);

            foreach (FieldInfo field in FloatFields)
            {
                float val = (float)field.GetValue(effects)!;
                bool changed = !_lastValues.TryGetValue(field.Name, out float last) || last != val;
                bool unseen = !_data.ContainsKey(field.Name);

                if (changed || unseen)
                {
                    if (!_data.TryGetValue(field.Name, out List<(float, float)>? list))
                    {
                        list = new List<(float, float)>();
                        _data[field.Name] = list;
                    }

                    list.Add((Time, val));
                    _lastValues[field.Name] = val;
                }
            }

            if (_lastShaderIndex != manager.CurrentShaderIndex)
            {
                _shaderIndexKeyframes.Add((Time, manager.CurrentShaderIndex));
                _lastShaderIndex = manager.CurrentShaderIndex;
            }
        }
        else if (Playing)
        {
            Time += dt;
            if (Time > Duration) Time = 0f;

            foreach (FieldInfo field in FloatFields)
            {
                if (_data.TryGetValue(field.Name, out List<(float Time, float Value)>? keyframes) && keyframes.Count > 0)
                {
                    field.SetValue(effects, InterpolateFloat(keyframes, Time));
                }
            }

            if (_shaderIndexKeyframes.Count > 0)
            {
                int idx = InterpolateStep(_shaderIndexKeyframes, Time);
                if (idx != manager.CurrentShaderIndex) manager.SelectShader(idx);
            }
        }
    }

    private static float InterpolateFloat(List<(float Time, float Value)> keyframes, float t)
    {
        if (t <= keyframes[0].Time) return keyframes[0].Value;
        if (t >= keyframes[^1].Time) return keyframes[^1].Value;

        for (int i = 0; i < keyframes.Count - 1; i++)
        {
            (float t1, float v1) = keyframes[i];
            (float t2, float v2) = keyframes[i + 1];
            if (t1 <= t && t < t2)
            {
                float alpha = t2 - t1 > 0f ? (t - t1) / (t2 - t1) : 0f;
                return v1 + ((v2 - v1) * alpha);
            }
        }

        return keyframes[^1].Value;
    }

    private static int InterpolateStep(List<(float Time, int Value)> keyframes, float t)
    {
        if (t <= keyframes[0].Time) return keyframes[0].Value;

        for (int i = 0; i < keyframes.Count - 1; i++)
        {
            if (keyframes[i].Time <= t && t < keyframes[i + 1].Time) return keyframes[i].Value;
        }

        return keyframes[^1].Value;
    }
}
