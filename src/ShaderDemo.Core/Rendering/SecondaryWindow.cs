// Copyright (c) 2026 Patrick JAILLET
using Silk.NET.Input;
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
    private IInputContext? _input;

    public bool IsOpen => _window != null;
    public int DisplayIndex { get; set; }
    public bool Fullscreen { get; set; }
    public bool TopMost { get; set; }
    public bool HideCursor { get; set; }

    public SecondaryWindow(IWindow primaryWindow)
    {
        _primary = primaryWindow;
    }

    public IReadOnlyList<(int Index, string Name)> ListDisplays()
    {
        var result = new List<(int, string)>();
        int i = 0;
        foreach (IMonitor monitor in Silk.NET.Windowing.Monitor.GetMonitors(_primary))
        {
            result.Add((i, string.IsNullOrEmpty(monitor.Name) ? $"Display {i}" : monitor.Name));
            i++;
        }

        return result;
    }

    public void Open(string title, int width, int height, Action<string>? log = null)
    {
        if (_window != null) return;

        try
        {
            var monitors = Silk.NET.Windowing.Monitor.GetMonitors(_primary).ToList();
            IMonitor? target = DisplayIndex >= 0 && DisplayIndex < monitors.Count ? monitors[DisplayIndex] : null;

            var options = WindowOptions.Default;
            options.Title = title;
            options.Size = new Vector2D<int>(width, height);
            options.SharedContext = _primary.GLContext;
            options.IsVisible = true;
            options.VSync = false;

            if (target != null)
            {
                options.Position = target.Bounds.Origin;
            }

            if (Fullscreen)
            {
                options.WindowState = WindowState.Fullscreen;
            }

            _window = Window.Create(options);
            _window.Initialize();

            if (TopMost) _window.TopMost = true;

            _gl = GL.GetApi(_window);
            _quad = new FullscreenQuad(_gl);
            _program = new ShaderProgram(_gl, "SecondaryBlit", BuiltinShaders.VertexPassthrough, BuiltinShaders.FragmentTexture);

            if (HideCursor)
            {
                _input = _window.CreateInput();
                foreach (IMouse mouse in _input.Mice)
                {
                    mouse.Cursor.CursorMode = CursorMode.Hidden;
                }
            }
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
        _input?.Dispose();
        _quad?.Dispose();
        _program?.Dispose();
        _window?.Dispose();
        _window = null;
        _gl = null;
        _quad = null;
        _program = null;
        _input = null;
    }

    public void Dispose()
    {
        Close();
    }
}
