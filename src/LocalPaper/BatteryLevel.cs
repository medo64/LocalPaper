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
                Percentage = (int)Math.Min(Math.Max(Math.Round((voltage.Value - 3.2) * 100, 0), 0), 100);  // assume linear from 4.2V to 3.2V
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

}
