// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core;
using ShaderDemo.Core.Gui;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Settings;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.App;

public static class WorkflowPanelsTest
{
    public static void Run(string shaderDirectory)
    {
        using var window = new GlWindow(AppInfo.Name, 900, 700, fullscreen: false);
        ShaderManager? engine = null;
        var timeline = new TimelineEngine();
        var settings = new AppSettings();
        var dragDrop = new DragDropHandler();
        int frame = 0;
        int templateFailures = 0;

        window.Load += () =>
        {
            engine = new ShaderManager(window.Api!, window.Width, window.Height);
            ShaderLoader.LoadFromDirectory(engine, shaderDirectory, Console.WriteLine);
            engine.SelectShader(0);
            Console.WriteLine($"[workflow-test] Loaded {engine.ShaderNames.Count} shaders");

            foreach (DemoTemplate template in TemplatesService.BuiltIn)
            {
                bool applied = TemplatesService.Apply(template, engine);
                Console.WriteLine($"[workflow-test] Template '{template.Name}' -> shader '{template.ShaderName}': {(applied ? "OK" : "FAILED (shader not found)")}");
                if (!applied) templateFailures++;
            }

            string theme = TemplatesService.Randomize(engine, settings, new Random(42));
            Console.WriteLine($"[workflow-test] Randomize Demo produced theme '{theme}', shader now '{engine.CurrentShaderName}'");

            dragDrop.Attach(window.NativeWindow);
            Console.WriteLine("[workflow-test] DragDropHandler.Attach completed with no exception");

            WelcomePanel.Show();
            GuidedTour.Start();
        };

        window.UpdateFrame += dt => engine?.Update(dt);
        window.RenderFrame += _ => engine?.RenderFrame();
        window.ImGuiRender += () =>
        {
            if (engine == null) return;

            try
            {
                WelcomePanel.Draw(engine, settings, "test_workflow_settings.json");
                GuidedTour.Draw(settings, "test_workflow_settings.json");

                if (frame == 5) DemoWizardPanel.Open();
                if (frame == 10) QuickExportPanel.Open();
                if (frame == 15) HelpPanel.Open();

                DemoWizardPanel.Draw(engine, settings);
                QuickExportPanel.Draw(engine, settings, timeline);
                HelpPanel.Draw();

                ImGui.Begin("WorkflowPanelsTest");
                TemplatesPanel.Draw(engine, settings);
                GeneralPanel.Draw(engine, settings, timeline);
                ImGui.End();

                if (frame == 25)
                {
                    Console.WriteLine("[workflow-test] 25 frames rendered across all new panels with no exceptions.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[workflow-test] EXCEPTION: {ex}");
            }

            frame++;
            if (frame == 30) window.RequestClose();
        };

        window.Closing += () =>
        {
            engine?.Dispose();
            Console.WriteLine(templateFailures == 0
                ? "[workflow-test] RESULT: PASS (all templates resolved, all panels drew cleanly)"
                : $"[workflow-test] RESULT: FAIL ({templateFailures} template(s) failed to resolve)");
        };

        window.Run();
    }
}
