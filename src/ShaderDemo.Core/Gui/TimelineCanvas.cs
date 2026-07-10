// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using ImGuiNET;
using ShaderDemo.Core.Rendering;
using ShaderDemo.Core.Timeline;

namespace ShaderDemo.Core.Gui;

public static class TimelineCanvas
{
    private const float RulerHeight = 24.0f;
    private const float HeaderWidth = 90.0f;
    private const float EdgeGrabPixels = 6.0f;

    private static float _pixelsPerSecond = 20.0f;
    private static float _scrollSeconds;

    private enum DragMode
    {
        None,
        Move,
        ResizeLeft,
        ResizeRight,
        Playhead,
    }

    private static TimelineClip? _dragClip;
    private static DragMode _dragMode = DragMode.None;
    private static float _dragStartMouseX;
    private static double _dragStartClipStart;
    private static double _dragStartClipDuration;
    private static TimelineClip? _selectedClip;

    private static readonly Dictionary<ClipType, Vector4> ClipColors = new()
    {
        [ClipType.Shader] = Theme.ClipShader,
        [ClipType.Image] = Theme.ClipImage,
        [ClipType.Music] = Theme.ClipMusic,
        [ClipType.Effect] = Theme.ClipEffect,
        [ClipType.Text] = Theme.ClipText,
        [ClipType.Model3D] = Theme.ClipModel3D,
        [ClipType.LayerAutomation] = Theme.ClipLayerAutomation,
    };

    public static void Draw(TimelineEngine timeline, ShaderManager manager)
    {
        ImGui.SliderFloat("Zoom (px/s)", ref _pixelsPerSecond, 2.0f, 200.0f);

        double maxEnd = timeline.Clips.Count > 0 ? timeline.Clips.Max(c => c.End) : 60.0;
        float maxScroll = (float)Math.Max(0.0, maxEnd - 5.0);
        ImGui.SliderFloat("Scroll (s)", ref _scrollSeconds, 0.0f, Math.Max(1.0f, maxScroll));

        float totalTrackHeight = timeline.Tracks.Sum(t => t.Height);
        var canvasSize = new Vector2(ImGui.GetContentRegionAvail().X, RulerHeight + totalTrackHeight + 20.0f);

        Vector2 origin = ImGui.GetCursorScreenPos();
        DrawTrackHeaders(timeline, origin, totalTrackHeight);

        Vector2 canvasPos = origin + new Vector2(HeaderWidth, 0);
        float canvasWidth = canvasSize.X - HeaderWidth;

        ImGui.SetCursorScreenPos(canvasPos);
        ImGui.InvisibleButton("##timeline_canvas", new Vector2(canvasWidth, canvasSize.Y));
        bool hovered = ImGui.IsItemHovered();
        bool active = ImGui.IsItemActive();
        Vector2 mouse = ImGui.GetIO().MousePos;

        ImDrawListPtr drawList = ImGui.GetWindowDrawList();
        drawList.AddRectFilled(canvasPos, canvasPos + new Vector2(canvasWidth, canvasSize.Y), ImGui.GetColorU32(Theme.SurfaceBase with { W = 1.0f }));

        DrawRuler(drawList, canvasPos, canvasWidth);
        DrawMarkers(timeline, drawList, canvasPos, canvasWidth);
        DrawTracksAndClips(timeline, drawList, canvasPos, canvasWidth, hovered, active, mouse);
        DrawPlayhead(manager, drawList, canvasPos, canvasSize.Y);

        HandlePlayheadScrub(manager, canvasPos, canvasWidth, hovered, active, mouse);

        ImGui.SetCursorScreenPos(origin + new Vector2(0, canvasSize.Y + 4));

        if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
        {
            _dragClip = null;
            _dragMode = DragMode.None;
        }
    }

    private static void DrawTrackHeaders(TimelineEngine timeline, Vector2 origin, float totalTrackHeight)
    {
        ImGui.SetCursorScreenPos(origin);
        ImGui.BeginChild("##track_headers", new Vector2(HeaderWidth, RulerHeight + totalTrackHeight + 20.0f), false, ImGuiWindowFlags.NoScrollbar);
        ImGui.Dummy(new Vector2(HeaderWidth, RulerHeight));

        for (int i = 0; i < timeline.Tracks.Count; i++)
        {
            Track track = timeline.Tracks[i];
            ImGui.PushID(i);
            ImGui.BeginGroup();
            ImGui.TextUnformatted(track.Name);
            bool mute = track.Mute;
            if (ImGui.Checkbox("M", ref mute)) track.Mute = mute;
            ImGui.SameLine();
            bool solo = track.Solo;
            if (ImGui.Checkbox("S", ref solo)) track.Solo = solo;
            ImGui.EndGroup();

            float used = ImGui.GetItemRectSize().Y;
            if (used < track.Height)
            {
                ImGui.Dummy(new Vector2(1, track.Height - used));
            }

            ImGui.PopID();
        }

        ImGui.EndChild();
    }

