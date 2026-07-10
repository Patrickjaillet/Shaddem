// Copyright (c) 2026 Patrick JAILLET
using Silk.NET.Core.Native;
using Silk.NET.GLFW;
using Silk.NET.Windowing;

namespace ShaderDemo.Core.Rendering;

public sealed unsafe class DragDropHandler
{
    private Glfw? _glfw;

    public event Action<string[]>? FilesDropped;

    public void Attach(IWindow window)
    {
        nint? handle = window.Native?.Glfw;
        if (handle == null) return;

        _glfw = Glfw.GetApi();
        _glfw.SetDropCallback((WindowHandle*)handle.Value, OnDrop);
    }

    private void OnDrop(WindowHandle* windowHandle, int count, nint paths)
    {
        if (count <= 0) return;

        string[] files = SilkMarshal.PtrToStringArray(paths, count);
        FilesDropped?.Invoke(files);
    }
}
