// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using Silk.NET.OpenGL;
using ShaderDemo.Core.Audio;
using ShaderDemo.Core.Export;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.Core.Rendering;

public sealed class ShaderManager : IDisposable
{
    private readonly GL _gl;
    private readonly Dictionary<string, ShaderProgram> _shaderPrograms = new();
    private readonly List<string> _shaderNames = new();
    private readonly Dictionary<string, Texture> _layerImageCache = new();
    private readonly Dictionary<Layer, (ModelRenderer Renderer, string? ModelPath, string? TexturePath)> _layerModelRenderers = new();
    private Layer? _textLayer;
    private TimelineClip? _activeTextClip;

    private Framebuffer _accumPing;
    private Framebuffer _accumPong;

    public GL Gl => _gl;
    public RenderPipeline Pipeline { get; }
    public PostEffectsPipeline PostEffects { get; }
    public List<Layer> Layers { get; } = new();
    public EffectParams Effects { get; } = new();
    public AudioReactivity Audio { get; } = new();
    public AutomationRecorder Automation { get; } = new();
    public GpuProfiler Profiler { get; }
    public AdaptiveResolutionController AdaptiveResolution { get; } = new();

    public int MaxLayerImageDimension { get; set; } = 2048;

    public static bool UseShaderBinaryCache { get; set; }
    public AudioPlayer Player { get; } = new();
    public AsyncAudioAnalyzer AsyncAudio { get; } = new();
    public VideoRecorder Recorder { get; } = new();
    public Framebuffer? LastComposedFrame { get; private set; }

    public bool PresentToScreenEnabled { get; set; } = true;
    public Texture Channel0Texture { get; private set; }

    public ParticleSystem Particles { get; }
    public ModelRenderer Model { get; }
    public Model3DState ModelState { get; } = new();
    public AudioOverlay AudioViz { get; }
    public TextOverlay TextOverlay { get; }
    public TimelineEngine? Timeline { get; set; }

    public bool IsTransitioning { get; private set; }
    public float TransitionStartTime { get; private set; }
    public float TransitionDuration { get; private set; } = 1.0f;
    public int TransitionType { get; private set; }
    private int _prevShaderIndex;

    public bool IsLiveCodingActive { get; set; }
    private ShaderProgram? _liveShaderProgram;

    public int CurrentShaderIndex { get; private set; }
    public float ElapsedTime { get; private set; }
    public Vector2 MousePosition { get; set; }

    public ShaderManager(GL gl, int width, int height)
    {
        _gl = gl;
        Pipeline = new RenderPipeline(gl, width, height);
        PostEffects = new PostEffectsPipeline(gl, width, height);
        Profiler = new GpuProfiler(gl);
        Channel0Texture = TextureLoader.CreateNoiseTexture(gl);
        Particles = new ParticleSystem(gl);
        Model = new ModelRenderer(gl);
        AudioViz = new AudioOverlay(gl);
        TextOverlay = new TextOverlay(gl);

        _accumPing = Framebuffer.Create(gl, width, height);
        _accumPong = Framebuffer.Create(gl, width, height);
    }

    public bool LoadChannel0Texture(string path, Action<string>? log = null)
    {
        Texture? loaded = TextureLoader.Load(_gl, path, log);
        if (loaded == null) return false;

        Channel0Texture.Dispose();
        Channel0Texture = loaded;
        return true;
    }

    public string? CurrentShaderName => _shaderNames.Count > CurrentShaderIndex ? _shaderNames[CurrentShaderIndex] : null;
    public IReadOnlyList<string> ShaderNames => _shaderNames;

    public void RegisterShader(string name, string fragmentSource)
    {
        string wrapped = ShaderWrapper.Wrap(fragmentSource);
        var program = new ShaderProgram(_gl, name, BuiltinShaders.VertexPassthrough, wrapped, UseShaderBinaryCache);

        if (_shaderPrograms.TryGetValue(name, out var existing))
        {
            existing.Dispose();
        }
        else
        {
            _shaderNames.Add(name);
        }

        _shaderPrograms[name] = program;
    }

    public void SelectShader(int index)
    {
        if (_shaderNames.Count == 0) return;
        CurrentShaderIndex = ((index % _shaderNames.Count) + _shaderNames.Count) % _shaderNames.Count;
    }

