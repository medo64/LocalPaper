namespace LocalPaper;

using System;
using SkiaSharp;

internal sealed class TimeComposer : IComposer {


    public TimeComposer(string textFormat, SKTextAlign textAlign) {
        TextFormat = textFormat;
        TextAlign = textAlign;
    }


    private readonly string TextFormat;
    private readonly SKTextAlign TextAlign;


    #region IComposer

    public void Draw(SKBitmap bitmap, SKRect clipRect, StyleBag style, DateTime time) {
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint() { Color = style.Color };

        var margin = 8;
        using var font = style.GetFont(bitmap.Height - margin * 2);

        var centerY = bitmap.Height / 2 - (font.Metrics.Ascent + font.Metrics.Descent) / 2;
        switch (TextAlign) {
            case SKTextAlign.Left:
                canvas.DrawText(time.ToString(TextFormat), margin * 1.5f, centerY, SKTextAlign.Left, font, paint);
                break;
            case SKTextAlign.Right:
                canvas.DrawText(time.ToString(TextFormat), bitmap.Width - margin * 1.5f, centerY, SKTextAlign.Right, font, paint);
                break;
            default:
                var centerX = bitmap.Width / 2;
                canvas.DrawText(time.ToString(TextFormat), centerX, centerY, SKTextAlign.Center, font, paint);
                break;
        }

    }

    #endregion IComposer

}
