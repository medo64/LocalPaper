namespace LocalPaper;

using System;
using System.Drawing;
using SkiaSharp;

internal interface IComposer {

    public void Draw(SKBitmap bitmap, SKColor color, DateTime time);

}
