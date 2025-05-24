namespace LocalPaper;

using System;
using SkiaSharp;

internal interface IComposer {

    public void Draw(SKBitmap bitmap, SKColor background, SKColor foreground, DateTime time);

}
