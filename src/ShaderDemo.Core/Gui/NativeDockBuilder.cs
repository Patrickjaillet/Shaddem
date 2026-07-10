// Copyright (c) 2026 Patrick JAILLET
using System.Numerics;
using System.Runtime.InteropServices;

namespace ShaderDemo.Core.Gui;

public static unsafe class NativeDockBuilder
{
    private const string LibName = "cimgui";

    private enum Dir { Left = 0, Right = 1, Up = 2, Down = 3 }

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern uint igDockBuilderAddNode(uint node_id, int flags);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void igDockBuilderRemoveNode(uint node_id);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void igDockBuilderSetNodeSize(uint node_id, Vector2 size);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern uint igDockBuilderSplitNode(uint node_id, int split_dir, float size_ratio_for_node_at_dir, uint* out_id_at_dir, uint* out_id_at_opposite_dir);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void igDockBuilderDockWindow([MarshalAs(UnmanagedType.LPUTF8Str)] string window_name, uint node_id);

    [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
    private static extern void igDockBuilderFinish(uint node_id);

    public enum LayoutPreset { Simple, Standard, PowerUser }

    public static void ApplyPreset(uint dockspaceId, LayoutPreset preset, Vector2 viewportSize)
    {
        igDockBuilderRemoveNode(dockspaceId);
        igDockBuilderAddNode(dockspaceId, 1 << 10);
        igDockBuilderSetNodeSize(dockspaceId, viewportSize);

        uint root = dockspaceId;

        float navRatio = preset == LayoutPreset.Simple ? 0.14f : 0.16f;
        uint navId, afterNav;
        igDockBuilderSplitNode(root, (int)Dir.Left, navRatio, &navId, &afterNav);

        float inspectorRatioOfRemainder = preset switch
        {
            LayoutPreset.Simple => 0.20f,
            LayoutPreset.PowerUser => 0.30f,
            _ => 0.25f,
        };

        uint inspectorId, centerId;
        igDockBuilderSplitNode(afterNav, (int)Dir.Right, inspectorRatioOfRemainder, &inspectorId, &centerId);

        uint previewId = centerId;
        if (preset == LayoutPreset.PowerUser)
        {
            uint timelineId, previewOnly;
            igDockBuilderSplitNode(centerId, (int)Dir.Down, 0.22f, &timelineId, &previewOnly);

            previewId = previewOnly;
            igDockBuilderDockWindow("Timeline Dock", timelineId);
        }

        igDockBuilderDockWindow("Navigation", navId);
        igDockBuilderDockWindow("Preview", previewId);
        igDockBuilderDockWindow("Inspector", inspectorId);

        igDockBuilderFinish(dockspaceId);
    }
}
