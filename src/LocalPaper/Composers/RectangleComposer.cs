namespace LocalPaper;

using System;
using SkiaSharp;

internal class RectangleComposer : IComposer {

    public RectangleComposer() {
    }


    #region IComposer

    public void Draw(SKBitmap bitmap, SKColor color, DateTime time) {
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(color);
    }

    #endregion IComposer

}
