namespace LocalPaper;

using System;
using SkiaSharp;

internal sealed class LineComposer : IComposer {


    public LineComposer(int thickness) {
        Thickness = thickness;
    }

    private readonly int Thickness;


    #region IComposer

    public void Draw(SKBitmap bitmap, SKRect clipRect, StyleBag style, DateTime time) {
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint() { Color = style.Color, StrokeWidth = Thickness };
        canvas.DrawLine(0, 0, bitmap.Width - 1, bitmap.Height - 1, paint);
    }

    #endregion IComposer

}
