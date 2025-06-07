namespace LocalPaper;

using System;
using SkiaSharp;

internal sealed record DataBag {

    public DataBag() : this(string.Empty, DateTime.UtcNow, TimeZoneInfo.Utc, null, null, null, null) {
    }

    public DataBag(string displayId, DateTime utcTime, TimeZoneInfo timeZone, float? batteryVoltage, int? batteryPercentage, int? wirelessRssi, int? wirelessPercentage) {
        DisplayId = displayId;
        UtcTime = utcTime;
        TimeZone = timeZone;
        BatteryVoltage = batteryVoltage;
        BatteryPercentage = batteryPercentage;
        WirelessRssi = wirelessRssi;
        WirelessPercentage = wirelessPercentage;
    }


    public string DisplayId { get; init; }

    public DateTime UtcTime { get; init; }
    public TimeZoneInfo TimeZone { get; init; }
    public DateTime LocalTime => TimeZoneInfo.ConvertTime(UtcTime, TimeZone);

    public float? BatteryVoltage { get; init; }
    public int? BatteryPercentage { get; init; }

    public int? WirelessRssi { get; init; }
    public int? WirelessPercentage { get; init; }

}
