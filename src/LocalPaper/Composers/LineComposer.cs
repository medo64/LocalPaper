namespace LocalPaper;

using System;
using SkiaSharp;

internal class LineComposer : IComposer {


    public LineComposer(int thickness) {
        Thickness = thickness;
    }

    private readonly int Thickness;


    #region IComposer

    public void Draw(SKBitmap bitmap, SKColor color, DateTime time) {
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint() { Color = color, StrokeWidth = Thickness };
        canvas.DrawLine(0, 0, bitmap.Width - 1, bitmap.Height - 1, paint);
    }

    #endregion IComposer

}
