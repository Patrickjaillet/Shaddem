// Copyright (c) 2026 Patrick JAILLET
using System.Runtime.InteropServices;
using System.Text;

namespace ShaderDemo.Core.Gui;

public static class NativeFileDialog
{
    public static nint OwnerHwnd { get; set; }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct OpenFileName
    {
        public int lStructSize;
        public nint hwndOwner;
        public nint hInstance;
        public string? lpstrFilter;
        public string? lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public nint lpstrFile;
        public int nMaxFile;
        public string? lpstrFileTitle;
        public int nMaxFileTitle;
        public string? lpstrInitialDir;
        public string? lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public string? lpstrDefExt;
        public nint lCustData;
        public nint lpfnHook;
        public string? lpTemplateName;
        public nint pvReserved;
        public int dwReserved;
        public int FlagsEx;
    }

    private const int OFN_FILEMUSTEXIST = 0x00001000;
    private const int OFN_PATHMUSTEXIST = 0x00000800;
    private const int OFN_EXPLORER = 0x00080000;
    private const int OFN_NOCHANGEDIR = 0x00000008;

    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool GetOpenFileNameW(ref OpenFileName ofn);

    public static string BuildFilter(params (string Label, string[] Extensions)[] groups)
    {
        var sb = new StringBuilder();
        foreach (var (label, extensions) in groups)
        {
            string pattern = string.Join(";", extensions.Select(e => "*" + e));
            sb.Append(label).Append(" (").Append(pattern).Append(')').Append('\0').Append(pattern).Append('\0');
        }

        sb.Append("All Files (*.*)").Append('\0').Append("*.*").Append('\0');
        sb.Append('\0');
        return sb.ToString();
    }

    public static string? OpenFile(string title, string filter, string? initialDirectory = null, nint? ownerHwnd = null)
    {
        nint owner = ownerHwnd ?? OwnerHwnd;

        var buffer = Marshal.AllocHGlobal(2048);
        try
        {
            Marshal.WriteInt16(buffer, 0, 0);

            var ofn = new OpenFileName
            {
                lStructSize = Marshal.SizeOf<OpenFileName>(),
                hwndOwner = owner,
                lpstrFilter = filter,
                lpstrFile = buffer,
                nMaxFile = 1024,
                lpstrInitialDir = initialDirectory != null && Directory.Exists(initialDirectory) ? Path.GetFullPath(initialDirectory) : null,
                lpstrTitle = title,
                Flags = OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_EXPLORER | OFN_NOCHANGEDIR,
            };

            if (!GetOpenFileNameW(ref ofn)) return null;

            return Marshal.PtrToStringUni(buffer);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
