namespace LocalPaper;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using SkiaSharp;

internal class DeviceDisplay {
    public DeviceDisplay(string deviceId, IEnumerable<(Rectangle, IComposer)> composers) {
        DeviceId = deviceId;
        Composers = composers;
    }

    public string DeviceId { get; }
    private readonly int ImageWidth = 800;
    private readonly int ImageHeight = 480;
    private readonly IEnumerable<(Rectangle, IComposer)> Composers;


    public byte[]? GetImageBytes(DateTime time) {
        using var bitmap = new SKBitmap(ImageWidth, ImageHeight);
        Draw(bitmap, time.ToLocalTime());
        return Get1BPPImageBytes(bitmap);
    }

    private static SKFontManager FontManager = SKFontManager.Default;

    private void Draw(SKBitmap bitmap, DateTime time) {
        var background = SKColors.White;
        var foreground = SKColors.Black;

        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint() { Color = background };
        canvas.Clear(foreground);

        var font = FontManager.MatchFamily("Arial").ToFont(42);
        font.Edging = SKFontEdging.Alias;
        font.Hinting = SKFontHinting.Normal;

        foreach (var (rect, composer) in Composers) {
            using var subBitmap = new SKBitmap(rect.Width, rect.Height);
            composer.Draw(subBitmap, background, foreground, time);
            canvas.DrawBitmap(subBitmap, new SKPoint(rect.Location.X, rect.Location.Y));
        }

        var x = ImageWidth / 2;
        var y = ImageHeight / 2 - (font.Metrics.Ascent + font.Metrics.Descent) / 2;
        canvas.DrawText("Hello World!", x, y, SKTextAlign.Center, font, paint);
    }

    private byte[] Get1BPPImageBytes(SKBitmap bitmap) {
        int width = bitmap.Width;
        int height = bitmap.Height;

        int rowSize = ((width + 31) / 32) * 4; // 32-bit aligned
        int imageSize = rowSize * height;
        int fileSize = 62 + imageSize; // Header + data

        var buffer = new byte[fileSize];

        // BMP Header (14 bytes @ 0)
        buffer[0] = (byte)'B';
        buffer[1] = (byte)'M';
        buffer[2] = (byte)(fileSize);  // little-endian file size
        buffer[3] = (byte)(fileSize >> 8);
        buffer[4] = (byte)(fileSize >> 16);
        buffer[5] = (byte)(fileSize >> 24);
        buffer[10] = 62;   // little-endian pixel data offset (14 + 40 + 8)
        buffer[11] = 0;
        buffer[12] = 0;
        buffer[13] = 0;

        // DIB Header (40 bytes @ 14)
        buffer[14] = 40;   // little-endian header size
        buffer[15] = 0;
        buffer[16] = 0;
        buffer[17] = 0;
        buffer[18] = (byte)(width);  // little-endian width
        buffer[19] = (byte)(width >> 8);
        buffer[20] = (byte)(width >> 16);
        buffer[21] = (byte)(width >> 24);
        buffer[22] = (byte)(height);  // little-endian height
        buffer[23] = (byte)(height >> 8);
        buffer[24] = (byte)(height >> 16);
        buffer[25] = (byte)(height >> 24);
        buffer[26] = 1;   // little-endian plane count
        buffer[27] = 0;
        buffer[28] = 1;   // little-endian bits per pixel
        buffer[29] = 0;
        buffer[30] = 0;   // little-endian compression (BI_RGB)
        buffer[31] = 0;
        buffer[32] = 0;
        buffer[33] = 0;
        buffer[34] = (byte)(imageSize);  // little-endian image size
        buffer[35] = (byte)(imageSize >> 8);
        buffer[36] = (byte)(imageSize >> 16);
        buffer[37] = (byte)(imageSize >> 24);
        buffer[46] = 2;   // little-endian colors count
        buffer[47] = 0;
        buffer[48] = 0;
        buffer[49] = 0;

        // Color Palette (8 bytes @ 54)
        buffer[58] = 255; // White
        buffer[59] = 255;
        buffer[60] = 255;
        buffer[61] = 0;

        // Pixel Data (@ 62)
        var i = 62;
        for (int y = height - 1; y >= 0; y--) {
            for (int x = 0; x < width; x++) {
                var color = bitmap.GetPixel(x, y);
                var gray = 0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue;
                int bitIndex = x % 8;
                if (gray >= 128) {  // only update if pixel is white
                    buffer[i + x / 8] |= (byte)(0x80 >> bitIndex);
                }
            }
            i += rowSize;
        }

        return buffer;
    }

}
