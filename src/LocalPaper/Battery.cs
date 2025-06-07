namespace LocalPaper;

using System;
using System.Collections.Generic;

internal static class Battery {

    private static readonly Dictionary<string, int> BatteryPercentages = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, float> BatteryVoltages = new(StringComparer.OrdinalIgnoreCase);

    public static int RecordVoltage(string device, float voltage) {
        var percentage = (int)Math.Min(Math.Max(Math.Round((voltage - 3.2) * 100, 0), 0), 100);  // assume linear from 4.2V to 3.2V
        BatteryPercentages[device] = percentage;
        BatteryVoltages[device] = voltage;
        return percentage;
    }

    public static int? GetPercentage(string device) {
        if (BatteryPercentages.TryGetValue(device, out var percentage)) {
            return percentage;
        }
        return null;
    }

    public static float? GetVoltage(string device) {
        if (BatteryVoltages.TryGetValue(device, out var voltage)) {
            return voltage;
        }
        return null;
    }

}