    private static void DrawRuler(ImDrawListPtr drawList, Vector2 canvasPos, float canvasWidth)
    {
        float step = _pixelsPerSecond >= 100 ? 1.0f : _pixelsPerSecond >= 40 ? 5.0f : _pixelsPerSecond >= 15 ? 10.0f : _pixelsPerSecond >= 5 ? 30.0f : 60.0f;
        double firstTick = Math.Floor(_scrollSeconds / step) * step;
        ImFontPtr tickFont = Theme.FontMono ?? ImGui.GetFont();

        for (double t = firstTick; ; t += step)
        {
            float x = canvasPos.X + ((float)(t - _scrollSeconds) * _pixelsPerSecond);
            if (x > canvasPos.X + canvasWidth) break;
            if (x < canvasPos.X) continue;

            drawList.AddLine(new Vector2(x, canvasPos.Y), new Vector2(x, canvasPos.Y + RulerHeight), ImGui.GetColorU32(new Vector4(1, 1, 1, 0.25f)));
            drawList.AddText(tickFont, 12.0f, new Vector2(x + 2, canvasPos.Y + 2), ImGui.GetColorU32(new Vector4(1, 1, 1, 0.6f)), $"{t:F0}s");
        }
    }

    private static void DrawMarkers(TimelineEngine timeline, ImDrawListPtr drawList, Vector2 canvasPos, float canvasWidth)
    {
        ImFontPtr labelFont = Theme.FontSemibold ?? ImGui.GetFont();

        foreach (TimelineMarker marker in timeline.Markers)
        {
            float x = canvasPos.X + ((float)(marker.Time - _scrollSeconds) * _pixelsPerSecond);
            if (x < canvasPos.X || x > canvasPos.X + canvasWidth) continue;

            uint col = ImGui.GetColorU32(marker.Color);
            drawList.AddTriangleFilled(
                new Vector2(x - 5, canvasPos.Y),
                new Vector2(x + 5, canvasPos.Y),
                new Vector2(x, canvasPos.Y + 8),
                col);

            Vector2 labelSize = ImGui.CalcTextSize(marker.Label);
            Vector2 labelPos = new(x + 4, canvasPos.Y + 8);
            drawList.AddRectFilled(labelPos - new Vector2(2, 1), labelPos + labelSize + new Vector2(2, 1), ImGui.GetColorU32(new Vector4(0.0f, 0.0f, 0.0f, 0.55f)), 2.0f);
            drawList.AddText(labelFont, 13.0f, labelPos, col, marker.Label);
        }
    }