    public bool RemoveShader(string name)
    {
        if (!_shaderPrograms.TryGetValue(name, out ShaderProgram? program)) return false;

        program.Dispose();
        _shaderPrograms.Remove(name);
        _shaderNames.Remove(name);

        if (CurrentShaderIndex >= _shaderNames.Count)
        {
            CurrentShaderIndex = Math.Max(0, _shaderNames.Count - 1);
        }

        return true;
    }

    public void NextShader()
    {
        SelectShader(CurrentShaderIndex + 1);
    }

    public void PreviousShader()
    {
        SelectShader(CurrentShaderIndex - 1);
    }

    public void StartTransition(int newIndex, float duration, int transitionType)
    {
        if (_shaderNames.Count == 0) return;
        _prevShaderIndex = CurrentShaderIndex;
        TransitionDuration = MathF.Max(0.01f, duration);
        TransitionType = transitionType;
        TransitionStartTime = ElapsedTime;
        IsTransitioning = true;
        SelectShader(newIndex);
    }

    public bool CompileLiveShader(string source, Action<string>? log = null)
    {
        try
        {
            string glsl = HlslToGlslConverter.Convert(source);
            string wrapped = ShaderWrapper.Wrap(glsl);
            var program = new ShaderProgram(_gl, "<Live Edit>", BuiltinShaders.VertexPassthrough, wrapped);
            _liveShaderProgram?.Dispose();
            _liveShaderProgram = program;
            return true;
        }
        catch (ShaderCompilationException ex)
        {
            log?.Invoke($"Live shader compile error: {ex.Message}");
            return false;
        }
    }

    public void Resize(int width, int height)
    {
        Pipeline.Resize(width, height);
        PostEffects.Resize(width, height);

        _accumPing.Dispose();
        _accumPong.Dispose();
        _accumPing = Framebuffer.Create(_gl, width, height);
        _accumPong = Framebuffer.Create(_gl, width, height);
    }

    public void Update(double deltaSeconds)
    {
        ElapsedTime += (float)deltaSeconds;
        Automation.Update((float)deltaSeconds, Effects, this);
    }

    public void SetElapsedTime(float seconds)
    {
        ElapsedTime = Math.Max(0.0f, seconds);
    }

    public void RenderFrame()
    {
        var frameStopwatch = System.Diagnostics.Stopwatch.StartNew();

        Profiler.Begin("Scene");
        Framebuffer composed = RenderBaseAndTransition();
        composed = RenderLayerChain(composed);
        Profiler.End("Scene");

        Profiler.Begin("ParticlesAndModel");
        if (Effects.ParticlesActive)
        {
            if (Particles.Count != Effects.ParticlesCount)
            {
                Particles.Resize(Effects.ParticlesCount);
            }

            composed.Use();
            float aspect = (float)Pipeline.Width / Pipeline.Height;
            Audio.TryGetBassValue(ElapsedTime, out float bassForKick);
            float kick = Effects.KickIntensity > 1.2f ? bassForKick * Effects.KickIntensity : 0.0f;
            Particles.Update(0.016f, kick);
            Particles.Render(ElapsedTime, aspect, Effects.ParticlesSize, Effects.ParticlesColor);
        }

        if (ModelState.ShowModel)
        {
            composed.Use();
            float aspect = (float)Pipeline.Width / Pipeline.Height;
            Model.Render(ModelState, ElapsedTime, aspect, Channel0Texture);
        }

        Profiler.End("ParticlesAndModel");

        Profiler.Begin("PostEffects");
        if (Effects.Bloom > 0.0f)
        {
            Framebuffer bloomTarget = composed == Pipeline.ComposeA ? Pipeline.ComposeB : Pipeline.ComposeA;
            PostEffects.RenderBloom(composed, bloomTarget, Effects.Bloom);
            composed = bloomTarget;
        }

        composed = RenderAccumulationEffects(composed);
        Profiler.End("PostEffects");

        Profiler.Begin("Overlay");
        if (AudioViz.Enabled)
        {
            AudioViz.Render(PostEffects, composed, ElapsedTime, Pipeline.Width, Pipeline.Height);
        }

        Profiler.End("Overlay");

        if (PresentToScreenEnabled) Pipeline.PresentToScreen(composed);
        else Pipeline.ClearScreen();
        LastComposedFrame = composed;

        if (Recorder.IsRecording)
        {
            Recorder.WriteFrame(composed.ReadPixelsRgb());
        }

        GpuResourceTracker.EndFrame();

        frameStopwatch.Stop();
        if (AdaptiveResolution.Update(frameStopwatch.Elapsed.TotalMilliseconds, PerformanceBudget.FrameTimeBudgetMs))
        {
            Pipeline.SetRenderScale(AdaptiveResolution.CurrentScale);
        }
    }

