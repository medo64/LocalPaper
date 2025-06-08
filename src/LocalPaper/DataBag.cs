namespace LocalPaper;

using System;
using SkiaSharp;

internal sealed record DataBag {

    public DataBag() : this(string.Empty, DateTime.UtcNow, TimeZoneInfo.Utc, new BatteryLevel(), new WirelessLevel()) {
    }

    public DataBag(string displayId, DateTime utcTime, TimeZoneInfo timeZone, BatteryLevel batteryLevel, WirelessLevel wirelessLevel) {
        DisplayId = displayId;
        UtcTime = utcTime;
        TimeZone = timeZone;
        BatteryLevel = batteryLevel;
        WirelessLevel = wirelessLevel;
    }


    public string DisplayId { get; init; }

    public DateTime UtcTime { get; init; }
    public TimeZoneInfo TimeZone { get; init; }
    public DateTime LocalTime => TimeZoneInfo.ConvertTime(UtcTime, TimeZone);

    public BatteryLevel BatteryLevel { get; init; }
    public WirelessLevel WirelessLevel { get; init; }

}
