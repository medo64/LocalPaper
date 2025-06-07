using System.Collections;

namespace LocalPaper;

using System;
using SkiaSharp;

internal sealed class TextComposer : IComposer {


    public TextComposer(string text, SKTextAlign hAlign, VerticalAlignment vAlign) {
        Text = text;
        HAlign = hAlign;
        VAlign = vAlign;
    }


    private readonly string Text;
    private readonly SKTextAlign HAlign;
    private readonly VerticalAlignment VAlign;


    #region IComposer

    public void Draw(SKBitmap bitmap, SKRect clipRect, StyleBag style, DataBag _) {
        using var canvas = new SKCanvas(bitmap);
        canvas.ClipRect(clipRect);
        using var paint = new SKPaint() { Color = style.Color };

        using var font = style.GetFont();
        var margin = clipRect.Left;

        var centerY = VAlign switch {
            VerticalAlignment.Top => clipRect.Top + (font.Metrics.Descent - font.Metrics.Ascent),
            VerticalAlignment.Bottom => clipRect.Bottom - font.Metrics.Descent,
            _ => bitmap.Height / 2 - (font.Metrics.Descent + font.Metrics.Ascent) / 2,
        };

        switch (HAlign) {
            case SKTextAlign.Left:
                canvas.DrawText(Text, margin, centerY, SKTextAlign.Left, font, paint);
                break;
            case SKTextAlign.Right:
                canvas.DrawText(Text, bitmap.Width - margin, centerY, SKTextAlign.Right, font, paint);
                break;
            default:
                var centerX = bitmap.Width / 2;
                canvas.DrawText(Text, centerX, centerY, SKTextAlign.Center, font, paint);
                break;
        }

    }

    #endregion IComposer

}