    private Framebuffer RenderBaseAndTransition()
    {
        ShaderProgram? active = IsLiveCodingActive && _liveShaderProgram != null
            ? _liveShaderProgram
            : CurrentShaderName != null ? _shaderPrograms[CurrentShaderName] : null;

        if (IsTransitioning)
        {
            float progress = (ElapsedTime - TransitionStartTime) / TransitionDuration;
            if (progress >= 1.0f)
            {
                IsTransitioning = false;
                Pipeline.RenderShaderPass(active, Effects, ElapsedTime, MousePosition, Audio.Compute(Effects, ElapsedTime), Channel0Texture, Pipeline.SceneFbo, AdaptiveResolution.CurrentScale);
                return Pipeline.SceneFbo;
            }

            ShaderProgram? prev = _shaderNames.Count > _prevShaderIndex ? _shaderPrograms[_shaderNames[_prevShaderIndex]] : null;
            Pipeline.RenderShaderPass(prev, Effects, ElapsedTime, MousePosition, Audio.Compute(Effects, ElapsedTime), Channel0Texture, Pipeline.SceneFbo, AdaptiveResolution.CurrentScale);
            Pipeline.RenderShaderPass(active, Effects, ElapsedTime, MousePosition, Audio.Compute(Effects, ElapsedTime), Channel0Texture, Pipeline.LayerFbo, AdaptiveResolution.CurrentScale);
            PostEffects.RenderTransition(Pipeline.SceneFbo, Pipeline.LayerFbo, progress, TransitionType, Pipeline.ComposeA);
            return Pipeline.ComposeA;
        }

        Pipeline.RenderShaderPass(active, Effects, ElapsedTime, MousePosition, Audio.Compute(Effects, ElapsedTime), Channel0Texture, Pipeline.SceneFbo, AdaptiveResolution.CurrentScale);
        return Pipeline.SceneFbo;
    }

    private Framebuffer RenderLayerChain(Framebuffer composed)
    {
        UpdateTextLayer();

        Framebuffer nextWrite = composed == Pipeline.ComposeA ? Pipeline.ComposeB : Pipeline.ComposeA;

        foreach (var layer in Layers)
        {
            if (!layer.Enabled) continue;

            if (layer.SourceType == LayerSourceType.Image)
            {
                if (string.IsNullOrEmpty(layer.ImagePath)) continue;
                Texture? image = GetOrLoadLayerImage(layer.ImagePath);
                if (image == null) continue;

                Pipeline.RenderImageLayerPass(image, layer.FitMode, layer.PositionX, layer.PositionY, layer.Scale, layer.Rotation, Pipeline.LayerFbo);
            }
            else if (layer.SourceType == LayerSourceType.Shader)
            {
                if (!_shaderPrograms.TryGetValue(layer.ShaderName, out var layerProgram)) continue;
                Pipeline.RenderShaderPass(layerProgram, Effects, ElapsedTime, MousePosition, Audio.Compute(Effects, ElapsedTime), Channel0Texture, Pipeline.LayerFbo, AdaptiveResolution.CurrentScale);
            }
            else if (layer.SourceType == LayerSourceType.Text)
            {
                if (_activeTextClip == null) continue;

                Pipeline.LayerFbo.Clear(0.0f, 0.0f, 0.0f, 0.0f);
                TextOverlay.Render(PostEffects, Pipeline.LayerFbo, _activeTextClip, ElapsedTime, Audio);
            }
            else if (layer.SourceType == LayerSourceType.Model3D)
            {
                if (string.IsNullOrEmpty(layer.ModelState.CurrentModelFilename)) continue;
                ModelRenderer renderer = GetOrCreateLayerModelRenderer(layer);
                if (renderer.Model == null) continue;

                Pipeline.LayerFbo.Clear(0.0f, 0.0f, 0.0f, 0.0f);
                float aspect = (float)Pipeline.Width / Pipeline.Height;
                layer.ModelState.ShowModel = true;
                renderer.Render(layer.ModelState, ElapsedTime, aspect, Channel0Texture);
            }
            else
            {
                continue;
            }

            Pipeline.BlendLayer(composed, Pipeline.LayerFbo, layer.BlendMode, layer.Opacity, nextWrite);
            composed = nextWrite;
            nextWrite = composed == Pipeline.ComposeA ? Pipeline.ComposeB : Pipeline.ComposeA;
        }

        return composed;
    }

