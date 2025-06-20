using System.Collections;

namespace LocalPaper;

using System;
using System.Globalization;
using System.Runtime.Serialization;
using SkiaSharp;

internal sealed class BatteryComposer : IComposer {


    public BatteryComposer(int showIfBelow, SKTextAlign hAlign) {
        ShowIfBelow = showIfBelow;
        HAlign = hAlign;
    }


    private readonly int ShowIfBelow;
    private readonly SKTextAlign HAlign;


    #region IComposer

    public void Draw(SKBitmap bitmap, SKRect clipRect, StyleBag style, DataBag data) {
        using var canvas = new SKCanvas(bitmap);
        using var paint = new SKPaint() { Color = style.Color };

        var top = bitmap.Height / 2 - 8;
        var left = HAlign switch {
            SKTextAlign.Left => 0,
            SKTextAlign.Right => bitmap.Width - 8,
            _ => bitmap.Width / 2 - 4,
        };
        DrawBattery(data.BatteryLevel, canvas, paint, top, left);

        var showText = (HAlign != SKTextAlign.Center);
        if (showText) {
            string text = string.Empty;
            if (data.BatteryLevel.IsCharging) {
                text = "Charging";
            } else if ((data.BatteryLevel.Percentage is not null) && (data.BatteryLevel.Percentage < ShowIfBelow)) {
                text = data.BatteryLevel.Percentage.Value.ToString("0'%'", CultureInfo.InvariantCulture);
            }
            using var font = style.GetFont(16);
            var textMargin = clipRect.Left / 2;
            var centerY = bitmap.Height / 2 - (font.Metrics.Descent + font.Metrics.Ascent) / 2;

            canvas.ClipRect(clipRect);
            switch (HAlign) {
                case SKTextAlign.Left:
                    canvas.DrawText(text, 8 + textMargin, centerY, SKTextAlign.Left, font, paint);
                    break;
                case SKTextAlign.Right:
                    canvas.DrawText(text, bitmap.Width - 8 - textMargin, centerY, SKTextAlign.Right, font, paint);
                    break;
            }
        }
    }

    private void DrawBattery(BatteryLevel level, SKCanvas canvas, SKPaint paint, int imageTop, int imageLeft) {
        if (level.IsCharging) {

            // battery body with arrow
            canvas.DrawPoint(imageLeft + 2, imageTop + 1, paint);
            canvas.DrawPoint(imageLeft + 3, imageTop + 1, paint);
            canvas.DrawPoint(imageLeft + 4, imageTop + 1, paint);
            for (var i = 0; i <= 6; i++) {
                canvas.DrawPoint(imageLeft + i, imageTop + 2, paint);
                canvas.DrawPoint(imageLeft + i, imageTop + 13, paint);
            }
            for (var j = 2; j <= 12; j++) {
                for (var i = 0; i <= 6; i++) {
                    if ((i == 3) && (j is >= 6 and <= 10)) { continue; }
                    if ((i is >= 1 and <= 5) && (j == 8)) { continue; }
                    canvas.DrawPoint(imageLeft + i, imageTop + j, paint);
                }
            }

        } else if (level.Percentage == 0) {

            // battery body
            canvas.DrawPoint(imageLeft + 2, imageTop + 1, paint);
            canvas.DrawPoint(imageLeft + 3, imageTop + 1, paint);
            canvas.DrawPoint(imageLeft + 4, imageTop + 1, paint);
            for (var i = 0; i <= 6; i++) {
                canvas.DrawPoint(imageLeft + i, imageTop + 2, paint);
                canvas.DrawPoint(imageLeft + i, imageTop + 13, paint);
            }
            for (var j = 0; j < 3; j++) {
                canvas.DrawPoint(imageLeft + 0, imageTop + 3 + j, paint);
                canvas.DrawPoint(imageLeft + 6, imageTop + 3 + j, paint);
                canvas.DrawPoint(imageLeft + 0, imageTop + 12 - j, paint);
                canvas.DrawPoint(imageLeft + 6, imageTop + 12 - j, paint);
            }

            // exclamation point
            canvas.DrawPoint(imageLeft + 3, imageTop + 5, paint);
            canvas.DrawPoint(imageLeft + 3, imageTop + 6, paint);
            canvas.DrawPoint(imageLeft + 3, imageTop + 7, paint);
            canvas.DrawPoint(imageLeft + 3, imageTop + 8, paint);
            canvas.DrawPoint(imageLeft + 3, imageTop + 10, paint);

        } else if (level.Percentage < ShowIfBelow) {

            // battery body
            canvas.DrawPoint(imageLeft + 2, imageTop + 1, paint);
            canvas.DrawPoint(imageLeft + 3, imageTop + 1, paint);
            canvas.DrawPoint(imageLeft + 4, imageTop + 1, paint);
            for (var i = 0; i <= 6; i++) {
                canvas.DrawPoint(imageLeft + i, imageTop + 2, paint);
                canvas.DrawPoint(imageLeft + i, imageTop + 13, paint);
            }
            for (var j = 2; j <= 12; j++) {
                canvas.DrawPoint(imageLeft + 0, imageTop + j, paint);
                canvas.DrawPoint(imageLeft + 6, imageTop + j, paint);
            }

            // battery fill
            var percentHeight = (int)Math.Ceiling(level.Percentage.Value / 10.0);
            for (var j = 13 - percentHeight; j <= 12; j++) {
                canvas.DrawPoint(imageLeft + 1, imageTop + j, paint);
                canvas.DrawPoint(imageLeft + 2, imageTop + j, paint);
                canvas.DrawPoint(imageLeft + 3, imageTop + j, paint);
                canvas.DrawPoint(imageLeft + 4, imageTop + j, paint);
                canvas.DrawPoint(imageLeft + 5, imageTop + j, paint);
            }

        }
    }

    #endregion IComposer

}
