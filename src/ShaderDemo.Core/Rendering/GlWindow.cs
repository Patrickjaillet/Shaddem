// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
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
    public uint DockspaceId { get; private set; }
    public float ContentScale { get; private set; } = 1.0f;

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

    private static unsafe float QueryContentScale()
    {
        try
        {
            var glfw = Silk.NET.GLFW.Glfw.GetApi();
            var monitor = glfw.GetPrimaryMonitor();
            if (monitor == null) return 1.0f;

            glfw.GetMonitorContentScale(monitor, out float xscale, out float _);
            return xscale > 0.0f ? xscale : 1.0f;
        }
        catch
        {
            return 1.0f;
        }
    }

    private void OnLoad()
    {
        Api = _window.CreateOpenGL();
        Gui.NativeFileDialog.OwnerHwnd = _window.Native?.Win32?.Hwnd ?? 0;
        _inputContext = _window.CreateInput();
        Mouse = _inputContext.Mice.Count > 0 ? _inputContext.Mice[0] : null;
        Keyboard = _inputContext.Keyboards.Count > 0 ? _inputContext.Keyboards[0] : null;

        if (_enableImGui)
        {
            ContentScale = QueryContentScale();

            _imGuiController = new ImGuiController(Api, _window, _inputContext, null, () =>
            {
                Gui.Theme.LoadFonts(ImGui.GetIO(), ContentScale);
                ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            });
            Gui.Theme.Apply();

            if (MathF.Abs(ContentScale - 1.0f) > 0.01f)
            {
                ImGui.GetStyle().ScaleAllSizes(ContentScale);
            }
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

        if (_enableImGui)
        {
            DockspaceId = ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode, null);
        }

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

    public bool IsFullscreen { get; private set; }
    private Vector2D<int> _windowedPosition;
    private Vector2D<int> _windowedSize;

    public void ToggleFullscreen()
    {
        if (!IsFullscreen)
        {
            _windowedPosition = _window.Position;
            _windowedSize = _window.Size;

            IMonitor monitor = _window.Monitor ?? Silk.NET.Windowing.Monitor.GetMainMonitor(_window);
            _window.WindowBorder = WindowBorder.Hidden;
            _window.Position = monitor.Bounds.Origin;
            _window.Size = monitor.Bounds.Size;
        }
        else
        {
            _window.WindowBorder = WindowBorder.Resizable;
            _window.Position = _windowedPosition;
            _window.Size = _windowedSize;
        }

        IsFullscreen = !IsFullscreen;
    }

    public void Dispose()
    {
        _imGuiController?.Dispose();
        _window.Dispose();
    }
}
