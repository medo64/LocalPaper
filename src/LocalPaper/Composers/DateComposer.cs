namespace LocalPaper;

using System;
using System.Drawing;
using System.IO;
using SkiaSharp;

internal class DateComposer : IComposer {


    public DateComposer(string formatLeft, string formatCenter, string formatRight) {
        FormatLeft = formatLeft;
        FormatCenter = formatCenter;
        FormatRight = formatRight;
    }


    private readonly string FormatLeft;
    private readonly string FormatCenter;
    private readonly string FormatRight;

    private static SKFontManager FontManager = SKFontManager.Default;


    public void Draw(SKBitmap bitmap, SKColor background, SKColor foreground, DateTime time) {
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint() { Color = foreground, };
        canvas.Clear(background);

        var margin = 8;

        var font = FontManager.MatchFamily("DejaVu Sans").ToFont(bitmap.Height - margin * 2);
        font.Edging = SKFontEdging.Alias;
        font.Hinting = SKFontHinting.Normal;

        var centerY = bitmap.Height / 2 - (font.Metrics.Ascent + font.Metrics.Descent) / 2;
        var centerX = bitmap.Width / 2;

        canvas.DrawText(time.ToString(FormatLeft), margin * 1.5f, centerY, SKTextAlign.Left, font, paint);
        canvas.DrawText(time.ToString(FormatRight), bitmap.Width - margin * 1.5f, centerY, SKTextAlign.Right, font, paint);
        canvas.DrawText(time.ToString(FormatCenter), centerX, centerY, SKTextAlign.Center, font, paint);
    }

}
