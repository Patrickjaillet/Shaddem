// Copyright (c) 2026 Patrick JAILLET
namespace ShaderDemo.Core.Rendering;

public static class GpuResourceTracker
{
    private static int _drawCallsThisFrame;
    private static long _estimatedVramBytes;

    public static int DrawCallsLastFrame { get; private set; }

    public static long EstimatedVramBytes => _estimatedVramBytes;

    public static void IncrementDrawCall()
    {
        _drawCallsThisFrame++;
    }

    public static void EndFrame()
    {
        DrawCallsLastFrame = _drawCallsThisFrame;
        _drawCallsThisFrame = 0;
    }

    public static void AddVram(long bytes)
    {
        _estimatedVramBytes += bytes;
    }

    public static void RemoveVram(long bytes)
    {
        _estimatedVramBytes -= bytes;
        if (_estimatedVramBytes < 0) _estimatedVramBytes = 0;
    }
}
