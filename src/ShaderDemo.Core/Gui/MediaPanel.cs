// Copyright (c) 2026 Patrick JAILLET
using ImGuiNET;
using ShaderDemo.Core.Logging;
using ShaderDemo.Core.Rendering;

namespace ShaderDemo.Core.Gui;

public static class MediaPanel
{
    private static string _texturePath = "";
    private static string _browseDirectory = "media";
    internal static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
    private static readonly string[] VideoExtensions = { ".mp4", ".avi", ".mov", ".mkv", ".webm" };
    internal static readonly Dictionary<string, Texture> _thumbnailCache = new();
    internal static readonly AsyncTextureLoader _asyncLoader = new();
    private static string[] _imageFiles = Array.Empty<string>();
    private static string[] _videoFiles = Array.Empty<string>();

    public static void Draw(ShaderManager manager)
    {
        ImGui.Text("iChannel0 Texture");
        ImGui.InputText("Image Path", ref _texturePath, 512);
        ImGui.SameLine();
        if (ImGui.Button("Browse...##Media"))
        {
            string filter = NativeFileDialog.BuildFilter(("Image Files", ImageExtensions), ("Video Files", VideoExtensions));
            string? picked = NativeFileDialog.OpenFile("Select an image or video file", filter, Path.GetDirectoryName(_texturePath) ?? _browseDirectory);
            if (picked != null) _texturePath = picked;
        }

        FileValidationHint.Draw(_texturePath, ImageExtensions);

        if (ImGui.Button("Load"))
        {
            bool loaded = manager.LoadChannel0Texture(_texturePath, AppLog.Info);
            ToastManager.Show(loaded ? $"Texture loaded: {Path.GetFileName(_texturePath)}" : "Failed to load texture", loaded ? ToastLevel.Success : ToastLevel.Danger);
        }

        ImGui.Text($"Current: {manager.Channel0Texture.Width}x{manager.Channel0Texture.Height}");

        ImGui.Separator();
        ImGui.Text("Media Browser (images / video files)");
        ImGui.InputText("Directory", ref _browseDirectory, 256);
        ImGui.SameLine();
        if (ImGui.Button("Refresh"))
        {
            RefreshFileLists();
        }

        if (_imageFiles.Length == 0 && _videoFiles.Length == 0)
        {
            ImGui.TextColored(Theme.TextMuted, "No media found. Set a directory and click Refresh.");
            return;
        }

        if (_imageFiles.Length > 0)
        {
            _asyncLoader.UploadReady(manager.Gl, (path, texture) => _thumbnailCache[path] = texture);

            ImGui.Text($"Images ({_asyncLoader.PendingCount} decoding in background)");
            ImGui.BeginChild("ImageThumbnails", new System.Numerics.Vector2(0, 180), true);
            foreach (string file in _imageFiles)
            {
                Texture? thumb = GetThumbnail(file);
                if (thumb != null)
                {
                    ImGui.Image((nint)thumb.Handle, new System.Numerics.Vector2(64, 64));
                    ImGui.SameLine();
                }
                else
                {
                    ImGui.TextColored(Theme.TextMuted, "...");
                    ImGui.SameLine();
                }

                ImGui.BeginGroup();
                ImGui.Text(Path.GetFileName(file));
                if (ImGui.Button($"Use##{file}"))
                {
                    _texturePath = file;
                    manager.LoadChannel0Texture(file, AppLog.Info);
                }

                ImGui.EndGroup();
            }

            ImGui.EndChild();
        }

        if (_videoFiles.Length > 0)
        {
            ImGui.Text("Video files (no in-engine video texture support yet — path copy only)");
            ImGui.BeginChild("VideoFiles", new System.Numerics.Vector2(0, 120), true);
            foreach (string file in _videoFiles)
            {
                ImGui.Text(Path.GetFileName(file));
                ImGui.SameLine();
                if (ImGui.Button($"Copy Path##{file}"))
                {
                    ImGui.SetClipboardText(file);
                }
            }

            ImGui.EndChild();
        }
    }

    private static void RefreshFileLists()
    {
        foreach (Texture tex in _thumbnailCache.Values) tex.Dispose();
        _thumbnailCache.Clear();

        if (!Directory.Exists(_browseDirectory))
        {
            _imageFiles = Array.Empty<string>();
            _videoFiles = Array.Empty<string>();
            return;
        }

        string[] allFiles = Directory.GetFiles(_browseDirectory);
        _imageFiles = allFiles.Where(f => ImageExtensions.Contains(Path.GetExtension(f).ToLowerInvariant())).ToArray();
        _videoFiles = allFiles.Where(f => VideoExtensions.Contains(Path.GetExtension(f).ToLowerInvariant())).ToArray();
    }

    internal static Texture? GetThumbnail(string file)
    {
        if (_thumbnailCache.TryGetValue(file, out Texture? cached)) return cached;
        if (_thumbnailCache.Count >= 64) return null;

        _asyncLoader.RequestLoad(file);
        return null;
    }
}
