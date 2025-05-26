namespace LocalPaper;

using System;
using SkiaSharp;

internal sealed record StyleBag : IDisposable {

    public StyleBag() : this(SKColors.Black, "DejaVu Sans") {
    }

    public StyleBag(SKColor color, string fontName) {
        Color = color;
        NormalFamily = FontManager.MatchFamily(fontName, SKFontStyle.Normal);
        BoldFamily = FontManager.MatchFamily(fontName, SKFontStyle.Bold);
        ItalicFamily = FontManager.MatchFamily(fontName, SKFontStyle.Italic);
        if (NormalFamily == null || BoldFamily == null || ItalicFamily == null) {
            throw new InvalidOperationException($"Font '{fontName}' not found");
        }

    }

    private readonly SKTypeface NormalFamily;
    private readonly SKTypeface BoldFamily;
    private readonly SKTypeface ItalicFamily;


    public SKColor Color { get; }

    public SKFont GetFont(int fontSize = 20) {
        return GetFont(NormalFamily, fontSize);
    }

    public SKFont GetBoldFont(int fontSize = 20) {
        return GetFont(BoldFamily, fontSize);
    }

    public SKFont GetItalicFont(int fontSize = 20) {
        return GetFont(ItalicFamily, fontSize);
    }

    private static SKFont GetFont(SKTypeface fontFamily, int fontSize) {
        return new SKFont(fontFamily) {
            Size = fontSize,
            Edging = SKFontEdging.Alias,
            Hinting = SKFontHinting.Normal,
        };
    }


    public void Dispose() {
        NormalFamily?.Dispose();
        BoldFamily?.Dispose();
        ItalicFamily?.Dispose();
    }


    private static SKFontManager FontManager = SKFontManager.Default;

}
