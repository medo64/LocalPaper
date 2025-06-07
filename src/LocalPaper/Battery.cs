namespace LocalPaper;

using System;
using System.Collections.Generic;

internal static class Battery {

    private static readonly Dictionary<string, int> BatteryPercentages = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, float> BatteryVoltages = new(StringComparer.OrdinalIgnoreCase);

    public static int RecordVoltage(string displayId, float voltage) {
        var percentage = (int)Math.Min(Math.Max(Math.Round((voltage - 3.2) * 100, 0), 0), 100);  // assume linear from 4.2V to 3.2V
        Log.Verbose($"Recorded voltage for {displayId} ({voltage:0.00}V, {percentage}%)");

        BatteryPercentages[displayId] = percentage;
        BatteryVoltages[displayId] = voltage;
        return percentage;
    }

    public static int? GetPercentage(string displayId) {
        if (BatteryPercentages.TryGetValue(displayId, out var percentage)) {
            return percentage;
        }
        return null;
    }

    public static float? GetVoltage(string displayId) {
        if (BatteryVoltages.TryGetValue(displayId, out var voltage)) {
            return voltage;
        }
        return null;
    }

}
