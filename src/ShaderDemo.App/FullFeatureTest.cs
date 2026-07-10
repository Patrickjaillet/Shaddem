// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using ShaderDemo.Core;
using ShaderDemo.Core.Audio;
using ShaderDemo.Core.Export;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.App;

public static class FullFeatureTest
{
    public static Task<int> Run(string[] args)
    {
        string shaderDirectory = Path.Combine(AppContext.BaseDirectory, "shaders");
        string audioPath = args.Length > 1 ? args[1] : "";

        using var window = new GlWindow(AppInfo.Name, 640, 360, fullscreen: false);
        ShaderManager? engine = null;
        var timeline = new TimelineEngine();
        var preview = new SecondaryWindow(window.NativeWindow);
        int frame = 0;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            engine.Timeline = timeline;
            int loaded = ShaderLoader.LoadFromDirectory(engine, shaderDirectory, Console.WriteLine);
            Console.WriteLine($"[test] Loaded {loaded} shaders");

            engine.SelectShader(0);

            engine.Effects.ParticlesActive = true;
            engine.Effects.ParticlesCount = 200;
            engine.Effects.ParticlesSize = 8f;
            engine.Effects.ParticlesColor = new Vector4(1, 1, 1, 0.8f);

            engine.Effects.Bloom = 1.0f;

            string modelPath = args.Length > 2 ? args[2] : Path.Combine(AppContext.BaseDirectory, "models", "cube.obj");
            engine.Model.LoadModel(modelPath, Console.WriteLine);
            engine.ModelState.ShowModel = true;
            engine.ModelState.AutoRotateSpeed = new Vector3(0.1f, 0.2f, 0.0f);
            Console.WriteLine($"[test] Model loaded: {(engine.Model.Model != null ? $"{engine.Model.Model.VertexCount} verts" : "FAILED")}");


            bool compiled = engine.CompileLiveShader("""
                void mainImage(out vec4 fragColor, in vec2 fragCoord) {
                    vec2 uv = fragCoord / iResolution.xy;
                    fragColor = vec4(uv, 0.5, 1.0);
                }
                """, Console.WriteLine);
            Console.WriteLine($"[test] Live shader compiled: {compiled}");

            timeline.Add(0, 100, ClipType.Text, "TEST OVERLAY", new Dictionary<string, object?>
            {
                ["size"] = 48.0,
                ["color"] = new double[] { 255, 255, 0 },
                ["position"] = "center",
            });
            timeline.Active = true;
            Console.WriteLine("[test] Timeline text clip added");

            if (audioPath.Length > 0 && File.Exists(audioPath))
            {
                var analysis = AudioAnalyzer.AnalyzeFull(audioPath);
                Console.WriteLine($"[test] Full analysis: {analysis.Spectrum.Length} spectrum frames, {analysis.Waveform.Length} waveform frames, duration={analysis.Duration:F1}s");
                engine.AudioViz.SetAnalysis(analysis, 0.0);
                engine.AudioViz.Enabled = true;
                engine.AudioViz.TrailDecay = 0.9f;
                engine.Audio.Load(audioPath, 0.0f);
                engine.Audio.Enabled = true;
            }
        };

        window.UpdateFrame += dt =>
        {
            engine?.Update(dt);
            if (engine != null && timeline.Active)
            {
                timeline.ApplyEffects(engine.Effects, engine.ElapsedTime);
            }
        };

        window.RenderFrame += _ =>
        {
            engine?.RenderFrame();
            frame++;

            if (frame == 10)
            {
                preview.Open("Preview", 320, 180, Console.WriteLine);
                Console.WriteLine($"[test] Preview window opened: {preview.IsOpen}");
            }

            if (frame >= 10 && preview.IsOpen)
            {
                preview.RenderFrame(engine?.LastComposedFrame);
            }

            if (frame == 30 && engine != null)
            {
                Console.WriteLine($"[test] Frame 30: IsTransitioning={engine.IsTransitioning}");
            }

            if (frame == 40)
            {
                preview.Close();
                Console.WriteLine($"[test] Preview window closed: {!preview.IsOpen}");
            }

            if (frame == 45 && engine != null)
            {
                engine.Effects.Datamosh = 0f;
                engine.Effects.FeedbackOpacity = 0.6f;
                engine.Effects.FeedbackScale = 1.02f;
                engine.Effects.FeedbackRotation = 0.01f;
                Console.WriteLine("[test] Switched to feedback pass");
            }

            if (frame == 90)
            {
                if (engine != null)
                {
                    string path = ScreenshotService.Save(engine.LastComposedFrame ?? engine.Pipeline.SceneFbo, "screenshots");
                    Console.WriteLine($"[test] Screenshot saved: {path}");
                }

                window.RequestClose();
            }
        };

        window.Closing += () =>
        {
            preview.Dispose();
            engine?.Dispose();
        };

        window.Run();
        Console.WriteLine("[test] Completed without crashing.");
        return Task.FromResult(0);
    }
}
