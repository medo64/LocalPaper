namespace LocalPaper;

using System;
using SkiaSharp;

internal sealed class TimeComposer : IComposer {


    public TimeComposer(string format, SKTextAlign hAlign, VerticalAlignment vAlign) {
        Format = format;
        HAlign = hAlign;
        VAlign = vAlign;
    }


    private readonly string Format;
    private readonly SKTextAlign HAlign;
    private readonly VerticalAlignment VAlign;


    #region IComposer

    public void Draw(SKBitmap bitmap, SKRect clipRect, StyleBag style, DataBag data) {
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint() { Color = style.Color };

        var margin = 8;
        using var font = style.GetFont(bitmap.Height - margin * 2);

        var centerY = VAlign switch {
            VerticalAlignment.Top => clipRect.Top + (font.Metrics.Descent - font.Metrics.Ascent),
            VerticalAlignment.Bottom => clipRect.Bottom - font.Metrics.Descent,
            _ => bitmap.Height / 2 - (font.Metrics.Descent + font.Metrics.Ascent) / 2,
        };

        switch (HAlign) {
            case SKTextAlign.Left:
                canvas.DrawText(data.LocalTime.ToString(Format), margin * 1.5f, centerY, SKTextAlign.Left, font, paint);
                break;
            case SKTextAlign.Right:
                canvas.DrawText(data.LocalTime.ToString(Format), bitmap.Width - margin * 1.5f, centerY, SKTextAlign.Right, font, paint);
                break;
            default:
                var centerX = bitmap.Width / 2;
                canvas.DrawText(data.LocalTime.ToString(Format), centerX, centerY, SKTextAlign.Center, font, paint);
                break;
        }

    }

    #endregion IComposer

}
