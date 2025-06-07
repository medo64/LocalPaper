namespace LocalPaper;

using System;
using System.IO;
using SkiaSharp;

internal sealed class EventsComposer : IComposer {

    public EventsComposer(DirectoryInfo directory) {
        Directory = directory;
    }

    private readonly DirectoryInfo Directory;


    #region IComposer

    public void Draw(SKBitmap bitmap, SKRect clipRect, StyleBag style, DataBag data) {
        using var canvas = new SKCanvas(bitmap);
        canvas.ClipRect(clipRect);
        using var paint = new SKPaint() { Color = style.Color };

        using var fontHead = style.GetFont();
        using var fontText = style.GetBoldFont();

        var y = (int)(clipRect.Top - fontText.Metrics.Ascent);
        var lastKey = string.Empty;
        var lastValue = string.Empty;
        foreach (var kvp in Helpers.GetConfigEntries(Directory, DateOnly.FromDateTime(data.LocalTime))) {
            if (!kvp.Key.Equals(lastKey, StringComparison.Ordinal)) {
                if (!string.IsNullOrEmpty(lastKey)) { y += (int)clipRect.Top; }
                canvas.DrawText(kvp.Key, clipRect.Left, y, SKTextAlign.Left, fontHead, paint);
                y += (int)(fontText.Metrics.Descent - fontText.Metrics.Ascent);
                lastKey = kvp.Key;
                lastValue = string.Empty;
            }

            if (kvp.Value.Equals(lastValue, StringComparison.Ordinal)) { continue; }  // skip if same
            canvas.DrawText(kvp.Value, clipRect.Left, y, SKTextAlign.Left, fontText, paint);
            y += (int)(fontText.Metrics.Descent - fontText.Metrics.Ascent);
            lastValue = kvp.Value;
        }
    }

    #endregion IComposer

}
