namespace LocalPaper;

using System;
using SkiaSharp;

internal interface IComposer {

    public void Draw(SKBitmap bitmap, SKRect clipRect, StyleBag style, DateTime time);

}
