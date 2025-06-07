namespace LocalPaper;

using System;
using SkiaSharp;

internal sealed record DataBag {

    public DataBag() : this(DateTime.UtcNow, TimeZoneInfo.Utc, null, null) {
    }

    public DataBag(DateTime utcTime, TimeZoneInfo timeZone, float? batteryVoltage, int? batteryPercentage) {
        UtcTime = utcTime;
        TimeZone = timeZone;
        BatteryVoltage = batteryVoltage;
        BatteryPercentage = batteryPercentage;
    }

    public DateTime UtcTime { get; init; }
    public TimeZoneInfo TimeZone { get; init; }
    public DateTime LocalTime => TimeZoneInfo.ConvertTime(UtcTime, TimeZone);

    public float? BatteryVoltage { get; init; }
    public int? BatteryPercentage { get; init; }

}