    private static void DrawTracksAndClips(TimelineEngine timeline, ImDrawListPtr drawList, Vector2 canvasPos, float canvasWidth, bool hovered, bool active, Vector2 mouse)
    {
        float yOffset = RulerHeight;

        foreach (Track track in timeline.Tracks)
        {
            float rowY0 = canvasPos.Y + yOffset;
            float rowY1 = rowY0 + track.Height;
            drawList.AddRectFilled(new Vector2(canvasPos.X, rowY0), new Vector2(canvasPos.X + canvasWidth, rowY1), ImGui.GetColorU32(track.Background));

            foreach (TimelineClip clip in timeline.Clips.Where(c => track.Types.Contains(c.Type)))
            {
                float x1 = canvasPos.X + ((float)(clip.Start - _scrollSeconds) * _pixelsPerSecond);
                float x2 = canvasPos.X + ((float)(clip.End - _scrollSeconds) * _pixelsPerSecond);
                if (x2 < canvasPos.X || x1 > canvasPos.X + canvasWidth)
                {
                    continue;
                }

                Vector4 baseColor = ClipColors.TryGetValue(clip.Type, out Vector4 c) ? c : new Vector4(0.5f, 0.5f, 0.5f, 0.9f);
                uint fillCol = ImGui.GetColorU32(baseColor);
                drawList.AddRectFilled(new Vector2(x1, rowY0 + 2), new Vector2(x2, rowY1 - 2), fillCol, 3.0f);

                bool selected = ReferenceEquals(clip, _selectedClip) || ReferenceEquals(clip, _dragClip);
                if (selected)
                {
                    drawList.AddRect(new Vector2(x1, rowY0 + 2), new Vector2(x2, rowY1 - 2), ImGui.GetColorU32(new Vector4(1, 1, 1, 1)), 3.0f, ImDrawFlags.None, 2.0f);
                }

                drawList.PushClipRect(new Vector2(x1, rowY0), new Vector2(x2, rowY1), true);
                drawList.AddText(Theme.FontMono ?? ImGui.GetFont(), 13.0f, new Vector2(x1 + 4, rowY0 + 4), ImGui.GetColorU32(new Vector4(0, 0, 0, 1)), clip.Resource);
                drawList.PopClipRect();

                if (hovered && mouse.Y >= rowY0 && mouse.Y <= rowY1 && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && _dragMode == DragMode.None)
                {
                    if (Math.Abs(mouse.X - x1) <= EdgeGrabPixels)
                    {
                        BeginDrag(timeline, clip, DragMode.ResizeLeft, mouse.X);
                    }
                    else if (Math.Abs(mouse.X - x2) <= EdgeGrabPixels)
                    {
                        BeginDrag(timeline, clip, DragMode.ResizeRight, mouse.X);
                    }
                    else if (mouse.X > x1 && mouse.X < x2)
                    {
                        BeginDrag(timeline, clip, DragMode.Move, mouse.X);
                    }
                }
            }

            yOffset += track.Height;
        }

        if (active && _dragClip != null && _dragMode != DragMode.None && _dragMode != DragMode.Playhead)
        {
            float deltaSeconds = (mouse.X - _dragStartMouseX) / _pixelsPerSecond;
            switch (_dragMode)
            {
                case DragMode.Move:
                    _dragClip.Start = Math.Max(0.0, _dragStartClipStart + deltaSeconds);
                    break;
                case DragMode.ResizeLeft:
                    double newStart = Math.Clamp(_dragStartClipStart + deltaSeconds, 0.0, _dragStartClipStart + _dragStartClipDuration - 0.05);
                    _dragClip.Duration = (_dragStartClipStart + _dragStartClipDuration) - newStart;
                    _dragClip.Start = newStart;
                    break;
                case DragMode.ResizeRight:
                    _dragClip.Duration = Math.Max(0.05, _dragStartClipDuration + deltaSeconds);
                    break;
            }
        }
    }

    private static void BeginDrag(TimelineEngine timeline, TimelineClip clip, DragMode mode, float mouseX)
    {
        timeline.BeginEdit();
        _selectedClip = clip;
        _dragClip = clip;
        _dragMode = mode;
        _dragStartMouseX = mouseX;
        _dragStartClipStart = clip.Start;
        _dragStartClipDuration = clip.Duration;
    }

    private static void DrawPlayhead(ShaderManager manager, ImDrawListPtr drawList, Vector2 canvasPos, float canvasHeight)
    {
        float x = canvasPos.X + ((manager.ElapsedTime - _scrollSeconds) * _pixelsPerSecond);
        uint col = ImGui.GetColorU32(new Vector4(1, 0.2f, 0.2f, 1));

        drawList.AddLine(new Vector2(x, canvasPos.Y), new Vector2(x, canvasPos.Y + canvasHeight), col, 2.0f);

        const float flagHalfWidth = 6.0f;
        const float flagHeight = 10.0f;
        drawList.AddTriangleFilled(
            new Vector2(x - flagHalfWidth, canvasPos.Y),
            new Vector2(x + flagHalfWidth, canvasPos.Y),
            new Vector2(x, canvasPos.Y + flagHeight),
            col);

        string label = $"{manager.ElapsedTime:F1}s";
        ImFontPtr labelFont = Theme.FontMono ?? ImGui.GetFont();
        Vector2 labelSize = ImGui.CalcTextSize(label) * (12.0f / ImGui.GetFontSize());
        Vector2 labelPos = new(x - labelSize.X * 0.5f, canvasPos.Y + flagHeight + 2.0f);
        drawList.AddRectFilled(labelPos - new Vector2(2, 1), labelPos + labelSize + new Vector2(2, 1), col, 2.0f);
        drawList.AddText(labelFont, 12.0f, labelPos, ImGui.GetColorU32(new Vector4(0, 0, 0, 1)), label);
    }

    private static void HandlePlayheadScrub(ShaderManager manager, Vector2 canvasPos, float canvasWidth, bool hovered, bool active, Vector2 mouse)
    {
        if (hovered && mouse.Y <= canvasPos.Y + RulerHeight && ImGui.IsMouseClicked(ImGuiMouseButton.Left) && _dragMode == DragMode.None)
        {
            _dragMode = DragMode.Playhead;
        }

        if (active && _dragMode == DragMode.Playhead)
        {
            float t = _scrollSeconds + ((mouse.X - canvasPos.X) / _pixelsPerSecond);
            manager.SetElapsedTime(Math.Max(0.0f, t));
        }
    }
}
