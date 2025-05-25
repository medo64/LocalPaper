namespace LocalPaper;

using System;
using System.Drawing;
using SkiaSharp;

internal class TimeComposer : IComposer {


    public TimeComposer(string textFormat, SKTextAlign textAlign) {
        TextFormat = textFormat;
        TextAlign = textAlign;
    }


    private readonly string TextFormat;
    private readonly SKTextAlign TextAlign;

    private static SKFontManager FontManager = SKFontManager.Default;


    #region IComposer

    public Rectangle Rectangle { get; }

    public void Draw(SKBitmap bitmap, SKColor color, DateTime time) {
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint() { Color = color };

        var margin = 8;

        var font = FontManager.MatchFamily("DejaVu Sans").ToFont(bitmap.Height - margin * 2);
        font.Edging = SKFontEdging.Alias;
        font.Hinting = SKFontHinting.Normal;

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
