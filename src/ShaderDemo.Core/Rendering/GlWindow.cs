// Copyright (c) 2026 Patrick JAILLET
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using Silk.NET.Input;

namespace ShaderDemo.Core.Rendering;

public sealed class GlWindow : IDisposable
{
    private readonly IWindow _window;
    private readonly bool _enableImGui;
    private IInputContext? _inputContext;
    private ImGuiController? _imGuiController;

    public GL? Api { get; private set; }
    public int Width => _window.Size.X;
    public int Height => _window.Size.Y;
    public IMouse? Mouse { get; private set; }
    public IKeyboard? Keyboard { get; private set; }
    public IWindow NativeWindow => _window;

    public event Action? Load;
    public event Action<double>? UpdateFrame;
    public event Action<double>? RenderFrame;
    public event Action? ImGuiRender;
    public event Action<int, int>? Resized;
    public event Action? Closing;

    public GlWindow(string title, int width, int height, bool fullscreen, bool enableImGui = true)
    {
        _enableImGui = enableImGui;
        var options = WindowOptions.Default;
        options.Title = title;
        options.Size = new Vector2D<int>(width, height);
        options.VSync = true;
        options.WindowState = fullscreen ? WindowState.Fullscreen : WindowState.Normal;

        _window = Window.Create(options);
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Resize += OnResize;
        _window.Closing += OnClosing;
    }

    private void OnLoad()
    {
        Api = _window.CreateOpenGL();
        _inputContext = _window.CreateInput();
        Mouse = _inputContext.Mice.Count > 0 ? _inputContext.Mice[0] : null;
        Keyboard = _inputContext.Keyboards.Count > 0 ? _inputContext.Keyboards[0] : null;

        if (_enableImGui)
        {
            _imGuiController = new ImGuiController(Api, _window, _inputContext);
            Gui.Theme.Apply();
        }

        Load?.Invoke();
    }

    private void OnUpdate(double deltaSeconds)
    {
        UpdateFrame?.Invoke(deltaSeconds);
    }

    private void OnRender(double deltaSeconds)
    {
        _imGuiController?.Update((float)deltaSeconds);
        RenderFrame?.Invoke(deltaSeconds);
        ImGuiRender?.Invoke();
        _imGuiController?.Render();
    }

    private void OnResize(Vector2D<int> size)
    {
        Api?.Viewport(size);
        Resized?.Invoke(size.X, size.Y);
    }

    private void OnClosing()
    {
        Closing?.Invoke();
    }

    public void Run()
    {
        _window.Run();
    }

    public void RequestClose()
    {
        _window.Close();
    }

    public void Dispose()
    {
        _imGuiController?.Dispose();
        _window.Dispose();
    }
}
