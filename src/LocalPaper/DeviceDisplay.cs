namespace LocalPaper;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using SkiaSharp;

internal class DeviceDisplay {
    public DeviceDisplay(string deviceId, TimeSpan interval, int imageWidth, int imageHeight, bool isInverted, TimeZoneInfo timeZone, IEnumerable<ComposerBag> composers) {
        DeviceId = deviceId;
        Interval = interval;
        ImageWidth = imageWidth;
        ImageHeight = imageHeight;
        IsInverted = isInverted;
        TimeZone = timeZone;
        Composers = composers;
    }

    public string DeviceId { get; }
    public TimeSpan Interval { get; }

    private readonly int ImageWidth;
    private readonly int ImageHeight;
    private readonly bool IsInverted;
    private readonly TimeZoneInfo TimeZone;
    private readonly IEnumerable<ComposerBag> Composers;



    public byte[]? GetImageBytes(DateTime time) {
        using var bitmap = new SKBitmap(ImageWidth, ImageHeight);
        Draw(bitmap, TimeZoneInfo.ConvertTime(time, TimeZone));
        return Get1BPPImageBytes(bitmap);
    }

    private void Draw(SKBitmap bitmap, DateTime time) {
        var displayBackground = IsInverted ? SKColors.Black : SKColors.White;

        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(displayBackground);

        foreach (var composerBag in Composers) {
            using var subBitmap = new SKBitmap(composerBag.Rectangle.Width, composerBag.Rectangle.Height);
            var margin = 4;
            var innerLeft = composerBag.Rectangle.Width >= margin * 4 ? margin : 0;
            var innerWidth = composerBag.Rectangle.Width >= margin * 4 ? composerBag.Rectangle.Width - margin * 2 : composerBag.Rectangle.Width;
            var innerTop = composerBag.Rectangle.Height >= margin * 4 ? margin : 0;
            var innerHeight = composerBag.Rectangle.Height >= margin * 4 ? composerBag.Rectangle.Height - margin * 2 : composerBag.Rectangle.Height;
            var rect = new SKRect(innerLeft, innerTop, innerWidth, innerHeight
            );
            using var style = new StyleBag(
                composerBag.IsInverted ? SKColors.White : SKColors.Black,
                "DejaVu Sans"
                //"Roboto Condensed"
            );
            composerBag.Composer.Draw(subBitmap, rect, style, time.AddSeconds(composerBag.Offset.TotalSeconds));

            canvas.DrawBitmap(subBitmap, new SKPoint(composerBag.Rectangle.Left, composerBag.Rectangle.Top));
        }
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
