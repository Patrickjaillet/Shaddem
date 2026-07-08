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

    private Framebuffer _accumPing;
    private Framebuffer _accumPong;

    public RenderPipeline Pipeline { get; }
    public PostEffectsPipeline PostEffects { get; }
    public List<Layer> Layers { get; } = new();
    public EffectParams Effects { get; } = new();
    public AudioReactivity Audio { get; } = new();
    public AudioPlayer Player { get; } = new();
    public VideoRecorder Recorder { get; } = new();
    public Framebuffer? LastComposedFrame { get; private set; }
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
        Channel0Texture = TextureLoader.CreateNoiseTexture(gl);
        Particles = new ParticleSystem(gl);
        Model = new ModelRenderer(gl);
        AudioViz = new AudioOverlay(gl);
        TextOverlay = new TextOverlay(gl);

        _accumPing = Framebuffer.Create(gl, width, height);
        _accumPong = Framebuffer.Create(gl, width, height);
    }

    public void LoadChannel0Texture(string path, Action<string>? log = null)
    {
        Texture? loaded = TextureLoader.Load(_gl, path, log);
        if (loaded == null) return;

        Channel0Texture.Dispose();
        Channel0Texture = loaded;
    }

    public string? CurrentShaderName => _shaderNames.Count > CurrentShaderIndex ? _shaderNames[CurrentShaderIndex] : null;
    public IReadOnlyList<string> ShaderNames => _shaderNames;

    public void RegisterShader(string name, string fragmentSource)
    {
        string wrapped = ShaderWrapper.Wrap(fragmentSource);
        var program = new ShaderProgram(_gl, name, BuiltinShaders.VertexPassthrough, wrapped);

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
    }

    public void RenderFrame()
    {
        Framebuffer composed = RenderBaseAndTransition();
        composed = RenderLayerChain(composed);

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

        if (Effects.Bloom > 0.0f)
        {
            Framebuffer bloomTarget = composed == Pipeline.ComposeA ? Pipeline.ComposeB : Pipeline.ComposeA;
            PostEffects.RenderBloom(composed, bloomTarget, Effects.Bloom);
            composed = bloomTarget;
        }

        composed = RenderAccumulationEffects(composed);

        if (AudioViz.Enabled)
        {
            AudioViz.Render(PostEffects, composed, ElapsedTime, Pipeline.Width, Pipeline.Height);
        }

        if (Timeline != null)
        {
            TimelineClip? textClip = Timeline.GetActiveClips(ElapsedTime).LastOrDefault(c => c.Type == ClipType.Text);
            TextOverlay.Update(textClip, Pipeline.Width, Pipeline.Height);
            if (textClip != null)
            {
                TextOverlay.Render(PostEffects, composed, textClip, ElapsedTime, Audio);
            }
        }

        Pipeline.PresentToScreen(composed);
        LastComposedFrame = composed;

        if (Recorder.IsRecording)
        {
            Recorder.WriteFrame(composed.ReadPixelsRgb());
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
                Pipeline.RenderShaderPass(active, Effects, ElapsedTime, MousePosition, Audio.Compute(Effects, ElapsedTime), Channel0Texture, Pipeline.SceneFbo);
                return Pipeline.SceneFbo;
            }

            ShaderProgram? prev = _shaderNames.Count > _prevShaderIndex ? _shaderPrograms[_shaderNames[_prevShaderIndex]] : null;
            Pipeline.RenderShaderPass(prev, Effects, ElapsedTime, MousePosition, Audio.Compute(Effects, ElapsedTime), Channel0Texture, Pipeline.SceneFbo);
            Pipeline.RenderShaderPass(active, Effects, ElapsedTime, MousePosition, Audio.Compute(Effects, ElapsedTime), Channel0Texture, Pipeline.LayerFbo);
            PostEffects.RenderTransition(Pipeline.SceneFbo, Pipeline.LayerFbo, progress, TransitionType, Pipeline.ComposeA);
            return Pipeline.ComposeA;
        }

        Pipeline.RenderShaderPass(active, Effects, ElapsedTime, MousePosition, Audio.Compute(Effects, ElapsedTime), Channel0Texture, Pipeline.SceneFbo);
        return Pipeline.SceneFbo;
    }

    private Framebuffer RenderLayerChain(Framebuffer composed)
    {
        Framebuffer nextWrite = composed == Pipeline.ComposeA ? Pipeline.ComposeB : Pipeline.ComposeA;

        foreach (var layer in Layers)
        {
            if (!layer.Enabled) continue;
            if (!_shaderPrograms.TryGetValue(layer.ShaderName, out var layerProgram)) continue;

            Pipeline.RenderShaderPass(layerProgram, Effects, ElapsedTime, MousePosition, Audio.Compute(Effects, ElapsedTime), Channel0Texture, Pipeline.LayerFbo);
            Pipeline.BlendLayer(composed, Pipeline.LayerFbo, layer.BlendMode, layer.Opacity, nextWrite);
            composed = nextWrite;
            nextWrite = composed == Pipeline.ComposeA ? Pipeline.ComposeB : Pipeline.ComposeA;
        }

        return composed;
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
        Recorder.Stop();
        Player.Dispose();
        Channel0Texture.Dispose();
        Particles.Dispose();
        Model.Dispose();
        AudioViz.Dispose();
        TextOverlay.Dispose();
        _accumPing.Dispose();
        _accumPong.Dispose();
        PostEffects.Dispose();
        Pipeline.Dispose();
    }
}