    private void UpdateTextLayer()
    {
        TimelineClip? textClip = Timeline?.GetFilteredActiveClips(ElapsedTime).LastOrDefault(c => c.Type == ClipType.Text);
        _activeTextClip = textClip;
        TextOverlay.Update(textClip, Pipeline.Width, Pipeline.Height);

        if (textClip == null)
        {
            if (_textLayer != null)
            {
                Layers.Remove(_textLayer);
                _textLayer = null;
            }

            return;
        }

        if (_textLayer == null)
        {
            _textLayer = new Layer("", BlendMode.Normal, 1.0f) { SourceType = LayerSourceType.Text, IsTimelineManaged = true };
            Layers.Add(_textLayer);
        }
    }

    private ModelRenderer GetOrCreateLayerModelRenderer(Layer layer)
    {
        if (_layerModelRenderers.TryGetValue(layer, out var entry) &&
            entry.ModelPath == layer.ModelState.CurrentModelFilename &&
            entry.TexturePath == layer.ModelState.CurrentTextureFilename)
        {
            return entry.Renderer;
        }

        ModelRenderer renderer = entry.Renderer ?? new ModelRenderer(_gl);

        if (!string.IsNullOrEmpty(layer.ModelState.CurrentModelFilename))
        {
            renderer.LoadModel(layer.ModelState.CurrentModelFilename);
        }

        if (!string.IsNullOrEmpty(layer.ModelState.CurrentTextureFilename))
        {
            renderer.LoadTexture(layer.ModelState.CurrentTextureFilename);
        }

        _layerModelRenderers[layer] = (renderer, layer.ModelState.CurrentModelFilename, layer.ModelState.CurrentTextureFilename);
        return renderer;
    }

    private Texture? GetOrLoadLayerImage(string imagePath)
    {
        if (_layerImageCache.TryGetValue(imagePath, out Texture? cached)) return cached;

        Texture? loaded = TextureLoader.Load(_gl, imagePath, null, MaxLayerImageDimension);
        if (loaded == null) return null;

        _layerImageCache[imagePath] = loaded;
        return loaded;
    }

    private Framebuffer RenderAccumulationEffects(Framebuffer scene)
    {
        if (Effects.Datamosh > 0.0f)
        {
            PostEffects.RenderDatamosh(scene, _accumPing, _accumPong, Effects.Datamosh, ElapsedTime);
        }
        else if (Effects.FeedbackOpacity > 0.0f)
        {
            PostEffects.RenderFeedback(scene, _accumPing, _accumPong, Effects.FeedbackOpacity, Effects.FeedbackScale, Effects.FeedbackRotation);
        }
        else if (Effects.MotionBlur > 0.0f)
        {
            PostEffects.RenderMotionBlur(scene, _accumPing, _accumPong, Effects.MotionBlur);
        }
        else
        {
            return scene;
        }

        (_accumPing, _accumPong) = (_accumPong, _accumPing);
        return _accumPing;
    }

    public void Dispose()
    {
        foreach (var program in _shaderPrograms.Values)
        {
            program.Dispose();
        }

        _liveShaderProgram?.Dispose();
        foreach (var texture in _layerImageCache.Values)
        {
            texture.Dispose();
        }

        foreach (var entry in _layerModelRenderers.Values)
        {
            entry.Renderer.Dispose();
        }

        Recorder.Stop();
        Player.Dispose();
        Audio.Dispose();
        Channel0Texture.Dispose();
        Particles.Dispose();
        Model.Dispose();
        AudioViz.Dispose();
        TextOverlay.Dispose();
        _accumPing.Dispose();
        _accumPong.Dispose();
        PostEffects.Dispose();
        Profiler.Dispose();
        Pipeline.Dispose();
    }
}
