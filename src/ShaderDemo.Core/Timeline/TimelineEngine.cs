// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using System.Reflection;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Timeline;

public sealed class TimelineEngine
{
    private const int MaxHistory = 50;
    private const double DefaultImageFadeSeconds = 0.5;

    private readonly List<List<TimelineClip>> _history = new();
    private readonly List<List<TimelineClip>> _redoStack = new();
    private readonly Dictionary<TimelineClip, Layer> _imageClipLayers = new();
    private readonly Dictionary<TimelineClip, Layer> _modelClipLayers = new();
    private TimelineClip? _activeMusicClip;

    public List<TimelineClip> Clips { get; private set; } = new();
    public List<TimelineMarker> Markers { get; } = new();
    public List<Track> Tracks { get; } = Track.CreateDefaults();
    public bool Active { get; set; }

    private void SaveState()
    {
        _history.Add(Clips.Select(c => c.Clone()).ToList());
        _redoStack.Clear();
        if (_history.Count > MaxHistory) _history.RemoveAt(0);
    }

    public void BeginEdit()
    {
        SaveState();
    }

    public void Undo()
    {
        if (_history.Count == 0) return;
        _redoStack.Add(Clips.Select(c => c.Clone()).ToList());
        Clips = _history[^1];
        _history.RemoveAt(_history.Count - 1);
    }

    public void Redo()
    {
        if (_redoStack.Count == 0) return;
        _history.Add(Clips.Select(c => c.Clone()).ToList());
        Clips = _redoStack[^1];
        _redoStack.RemoveAt(_redoStack.Count - 1);
    }

    public TimelineClip Add(double start, double duration, ClipType type, string resource, Dictionary<string, object?>? clipParams = null)
    {
        SaveState();
        var clip = new TimelineClip(start, duration, type, resource, clipParams);
        Clips.Add(clip);
        Clips.Sort((a, b) => a.Start.CompareTo(b.Start));
        return clip;
    }

    public void Remove(TimelineClip clip)
    {
        if (!Clips.Contains(clip)) return;
        SaveState();
        Clips.Remove(clip);
    }

    public TimelineClip? Split(TimelineClip clip, double splitTime)
    {
        if (!(clip.Start + 0.01 < splitTime && splitTime < clip.End - 0.01)) return null;

        SaveState();

        double newStart = splitTime;
        double newDuration = clip.End - splitTime;
        var newClip = new TimelineClip(newStart, newDuration, clip.Type, clip.Resource, new Dictionary<string, object?>(clip.Params));

        clip.Duration = splitTime - clip.Start;

        Clips.Add(newClip);
        Clips.Sort((a, b) => a.Start.CompareTo(b.Start));
        return newClip;
    }

    public void AddMarker(double time, string label, Vector4 color)
    {
        Markers.Add(new TimelineMarker(time, label, color));
        Markers.Sort((a, b) => a.Time.CompareTo(b.Time));
    }

    public void RemoveMarker(double time)
    {
        Markers.RemoveAll(m => Math.Abs(m.Time - time) <= 0.001);
    }

    public IEnumerable<TimelineClip> GetActiveClips(double time)
    {
        return Clips.Where(c => c.Start <= time && time < c.End);
    }

    public Track? FindTrackFor(ClipType type)
    {
        return Tracks.FirstOrDefault(t => t.Types.Contains(type));
    }

    public IEnumerable<TimelineClip> GetFilteredActiveClips(double time)
    {
        bool soloActive = Tracks.Any(t => t.Solo);

        foreach (TimelineClip clip in GetActiveClips(time))
        {
            Track? track = FindTrackFor(clip.Type);
            if (track == null)
            {
                if (!soloActive) yield return clip;
                continue;
            }

            if (soloActive)
            {
                if (track.Solo) yield return clip;
            }
            else if (!track.Mute)
            {
                yield return clip;
            }
        }
    }

