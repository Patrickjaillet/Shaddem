// Copyright (c) 2026 Patrick JAILLET
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace ShaderDemo.Core.Rendering;

public sealed class SecondaryWindow : IDisposable
{
    private readonly IWindow _primary;
    private IWindow? _window;
    private GL? _gl;
    private FullscreenQuad? _quad;
    private ShaderProgram? _program;

    public bool IsOpen => _window != null;

    public SecondaryWindow(IWindow primaryWindow)
    {
        _primary = primaryWindow;
    }

    public void Open(string title, int width, int height, Action<string>? log = null)
    {
        if (_window != null) return;

        try
        {
            var options = WindowOptions.Default;
            options.Title = title;
            options.Size = new Vector2D<int>(width, height);
            options.SharedContext = _primary.GLContext;
            options.IsVisible = true;
            options.VSync = false;

            _window = Window.Create(options);
            _window.Initialize();

            _gl = GL.GetApi(_window);
            _quad = new FullscreenQuad(_gl);
            _program = new ShaderProgram(_gl, "SecondaryBlit", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentTexture);
        }
        catch (Exception ex)
        {
            log?.Invoke($"Failed to open secondary window: {ex.Message}");
            Close();
        }
    }

    public void RenderFrame(Framebuffer? source)
    {
        if (_window == null || _gl == null || _program == null || _quad == null || source == null) return;

        if (_window.IsClosing)
        {
            Close();
            return;
        }

        _window.DoEvents();

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _gl.Viewport(0, 0, (uint)_window.Size.X, (uint)_window.Size.Y);
        _gl.ClearColor(0f, 0f, 0f, 1f);
        _gl.Clear((uint)ClearBufferMask.ColorBufferBit);

        source.BindTexture(0);
        _program.Use();
        _program.SetUniform("tex", 0);
        _quad.Draw();

        _window.SwapBuffers();
    }

    public void Close()
    {
        _quad?.Dispose();
        _program?.Dispose();
        _window?.Dispose();
        _window = null;
        _gl = null;
        _quad = null;
        _program = null;
    }

    public void Dispose()
    {
        Close();
    }
}
