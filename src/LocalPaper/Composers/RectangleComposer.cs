namespace LocalPaper;

using System;
using SkiaSharp;

internal sealed class RectangleComposer : IComposer {

    public RectangleComposer(int thickness, bool fill = true) {
        Thickness = thickness;
        Fill = fill;
    }

    private readonly int Thickness;
    private readonly bool Fill;


    #region IComposer

    public void Draw(SKBitmap bitmap, SKRect clipRect, StyleBag style, DataBag _) {
        using var canvas = new SKCanvas(bitmap);
        if (Fill) {
            canvas.Clear(style.Color);
        } else {
            using var paint = new SKPaint { Color = style.Color, Style = SKPaintStyle.Stroke, StrokeWidth = Thickness };
            canvas.DrawRect(new SKRect(0, 0, bitmap.Width - Thickness, bitmap.Height - Thickness), paint);
        }
    }

    #endregion IComposer

}