    private static double ApplyEasing(double t, string? easing)
    {
        return easing switch
        {
            "ease_in" => t * t,
            "ease_out" => t * (2 - t),
            "smooth" => t * t * (3 - 2 * t),
            "smoother" => t * t * t * (t * (t * 6 - 15) + 10),
            _ => t,
        };
    }

    private static bool TryGetInterpolatedScalar(TimelineClip clip, double currentTime, out double value)
    {
        if (clip.TryGetDouble("value", out value)) return true;

        if (clip.TryGetDouble("start_value", out double start) && clip.TryGetDouble("end_value", out double end))
        {
            value = InterpolateScalar(clip, currentTime, start, end);
            return true;
        }

        value = 0.0;
        return false;
    }

    private static bool TryGetInterpolatedVector(TimelineClip clip, double currentTime, out double[] value)
    {
        if (clip.TryGetDoubleArray("value", out value)) return true;

        if (clip.TryGetDoubleArray("start_value", out double[] start) && clip.TryGetDoubleArray("end_value", out double[] end) && start.Length == end.Length)
        {
            double t = NormalizedProgress(clip, currentTime);
            value = new double[start.Length];
            for (int i = 0; i < start.Length; i++) value[i] = start[i] + (end[i] - start[i]) * t;
            return true;
        }

        value = Array.Empty<double>();
        return false;
    }

    private static double NormalizedProgress(TimelineClip clip, double currentTime)
    {
        if (clip.Duration <= 0.001) return 1.0;
        double t = (currentTime - clip.Start) / clip.Duration;
        t = Math.Clamp(t, 0.0, 1.0);
        return ApplyEasing(t, clip.GetString("easing"));
    }

    private static double InterpolateScalar(TimelineClip clip, double currentTime, double start, double end)
    {
        double t = NormalizedProgress(clip, currentTime);
        return start + (end - start) * t;
    }

