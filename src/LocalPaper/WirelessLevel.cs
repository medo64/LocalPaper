namespace LocalPaper;

using System;

internal sealed record WirelessLevel {

    public WirelessLevel()
        : this(default(int?)) { }

    public WirelessLevel(int? rssi) {
        if (rssi is not null) {
            Rssi = rssi;
            Percentage = (int)Math.Min(Math.Max(Math.Round(100.0 * (rssi.Value - MinRssi) / (MaxRssi - MinRssi), 0), 0), 100);
        } else {
            Rssi = null;
            Percentage = null;
        }
    }


    public int? Rssi { get; init; }
    public int? Percentage { get; init; }

    private static readonly int MinRssi = -90;
    private static readonly int MaxRssi = -30;

}
