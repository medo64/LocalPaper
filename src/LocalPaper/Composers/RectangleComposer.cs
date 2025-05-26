namespace LocalPaper;

using System;
using SkiaSharp;

internal sealed class RectangleComposer : IComposer {

    public RectangleComposer() {
    }


    #region IComposer

    public void Draw(SKBitmap bitmap, SKRect clipRect, StyleBag style, DateTime time) {
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(style.Color);
    }

    #endregion IComposer

}
