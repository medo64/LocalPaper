namespace LocalPaper;

using System;
using System.IO;
using SkiaSharp;

internal sealed class EventsComposer : IComposer {

    public EventsComposer(DirectoryInfo directory, TimeSpan offset) {
        Directory = directory;
        Offset = offset;
    }

    private readonly DirectoryInfo Directory;
    private readonly TimeSpan Offset;


    #region IComposer

    public void Draw(SKBitmap bitmap, SKRect clipRect, StyleBag style, DateTime time) {
        using var canvas = new SKCanvas(bitmap);
        canvas.ClipRect(clipRect);
        using var paint = new SKPaint() { Color = style.Color };

        using var font = style.GetFont();
        using var fontBold = style.GetBoldFont();

        var y = clipRect.Top - font.Metrics.Ascent;
        var lastKey = string.Empty;
        foreach (var kvp in Helpers.GetConfigEntries(Directory, DateOnly.FromDateTime(time.AddSeconds(Offset.TotalSeconds)))) {
            if (!kvp.Key.Equals(lastKey)) {
                if (!string.IsNullOrEmpty(lastKey)) { y += clipRect.Top; }
                canvas.DrawText(kvp.Key, clipRect.Left, y, SKTextAlign.Left, fontBold, paint);
                y += font.Metrics.Descent - font.Metrics.Ascent;
                lastKey = kvp.Key;
            }
            canvas.DrawText(kvp.Value, clipRect.Left, y, SKTextAlign.Left, font, paint);
            y += font.Metrics.Descent - font.Metrics.Ascent;
        }
    }

    #endregion IComposer

}