    private static string ToPascalCase(string snakeCase)
    {
        string[] parts = snakeCase.Split('_', StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
    }

    public void ApplyEffects(EffectParams effects, double currentTime)
    {
        effects.Speed = 1.0f;
        effects.Intensity = 1.0f;
        effects.Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

        foreach (TimelineClip clip in GetFilteredActiveClips(currentTime))
        {
            if (clip.Type != ClipType.Effect) continue;

            FieldInfo? field = typeof(EffectParams).GetField(ToPascalCase(clip.Resource));
            if (field == null) continue;

            if (field.FieldType == typeof(float))
            {
                if (TryGetInterpolatedScalar(clip, currentTime, out double v)) field.SetValue(effects, (float)v);
            }
            else if (field.FieldType == typeof(int))
            {
                if (TryGetInterpolatedScalar(clip, currentTime, out double v)) field.SetValue(effects, (int)v);
            }
            else if (field.FieldType == typeof(bool))
            {
                if (TryGetInterpolatedScalar(clip, currentTime, out double v)) field.SetValue(effects, v != 0.0);
            }
            else if (field.FieldType == typeof(Vector3))
            {
                if (TryGetInterpolatedVector(clip, currentTime, out double[] v) && v.Length >= 3)
                {
                    field.SetValue(effects, new Vector3((float)v[0], (float)v[1], (float)v[2]));
                }
            }
            else if (field.FieldType == typeof(Vector4))
            {
                if (TryGetInterpolatedVector(clip, currentTime, out double[] v) && v.Length >= 4)
                {
                    field.SetValue(effects, new Vector4((float)v[0], (float)v[1], (float)v[2], (float)v[3]));
                }
            }
        }
    }

    public void ApplyShader(ShaderManager manager, double currentTime)
    {
        TimelineClip? shaderClip = GetFilteredActiveClips(currentTime).LastOrDefault(c => c.Type == ClipType.Shader);
        if (shaderClip == null) return;

        int index = manager.ShaderNames.ToList().IndexOf(shaderClip.Resource);
        if (index >= 0) manager.SelectShader(index);
    }

    public void ApplyImageLayers(ShaderManager manager, double currentTime)
    {
        var activeImageClips = GetFilteredActiveClips(currentTime).Where(c => c.Type == ClipType.Image).ToList();
        var activeSet = new HashSet<TimelineClip>(activeImageClips);

        foreach (TimelineClip stale in _imageClipLayers.Keys.Where(c => !activeSet.Contains(c)).ToList())
        {
            manager.Layers.Remove(_imageClipLayers[stale]);
            _imageClipLayers.Remove(stale);
        }

        foreach (TimelineClip clip in activeImageClips)
        {
            if (!_imageClipLayers.TryGetValue(clip, out Layer? layer))
            {
                layer = Layer.CreateImage(clip.Resource);
                layer.IsTimelineManaged = true;
                manager.Layers.Add(layer);
                _imageClipLayers[clip] = layer;
            }

            if (layer.ImagePath != clip.Resource) layer.ImagePath = clip.Resource;
            layer.Enabled = true;

            double baseOpacity = clip.TryGetDouble("opacity", out double op) ? op : 1.0;
            double fadeIn = clip.TryGetDouble("fade_in", out double fi) ? fi : DefaultImageFadeSeconds;
            double fadeOut = clip.TryGetDouble("fade_out", out double fo) ? fo : DefaultImageFadeSeconds;

            double timeSinceStart = currentTime - clip.Start;
            double timeUntilEnd = clip.End - currentTime;
            double fadeInFactor = fadeIn > 0.0 ? Math.Clamp(timeSinceStart / fadeIn, 0.0, 1.0) : 1.0;
            double fadeOutFactor = fadeOut > 0.0 ? Math.Clamp(timeUntilEnd / fadeOut, 0.0, 1.0) : 1.0;

            layer.Opacity = (float)(baseOpacity * Math.Min(fadeInFactor, fadeOutFactor));

            if (clip.TryGetDouble("position_x", out double px)) layer.PositionX = (float)px;
            if (clip.TryGetDouble("position_y", out double py)) layer.PositionY = (float)py;
            if (clip.TryGetDouble("scale", out double sc)) layer.Scale = (float)sc;
            if (clip.TryGetDouble("rotation", out double rot)) layer.Rotation = (float)rot;

            string? blendName = clip.GetString("blend_mode");
            if (blendName != null && Enum.TryParse(blendName, true, out BlendMode blendMode)) layer.BlendMode = blendMode;

            string? fitName = clip.GetString("fit_mode");
            if (fitName != null && Enum.TryParse(fitName, true, out ImageFitMode fitMode)) layer.FitMode = fitMode;
        }
    }

    public void ApplyModelLayers(ShaderManager manager, double currentTime)
    {
        var activeClips = GetFilteredActiveClips(currentTime).Where(c => c.Type == ClipType.Model3D).ToList();
        var activeSet = new HashSet<TimelineClip>(activeClips);

        foreach (TimelineClip stale in _modelClipLayers.Keys.Where(c => !activeSet.Contains(c)).ToList())
        {
            manager.Layers.Remove(_modelClipLayers[stale]);
            _modelClipLayers.Remove(stale);
        }

        foreach (TimelineClip clip in activeClips)
        {
            if (!_modelClipLayers.TryGetValue(clip, out Layer? layer))
            {
                layer = Layer.CreateModel3D(clip.Resource);
                layer.IsTimelineManaged = true;
                manager.Layers.Add(layer);
                _modelClipLayers[clip] = layer;
            }

            if (layer.ModelState.CurrentModelFilename != clip.Resource) layer.ModelState.CurrentModelFilename = clip.Resource;
            layer.Enabled = true;

            string? texturePath = clip.GetString("texture_path");
            if (texturePath != null) layer.ModelState.CurrentTextureFilename = texturePath;

            double baseOpacity = clip.TryGetDouble("opacity", out double op) ? op : 1.0;
            double fadeIn = clip.TryGetDouble("fade_in", out double fi) ? fi : DefaultImageFadeSeconds;
            double fadeOut = clip.TryGetDouble("fade_out", out double fo) ? fo : DefaultImageFadeSeconds;

            double timeSinceStart = currentTime - clip.Start;
            double timeUntilEnd = clip.End - currentTime;
            double fadeInFactor = fadeIn > 0.0 ? Math.Clamp(timeSinceStart / fadeIn, 0.0, 1.0) : 1.0;
            double fadeOutFactor = fadeOut > 0.0 ? Math.Clamp(timeUntilEnd / fadeOut, 0.0, 1.0) : 1.0;
            layer.Opacity = (float)(baseOpacity * Math.Min(fadeInFactor, fadeOutFactor));

            Vector3 position = layer.ModelState.Position;
            if (clip.TryGetDouble("position_x", out double px)) position.X = (float)px;
            if (clip.TryGetDouble("position_y", out double py)) position.Y = (float)py;
            if (clip.TryGetDouble("position_z", out double pz)) position.Z = (float)pz;
            layer.ModelState.Position = position;

            Vector3 rotation = layer.ModelState.Rotation;
            if (clip.TryGetDouble("rotation_x", out double rx)) rotation.X = (float)rx;
            if (clip.TryGetDouble("rotation_y", out double ry)) rotation.Y = (float)ry;
            if (clip.TryGetDouble("rotation_z", out double rz)) rotation.Z = (float)rz;
            layer.ModelState.Rotation = rotation;

            if (clip.TryGetDouble("scale", out double s)) layer.ModelState.Scale = new Vector3((float)s);

            Vector3 autoRotate = layer.ModelState.AutoRotateSpeed;
            if (clip.TryGetDouble("auto_rotate_y", out double ary)) autoRotate.Y = (float)ary;
            layer.ModelState.AutoRotateSpeed = autoRotate;

            string? blendName = clip.GetString("blend_mode");
            if (blendName != null && Enum.TryParse(blendName, true, out BlendMode blendMode)) layer.BlendMode = blendMode;
        }
    }

    public void ApplyMusicClip(ShaderManager manager, double currentTime)
    {
        TimelineClip? musicClip = GetFilteredActiveClips(currentTime).LastOrDefault(c => c.Type == ClipType.Music);

        if (musicClip == null)
        {
            if (_activeMusicClip != null)
            {
                manager.Player.Stop();
                _activeMusicClip = null;
            }

            return;
        }

        float volume = musicClip.TryGetDouble("volume", out double v) ? (float)Math.Clamp(v, 0.0, 1.0) : 1.0f;

        if (!ReferenceEquals(musicClip, _activeMusicClip))
        {
            TimeSpan offset = TimeSpan.FromSeconds(Math.Max(0.0, currentTime - musicClip.Start));
            manager.Player.Play(musicClip.Resource, volume, offset);
            _activeMusicClip = musicClip;
        }
        else
        {
            manager.Player.SetVolume(volume);
        }
    }

    public void ApplyLayerAutomation(ShaderManager manager, double currentTime)
    {
        foreach (TimelineClip clip in GetFilteredActiveClips(currentTime))
        {
            if (clip.Type != ClipType.LayerAutomation) continue;

            Layer? target = manager.Layers.FirstOrDefault(l => l.Name == clip.Resource && !l.IsTimelineManaged);
            if (target == null) continue;

            string property = clip.GetString("property") ?? "opacity";
            if (property == "enabled")
            {
                if (TryGetInterpolatedScalar(clip, currentTime, out double v)) target.Enabled = v >= 0.5;
            }
            else if (TryGetInterpolatedScalar(clip, currentTime, out double v))
            {
                target.Opacity = (float)Math.Clamp(v, 0.0, 1.0);
            }
        }
    }
}
