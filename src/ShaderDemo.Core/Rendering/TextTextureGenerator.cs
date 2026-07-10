// Copyright (c) 2026 Patrick JAILLET
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using Silk.NET.OpenGL;

namespace ShaderDemo.Core.Rendering;

[SupportedOSPlatform("windows")]
public static class TextTextureGenerator
{
    public static Texture? Create(
        GL gl,
        int width,
        int height,
        string text,
        int fontSize,
        System.Numerics.Vector3 colorRgb255,
        string position,
        Action<string>? log = null)
    {
        try
        {
            using var bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
            using (var font = new Font("Arial", fontSize * 0.75f, GraphicsUnit.Pixel))
            using (var brush = new SolidBrush(Color.FromArgb(255, (int)colorRgb255.X, (int)colorRgb255.Y, (int)colorRgb255.Z)))
            using (var format = new StringFormat())
            {
                graphics.Clear(Color.Transparent);
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                bool centered = position == "center";
                format.Alignment = centered ? StringAlignment.Center : StringAlignment.Near;
                format.LineAlignment = centered ? StringAlignment.Center : StringAlignment.Near;

                var layoutRect = centered
                    ? new RectangleF(0, 0, width, height)
                    : new RectangleF(0, 0, width, height);

                graphics.DrawString(text, font, brush, layoutRect, format);
            }

            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            try
            {
                byte[] pixels = new byte[width * height * 4];
                System.Runtime.InteropServices.Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                BgraToRgba(pixels);
                return Texture.Create(gl, width, height, pixels, hasAlpha: true, buildMipmaps: false);
            }
            finally
            {
                bitmap.UnlockBits(data);
            }
        }
        catch (Exception ex)
        {
            log?.Invoke($"Failed to create text texture: {ex.Message}");
            return null;
        }
    }

    private static void BgraToRgba(byte[] pixels)
    {
        for (int i = 0; i < pixels.Length; i += 4)
        {
            (pixels[i], pixels[i + 2]) = (pixels[i + 2], pixels[i]);
        }
    }
}
