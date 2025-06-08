namespace LocalPaper;

using System;

internal sealed record BatteryLevel {

    public BatteryLevel()
        : this(default(double?)) { }

    public BatteryLevel(double? voltage) {
#if DEBUG
        voltage ??= 4.6;  // just for testing
#endif

        if (voltage is not null) {
            Voltage = voltage;
            if (voltage > 4.5) {  // charging
                Percentage = null;
                IsCharging = true;
            } else {
                Percentage = (int)Math.Min(Math.Max(Math.Ceiling(100.0 * (voltage.Value - MinVoltage) / (MaxVoltage - MinVoltage)), 0), 100);
                IsCharging = false;
            }
        } else {
            Voltage = null;
            Percentage = null;
            IsCharging = false;
        }
    }


    public double? Voltage { get; init; }
    public int? Percentage { get; init; }
    public bool IsCharging { get; init; }


    private static readonly double MinVoltage = 3.05;
    private static readonly double MaxVoltage = 4.05;

}
